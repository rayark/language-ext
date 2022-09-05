#nullable enable
using System;
using LanguageExt.Common;
using LanguageExt.DSL.Transducers;
using LanguageExt.UnsafeValueAccess;
using static LanguageExt.Prelude;

namespace LanguageExt.DSL;

public static class CoProduct
{
    public static CoProduct<A, B> Fail<A, B>(Error value) => new CoProductFail<A, B>(value);
    public static CoProduct<A, B> Left<A, B>(A value) => new CoProductLeft<A, B>(value);
    public static CoProduct<A, B> Right<A, B>(B value) => new CoProductRight<A, B>(value);
}

public abstract record CoProduct<A, B>
{
    public abstract CoProduct<X, B> LeftMap<X>(Func<A, X> Left);
    public abstract CoProduct<A, Y> RightMap<Y>(Func<B, Y> Right);
    public abstract CoProduct<X, Y> BiMap<X, Y>(Func<A, X> Left, Func<B, Y> Right);
    public abstract bool IsRight { get; }
    public abstract bool IsLeft { get; }
    public abstract bool IsError { get; }

    public abstract TResult<CoProduct<S, T>> Transform<X, Y, S, T>(
        CoProduct<S, T> seed, 
        BiTransducer<A, X, B, Y> transducer,
        Func<TState<S>, X, TResult<S>> reducerLeft,
        Func<TState<T>, Y, TResult<T>> reducerRight);

    public abstract TResult<CoProduct<X, Y>> Transform<X, Y>(BiTransducer<A, X, B, Y> transducer);
}

public record CoProductLeft<A, B>(A Value) : CoProduct<A, B>
{
    public override CoProduct<X, B> LeftMap<X>(Func<A, X> Left) =>
        new CoProductLeft<X, B>(Left(Value));
    
    public override CoProduct<A, Y> RightMap<Y>(Func<B, Y> Right) =>
        new CoProductLeft<A, Y>(Value);
    
    public override CoProduct<X, Y> BiMap<X, Y>(Func<A, X> Left, Func<B, Y> Right) =>
        new CoProductLeft<X, Y>(Left(Value));

    public override TResult<CoProduct<S, T>> Transform<X, Y, S, T>(
        CoProduct<S, T> seed,
        BiTransducer<A, X, B, Y> transducer,
        Func<TState<S>, X, TResult<S>> reducerLeft,
        Func<TState<T>, Y, TResult<T>> reducerRight) =>
        seed switch
        {
            CoProductLeft<S, T> left => transducer.LeftTransducer
                .Transform(reducerLeft)(TState<S>.Create(left.Value), Value)
                .Map(CoProduct.Left<S, T>),
            CoProductRight<S, T> right => TResult.Continue(CoProduct.Right<S, T>(right.Value)),
            CoProductFail<S, T> fail => TResult.Fail<CoProduct<S, T>>(fail.Value),
            _ => throw new NotSupportedException()
        };
    
    public override TResult<CoProduct<X, Y>> Transform<X, Y>(BiTransducer<A, X, B, Y> transducer) =>
        transducer.LeftTransducer
                  .Transform<Option<X>>(Obj.MapNoReduce)(TState<Option<X>>.Create(None), Value)
                  .Map(static ox => ox.ValueUnsafe())
                  .Map(CoProduct.Left<X, Y>);
    
    public override bool IsRight => false;
    public override bool IsLeft => true;
    public override bool IsError => false;
}

public record CoProductRight<A, B>(B Value) : CoProduct<A, B>
{
    public override CoProduct<X, B> LeftMap<X>(Func<A, X> Left) =>
        new CoProductRight<X, B>(Value);
    
    public override CoProduct<A, Y> RightMap<Y>(Func<B, Y> Right) =>
        new CoProductRight<A, Y>(Right(Value));
    
    public override CoProduct<X, Y> BiMap<X, Y>(Func<A, X> Left, Func<B, Y> Right) =>
        new CoProductRight<X, Y>(Right(Value));

    public override TResult<CoProduct<S, T>> Transform<X, Y, S, T>(
        CoProduct<S, T> seed,
        BiTransducer<A, X, B, Y> transducer,
        Func<TState<S>, X, TResult<S>> reducerLeft,
        Func<TState<T>, Y, TResult<T>> reducerRight) =>
        seed switch
        {
            CoProductRight<S, T> right => transducer.RightTransducer
                .Transform(reducerRight)(TState<T>.Create(right.Value), Value)
                .Map(CoProduct.Right<S, T>),
            CoProductLeft<S, T> left => TResult.Continue(CoProduct.Left<S, T>(left.Value)),
            CoProductFail<S, T> fail => TResult.Fail<CoProduct<S, T>>(fail.Value),
            _ => throw new NotSupportedException()
        };
    
    public override TResult<CoProduct<X, Y>> Transform<X, Y>(BiTransducer<A, X, B, Y> transducer) =>
        transducer.RightTransducer
                  .Transform<Option<Y>>(Obj.MapNoReduce)(TState<Option<Y>>.Create(None), Value)
                  .Map(static oy => oy.ValueUnsafe())
                  .Map(CoProduct.Right<X, Y>); 

    public override bool IsRight => true;
    public override bool IsLeft => false;
    public override bool IsError => false;
}

public record CoProductFail<A, B>(Error Value) : CoProduct<A, B>
{
    public override CoProduct<X, B> LeftMap<X>(Func<A, X> Left) =>
        new CoProductFail<X, B>(Value);
    
    public override CoProduct<A, Y> RightMap<Y>(Func<B, Y> Right) =>
        new CoProductFail<A, Y>(Value);
    
    public override CoProduct<X, Y> BiMap<X, Y>(Func<A, X> Left, Func<B, Y> Right) =>
        new CoProductFail<X, Y>(Value);

    public override TResult<CoProduct<S, T>> Transform<X, Y, S, T>(
        CoProduct<S, T> seed,
        BiTransducer<A, X, B, Y> transducer,
        Func<TState<S>, X, TResult<S>> reducerLeft,
        Func<TState<T>, Y, TResult<T>> reducerRight) =>
        seed switch
        {
            CoProductFail<S, T> fail => TResult.Fail<CoProduct<S, T>>(fail.Value + Value),
            _                        => TResult.Fail<CoProduct<S, T>>(Value)
        };

    public override TResult<CoProduct<X, Y>> Transform<X, Y>(BiTransducer<A, X, B, Y> transducer) =>
        TResult.Fail<CoProduct<X, Y>>(Value);

    public override bool IsRight => false;
    public override bool IsLeft => true;
    public override bool IsError => true;

}
