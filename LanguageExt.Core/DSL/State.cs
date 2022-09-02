#nullable enable
using System;
using System.Collections.Concurrent;
using System.Threading;
using LanguageExt.DSL.Transducers;

namespace LanguageExt.DSL;

public record State<RT>(RT Runtime, object? This)
{
    int resource;
    ConcurrentDictionary<object, IDisposable>? disps;

    public static State<RT> Create(RT runtime) =>
        new(null, runtime, null);

    State(ConcurrentDictionary<object, IDisposable>? disps, RT runtime, object? @this) : this(runtime, @this) =>
        this.disps = disps;

    public State<NRT> LocalRuntime<NRT>(Func<RT, NRT> f) =>
        new(disps, f(Runtime), This);

    public State<RT> SetThis(object @this) =>
        new(disps, Runtime, @this);

    public State<RT> LocalResources() =>
        new(null, Runtime, This);

    public Unit Use(object key, IDisposable d)
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

    public Unit Release(object key)
    {
        SpinWait sw = default;
        while (true)
        {
            if (Interlocked.CompareExchange(ref resource, 1, 0) == 0)
            {
                disps = disps ?? new ConcurrentDictionary<object, IDisposable>();
                disps.TryRemove(key, out var d);
                d.Dispose();
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
}

public record TState<S>(S Value, object? This)
{
    int resource;
    ConcurrentDictionary<object, IDisposable>? disps;

    public static TState<S> Create(S value) =>
        new(value, null);

    TState(ConcurrentDictionary<object, IDisposable>? disps, S value, object? @this) : this(value, @this) =>
        this.disps = disps;

    public TState<S> SetValue(S value) =>
        new(disps, value, This);

    public TState<S> SetValue(TResult<S> value) =>
        value.Continue
            ? new(disps, value.ValueUnsafe, This)
            : this;

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
                d.Dispose();
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
