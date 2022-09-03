#nullable enable
using System;
using System.Collections.Generic;

namespace LanguageExt.DSL.Transducers;

internal sealed record EnumerableTransducer<A> : Transducer<IEnumerable<A>, A>
{
    public Func<TState<S>, IEnumerable<A>, TResult<S>> Transform<S>(Func<TState<S>, A, TResult<S>> reduce) =>
        (seed, values) =>
        {
            foreach (var value in values)
            {
                var res = reduce(seed, value);
                if (res.Faulted) return res;
                if (res.Complete) return TResult.Continue(seed.Value);
                seed = seed.SetValue(res);
            }
            return TResult.Continue(seed.Value);
        };
}
