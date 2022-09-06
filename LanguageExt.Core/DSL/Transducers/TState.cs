#nullable enable
using System;
using System.Collections.Concurrent;
using System.Threading;
using LanguageExt.DSL.Transducers;

namespace LanguageExt.DSL.Transducers;

public record TState<S>(S Value, object? This)
{
    int resource;
    ConcurrentDictionary<object, IDisposable>? disps;

    public static TState<S> Create(S value) =>
        new(value, null);

    TState(ConcurrentDictionary<object, IDisposable>? disps, S value, object? @this) : this(value, @this) =>
        this.disps = disps;

    public TState<S> Scope() =>
        new(null, Value, This);

    public TState<T> SetValue<T>(T value) =>
        new (disps, value, This);

    public TState<S> SetValue(TResult<S> value) =>
        value.Continue
            ? new(disps, value.ValueUnsafe, This)
            : this;

    public TState<T> SetValue<T>(TResult<T> value) =>
        new(disps, value.ValueUnsafe, This);

    public TState<S> SetThis(object @this) =>
        new(disps, Value, @this);

    public TState<S> LocalResources() =>
        new(null, Value, This);

    public Unit Use<A>(A key, IDisposable d)
    {
        SpinWait sw = default;
        while (true)
        {
            if (Interlocked.CompareExchange(ref resource, 1, 0) == 0)
            {
                disps = disps ?? new ConcurrentDictionary<object, IDisposable>();
                disps.TryAdd(key, d);
                resource = 0;
                return default;
            }

            sw.SpinOnce();
        }
    }

    public Unit Release<A>(A key)
    {
        SpinWait sw = default;
        while (true)
        {
            if (Interlocked.CompareExchange(ref resource, 1, 0) == 0)
            {
                disps = disps ?? new ConcurrentDictionary<object, IDisposable>();
                disps.TryRemove(key, out var d);
                d?.Dispose();
                resource = 0;
                return default;
            }

            sw.SpinOnce();
        }
    }

    public Unit CleanUp()
    {
        SpinWait sw = default;
        while (true)
        {
            if (Interlocked.CompareExchange(ref resource, 1, 0) == 0)
            {
                if (disps == null) return default;
                foreach (var disp in disps)
                {
                    disp.Value.Dispose();
                }

                disps.Clear();
                disps = null;
                resource = 0;
                return default;
            }

            sw.SpinOnce();
        }
    }

    public static implicit operator S(TState<S> state) =>
        state.Value;
}
