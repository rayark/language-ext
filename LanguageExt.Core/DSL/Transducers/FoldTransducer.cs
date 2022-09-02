#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record FoldUntilTransducer<ST, A>(
    ST State, 
    Func<ST, A, ST> Fold,
    Func<ST, bool> Predicate
    ) : Transducer<A, ST>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, ST, TResult<S>> reducer)
    {
        var nstate = State;
        return (state, value) =>
        {
            nstate = Fold(nstate, value);
            return Predicate(nstate) ? TResult.Complete(state.Value) : reducer(state, nstate);  
        };
    }
}

internal sealed record FoldWhileTransducer<ST, A>(
    ST State, 
    Func<ST, A, ST> Fold,
    Func<ST, bool> Predicate
    ) : Transducer<A, ST>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, ST, TResult<S>> reducer)
    {
        var nstate = State;
        return (state, value) =>
        {
            nstate = Fold(nstate, value);
            return Predicate(nstate) ? reducer(state, nstate) : TResult.Complete(state.Value);  
        };
    }
}

internal sealed record FoldTransducer<ST, A>(
    ST State, 
    Func<ST, A, ST> Fold
    ) : Transducer<A, ST>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, ST, TResult<S>> reducer)
    {
        var nstate = State;
        return (state, value) =>
        {
            nstate = Fold(nstate, value);
            return reducer(state, nstate);  
        };
    }
}
