#nullable enable

using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record BiMapTransducer<X, Y, A, B>(Transducer<X, Y> Left, Transducer<A, B> Right) : BiTransducer<X, Y, A, B>, Transducer<CoProduct<X, A>, CoProduct<Y, B>>
{
    public override Transducer<X, Y> LeftTransducer => 
        Left;
    
    public override Transducer<A, B> RightTransducer => 
        Right;

    public Func<TState<S>, CoProduct<X, A>, TResult<S>> Transform<S>(
        Func<TState<S>, CoProduct<Y, B>, TResult<S>> reduce) =>
        (state, value) =>
        {
            var res = value.Transduce(this);
            if (res.Faulted) return TResult.Fail<S>(res.ErrorUnsafe);
            if (res.Complete) return TResult.Complete<S>(state);
            return reduce(state, res.ValueUnsafe);
        };
}

internal sealed record BiMapTransducer2<X, Y, A, B>(Func<X, Y> Left, Func<A, B> Right) : BiTransducer<X, Y, A, B>, Transducer<CoProduct<X, A>, CoProduct<Y, B>>
{
    public override Transducer<X, Y> LeftTransducer => 
        Transducer.map(Left);
    
    public override Transducer<A, B> RightTransducer => 
        Transducer.map(Right);

    public Func<TState<S>, CoProduct<X, A>, TResult<S>> Transform<S>(
        Func<TState<S>, CoProduct<Y, B>, TResult<S>> reduce) =>
        (state, value) => reduce(state, value.BiMap(Left, Right));
}
