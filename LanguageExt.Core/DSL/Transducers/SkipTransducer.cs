#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record SkipUntilTransducer<A>(Func<A, bool> Predicate) : Transducer<A, A>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, A, TResult<S>> reducer) =>
        (state, value) =>
            Predicate(value)
                ? reducer(state, value)
                : TResult.Continue(state.Value);
}

internal sealed record SkipWhileTransducer<A>(Func<A, bool> Predicate) : Transducer<A, A>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, A, TResult<S>> reducer) =>
        (state, value) =>
            Predicate(value)
                ? TResult.Continue(state.Value)
                : reducer(state, value);
}

internal sealed record SkipTransducer<A>(int Count) : Transducer<A, A>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, A, TResult<S>> reducer)
    {
        var remaining = Count;
        return (state, value) =>
        {
            if (remaining <= 0)
            {
                return reducer(state, value);
            }
            remaining--;
            return TResult.Continue(state.Value);
        };
    }
}
