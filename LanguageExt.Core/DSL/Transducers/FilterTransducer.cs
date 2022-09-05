#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record FilterTransducer<A>(Func<A, bool> Predicate) : Transducer<A, A>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, A, TResult<S>> reducer) =>
        (state, value) => Predicate(value) ? reducer(state, value) : TResult.Continue(state.Value);
    
}
