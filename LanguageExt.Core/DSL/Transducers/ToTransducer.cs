#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record ToTransducer<M, A, B> : Transducer<M, Transducer<A, B>>
    where M : IsTransducer<A, B>
{
    public static readonly Transducer<M, Transducer<A, B>> Default = new ToTransducer<M, A, B>();
    
    public Func<TState<S>, M, TResult<S>> Transform<S>(Func<TState<S>, Transducer<A, B>, TResult<S>> reducer) => 
        (state, value) => reducer(state, value.ToTransducer());
}
