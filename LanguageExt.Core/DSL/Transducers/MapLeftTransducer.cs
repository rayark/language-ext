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
            _ => TResult.Complete(state.Value)   
        };
}

internal sealed record MapLeftTransducer2<X, Y, A>(Func<X, Y> Function) : Transducer<CoProduct<X, A>, CoProduct<Y, A>>
{
    public Func<TState<S>, CoProduct<X, A>, TResult<S>> Transform<S>(
        Func<TState<S>, CoProduct<Y, A>, TResult<S>> reduce) =>
        (state, value) => value.IsLeft 
            ? reduce(state, value.LeftMap(Function))
            :  TResult.Complete(state.Value);
}

internal sealed record MapLeftBackTransducer<X, Y, A>(Transducer<X, Y> Function) : Transducer<CoProduct<X, A>, Y>
{
    public Func<TState<S>, CoProduct<X, A>, TResult<S>> Transform<S>(
        Func<TState<S>, Y, TResult<S>> reduce) =>
        (state, value) => value is CoProductLeft<X, A> l 
            ? Function.Transform(reduce)(state, l.Value) 
            : TResult.Complete(state.Value);
}

internal sealed record MapLeftBackTransducer2<X, Y, A>(Func<X, Y> Function) : Transducer<CoProduct<X, A>, Y>
{
    public Func<TState<S>, CoProduct<X, A>, TResult<S>> Transform<S>(
        Func<TState<S>, Y, TResult<S>> reduce) =>
        (state, value) => value is CoProductLeft<X, A> l 
            ? reduce(state, Function(l.Value)) 
            : TResult.Complete(state.Value);
}
