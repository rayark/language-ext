#nullable enable
using System;
using System.Collections.Concurrent;
using System.Threading;
using LanguageExt.TypeClasses;

namespace LanguageExt.DSL;

public static partial class DSL<MErr, E>
    where MErr : struct, Semigroup<E>, Convertable<Exception, E>
{
    public record State<RT>(RT Runtime, object This)
    {
        int resource;
        ConcurrentDictionary<object, IDisposable>? disps;

        public static State<RT> Create(RT runtime) =>
            new(null, runtime, Prim<Unit>.None);

        State(ConcurrentDictionary<object, IDisposable>? disps, RT runtime, object @this) : this(runtime, @this) =>
            this.disps = disps;

        public State<NRT> LocalRuntime<NRT>(Func<RT, NRT> f) =>
            new(disps, f(Runtime), This);

        public State<RT> SetThis<NEW_THIS>(Prim<NEW_THIS> @this) =>
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
}
