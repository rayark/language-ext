#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record MapRightTransducer<X, A, B>(Transducer<A, B> Function) : Transducer<CoProduct<X, A>, CoProduct<X, B>>
{
    public Func<TState<S>, CoProduct<X, A>, TResult<S>> Transform<S>(
        Func<TState<S>, CoProduct<X, B>, TResult<S>> reduce) =>
        (state, value) => value switch
        {
            CoProductRight<X, A> r => Function.Transform<S>((s, b) => reduce(s, CoProduct.Right<X, B>(b)))(state, r.Value),
            CoProductFail<X, A> f => TResult.Fail<S>(f.Value), 
            _ => TResult.Continue(state.Value)
        };
}

internal sealed record MapRightTransducer2<X, A, B>(Func<A, B> Function) : Transducer<CoProduct<X, A>, CoProduct<X, B>>
{
    public Func<TState<S>, CoProduct<X, A>, TResult<S>> Transform<S>(
        Func<TState<S>, CoProduct<X, B>, TResult<S>> reduce) =>
        (state, value) => reduce(state, value.RightMap(Function));
}
