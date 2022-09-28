using System;
using static LanguageExt.Prelude;

namespace LanguageExt.DSL.Transducers;

public abstract record BiTransducer<X, Y, A, B>
{
    public (Func<TState<S>, X, TResult<S>> Left, Func<TState<T>, A, TResult<T>> Right) BiTransform<S, T>(
        Func<TState<S>, Y, TResult<S>> reduceLeft,
        Func<TState<T>, B, TResult<T>> reduceRight) =>
        (LeftTransducer.Transform(reduceLeft), RightTransducer.Transform(reduceRight));

    public Func<TState<S>, X, TResult<S>> LeftTransform<S>(Func<TState<S>, Y, TResult<S>> reduceLeft) =>
        LeftTransducer.Transform(reduceLeft);

    public Func<TState<T>, A, TResult<T>> RightTransform<T>(Func<TState<T>, B, TResult<T>> reduceRight) =>
        RightTransducer.Transform(reduceRight);

    public abstract Transducer<X, Y> LeftTransducer { get; }
    public abstract Transducer<A, B> RightTransducer { get; }
}

public static class BiTransducer<X, A>
{
    public static BiTransducer<X, X, A, A> identity = 
        BiTransducer.bimap<X, X, A, A>(static x => x, static a => a);
        
    public static BiTransducer<Obj<X>, X, Obj<A>, A> join => BiJoinObjTransducer<X, A>.Default;
}

public static class BiTransducer
{
    public static BiTransducer<X, Z, A, C> compose<X, Y, Z, A, B, C>(
        BiTransducer<X, Y, A, B> first,
        BiTransducer<Y, Z, B, C> second) =>
        new BiComposeTransducer<X, Y, Z, A, B, C>(first, second);

    public static BiTransducer<X, Z, A, C> compose<X, Y, Z, A, B, C>(
        Transducer<X, Y> leftFirst, Transducer<Y, Z> leftSecond,
        Transducer<A, B> rightFirst, Transducer<B, C> rightSecond) =>
        new BiComposeTransducer2<X, Y, Z, A, B, C>(leftFirst, leftSecond, rightFirst, rightSecond);

    public static BiTransducer<X, Y, A, B> constant<X, Y, A, B>(Y Left, B Right) =>
        bimap(Transducer.constant<X, Y>(Left), Transducer.constant<A, B>(Right));

    public static BiTransducer<X, Y, A, B> bimap<X, Y, A, B>(Func<X, Y> Left, Func<A, B> Right) =>
        new BiMapTransducer<X, Y, A, B>(Transducer.map(Left), Transducer.map(Right));

    public static BiTransducer<X, Y, A, B> bimap<X, Y, A, B>(Transducer<X, Y> Left, Transducer<A, B> Right) =>
        new BiMapTransducer<X, Y, A, B>(Left, Right);

    public static BiTransducer<X, Y, A, A> mapLeft<X, Y, A>(Transducer<X, Y> Left) =>
        bimap(Left, Transducer<A>.identity);

    public static BiTransducer<X, Y, A, A> mapLeft<X, Y, A>(Func<X, Y> Left) =>
        bimap<X, Y, A, A>(Left, static x => x);

    public static BiTransducer<X, X, A, B> mapRight<X, A, B>(Transducer<A, B> Right) =>
        bimap(Transducer<X>.identity, Right);

    public static BiTransducer<X, X, A, B> mapRight<X, A, B>(Func<A, B> Right) =>
        bimap<X, X, A, B>(static x => x, Right);

    public static BiTransducer<X, X, A, A> bifilter<X, A>(Func<X, bool> Left, Func<A, bool> Right) =>
        new BiFilterTransducer<X, A>(Left, Right);

    public static BiTransducer<X, X, A, A> filterLeft<X, A>(Func<X, bool> Left) =>
        new BiFilterTransducer<X, A>(Left, static _ => true);

    public static BiTransducer<X, X, A, A> filterRight<X, A>(Func<A, bool> Right) =>
        new BiFilterTransducer<X, A>(static _ => true, Right);
}
