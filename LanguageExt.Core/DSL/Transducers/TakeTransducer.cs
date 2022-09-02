#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record TakeUntilTransducer<A>(Func<A, bool> Predicate) : Transducer<A, A>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, A, TResult<S>> reducer) =>
        (state, value) =>
            Predicate(value)
                ? TResult.Complete(state.Value)
                : reducer(state, value);
}

internal sealed record TakeWhileTransducer<A>(Func<A, bool> Predicate) : Transducer<A, A>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, A, TResult<S>> reducer) =>
        (state, value) =>
            Predicate(value)
                ? reducer(state, value)
                : TResult.Complete(state.Value);
}

internal sealed record TakeTransducer<A>(int Count) : Transducer<A, A>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, A, TResult<S>> reducer)
    {
        var remaining = Count;
        return (state, value) =>
        {
            if (remaining <= 0)
            {
                return TResult.Complete(state.Value);
            }
            remaining--;
            return reducer(state, value);
        };
    }
}
