#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record MapLeftTransducer<X, Y, A>(Transducer<X, Y> Function) : Transducer<CoProduct<X, A>, CoProduct<Y, A>>
{
    public Func<TState<S>, CoProduct<X, A>, TResult<S>> Transform<S>(
        Func<TState<S>, CoProduct<Y, A>, TResult<S>> reduce) =>
        (state, value) => value switch
        {
            CoProductLeft<X, A> l => Function.Transform<S>((s, x) => reduce(s, CoProduct.Left<Y, A>(x)))(state, l.Value),   
            CoProductFail<X, A> f => TResult.Fail<S>(f.Value),   
            _ => TResult.Continue(state.Value)   
        };
}

internal sealed record MapLeftTransducer2<X, Y, A>(Func<X, Y> Function) : Transducer<CoProduct<X, A>, CoProduct<Y, A>>
{
    public Func<TState<S>, CoProduct<X, A>, TResult<S>> Transform<S>(
        Func<TState<S>, CoProduct<Y, A>, TResult<S>> reduce) =>
        (state, value) => reduce(state, value.LeftMap(Function));
}

