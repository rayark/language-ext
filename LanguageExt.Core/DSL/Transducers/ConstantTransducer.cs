#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record ConstantTransducer<A, B>(B Constant) : Transducer<A, B>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, B, TResult<S>> reducer) =>
        (state, _) => reducer(state, Constant);
}

internal sealed record ConstantRightTransducer<X, A, B>(B Constant) : Transducer<X, CoProduct<A, B>>
{
    public Func<TState<S>, X, TResult<S>> Transform<S>(Func<TState<S>, CoProduct<A, B>, TResult<S>> reducer) =>
        (state, _) => reducer(state, CoProduct.Right<A, B>(Constant));
}

internal sealed record ConstantLeftTransducer<X, A, B>(A Constant) : Transducer<X, CoProduct<A, B>>
{
    public Func<TState<S>, X, TResult<S>> Transform<S>(Func<TState<S>, CoProduct<A, B>, TResult<S>> reducer) =>
        (state, _) => reducer(state, CoProduct.Left<A, B>(Constant));
}
