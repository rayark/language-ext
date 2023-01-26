using System;
using static LanguageExt.DSL2.Sum;

namespace LanguageExt.DSL2;

public static partial class SumTransducerExtensions
{
    public static SumTransducer<X, Z, A, C> BiMap<X, Y, Z, A, B, C>(
        this SumTransducer<X, Y, A, B> f, 
        SumTransducer<Y, Z, B, C> g) =>
        compose(f, g);
    
    public static SumTransducer<X, Z, A, C> BiMap<X, Y, Z, A, B, C>(
        this SumTransducer<X, Y, A, B> f, Func<Y, Z> Left, Func<B, C> Right) =>
        compose(f, bimap(Left, Right));
    
    public static SumTransducer<X, Z, A, C> BiMap<X, Y, Z, A, B, C>(
        this SumTransducer<X, Y, A, B> f, Transducer<Y, Z> Left, Transducer<B, C> Right) =>
        compose(f, bimap(Left, Right));
    
    public static SumTransducer<X, X, A, C> MapRight<X, A, B, C>(
        this SumTransducer<X, X, A, B> f, 
        Func<B, C> g) =>
        compose(f, mapRight<X, B, C>(g));
    
    public static SumTransducer<X, X, A, C> MapRight<X, A, B, C>(
        this SumTransducer<X, X, A, B> f, 
        Transducer<B, C> g) =>
        compose(f, mapRight<X, B, C>(g));
    
    public static SumTransducer<X, Z, A, A> MapLeft<X, Y, Z, A>(
        this SumTransducer<X, Y, A, A> f, 
        Func<Y, Z> g) =>
        compose(f, mapLeft<Y, Z, A>(g));
    
    public static SumTransducer<X, Z, A, A> MapLeft<X, Y, Z, A>(
        this SumTransducer<X, Y, A, A> f, 
        Transducer<Y, Z> g) =>
        compose(f, mapLeft<Y, Z, A>(g));
    
    public static SumTransducer<X, X, A, C> Select<X, A, B, C>(this SumTransducer<X, X, A, B> f, Func<B, C> g) =>
        compose(f, mapRight<X, B, C>(g));
    
    public static SumTransducer<X, X, A, B> Filter<X, A, B>(this SumTransducer<X, X, A, B> f, Transducer<B, bool> g) =>
        compose(f, mapRight<X, B, B>(Transducer.filter(g)));
    
    public static SumTransducer<X, X, A, B> Filter<X, A, B>(this SumTransducer<X, X, A, B> f, Func<B, bool> g) =>
        compose(f, mapRight<X, B, B>(Transducer.filter(g)));
    
    public static SumTransducer<X, X, A, B> Where<X, A, B>(this SumTransducer<X, X, A, B> f, Transducer<B, bool> g) =>
        f.Filter(g);
    
    public static SumTransducer<X, X, A, B> Where<X, A, B>(this SumTransducer<X, X, A, B> f, Func<B, bool> g) =>
        f.Filter(g);
    
    public static SumTransducer<X, X, E, B> Action<E, X, A, B>(
        this SumTransducer<X, X, E, A> fa, 
        SumTransducer<X, X, E, B> fb) =>
        action(fa, fb);

    public static SumTransducer<X, X, E, B> Apply<E, X, A, B>(
        this SumTransducer<X, X, E, Transducer<A, B>> ff, 
        SumTransducer<X, X, E, A> fa) =>
        apply(ff, fa);
    
    public static SumTransducer<X, X, E, B> Apply<E, X, A, B>(
        this SumTransducer<X, X, E, Func<A, B>> ff, 
        SumTransducer<X, X, E, A> fa) =>
        apply(ff, fa);

    public static SumTransducer<X, Y, A, B> Flatten<X, Y, A, B>(this SumTransducer<X, Y, A, SumTransducer<X, Y, A, B>> f) => 
        flatten(f);
 
    public static SumTransducer<X, Y, A, B> Flatten<X, Y, A, B>(this SumTransducer<X, Y, A, SumTransducer<X, Y, Unit, B>> f) => 
        flatten(f);

    public static SumTransducer<X, X, E, B> Bind<E, X, A, B, MXB>(this SumTransducer<X, X, E, A> fa, Func<A, MXB> fab) 
        where MXB : IsSumTransducer<X, X, E, B> =>
        bind<E, X, A, B, MXB>(fa, fab);

    public static SumTransducer<X, X, E, B> Bind<E, X, A, B>(this SumTransducer<X, X, E, A> fa, SumTransducer<X, X,  A, SumTransducer<X, X, E, B>> fab) =>
        bind(fa, fab);

    public static SumTransducer<X, X, E, B> Bind<E, X, A, B>(this SumTransducer<X, X, E, A> fa, Func<A, SumTransducer<X, X, E, B>> fab) =>
        bind(fa, fab);

    public static SumTransducer<X, X, E, B> SelectMany<E, X, A, B>(this SumTransducer<X, X, E, A> fa, SumTransducer<X, X, A, SumTransducer<X, X, E, B>> fab) =>
        bind(fa, fab);

    public static SumTransducer<X, X, E, B> SelectMany<E, X, A, B>(this SumTransducer<X, X, E, A> fa, Func<A, SumTransducer<X, X, E, B>> fab) =>
        bind(fa, fab);

    public static SumTransducer<X, X, E, C> SelectMany<E, X, A, B, C>(
        this SumTransducer<X, X, E, A> fa, 
        SumTransducer<X, X, A, SumTransducer<X, X, E, B>> fab,
        SumTransducer<X, X, A, SumTransducer<X, X, B, C>> fabc) =>
        bindMap(fa, fab, fabc);

    public static SumTransducer<X, X, E, C> SelectMany<E, X, A, B, C>(
        this SumTransducer<X, X, E, A> fa, 
        SumTransducer<X, X, A, SumTransducer<X, X, E, B>> fab,
        Func<A, B, C> fabc) =>
        bindMap(fa, fab, fabc);

    public static SumTransducer<X, X, E, C> SelectMany<E, X, A, B, C>(
        this SumTransducer<X, X, E, A> fa, 
        Func<A, SumTransducer<X, X, E, B>> fab,
        Func<A, B, C> fabc) =>
        bindMap(fa, fab, fabc);

    public static SumTransducer<X, Y, E, B> Fold<E, X, Y, A, B>(
        this SumTransducer<X, Y, E, A> ta,
        Transducer<A, Transducer<B, B>> fold, 
        B state,
        Schedule schedule) =>
        new SumFoldTransducer<E, X, Y, A, B>(fold, state, ta, (_, _) => true, schedule);

    public static SumTransducer<X, Y, E, B> Fold<E, X, Y, A, B>(
        this SumTransducer<X, Y, E, A> ta,
        Func<A, B, B> fold, 
        B state,
        Schedule schedule) =>
        new SumFoldTransducer<E, X, Y, A, B>(Transducer.curry(fold), state, ta, (_, _) => true, schedule);

    public static SumTransducer<X, Y, E, B> FoldWhile<E, X, Y, A, B>(
        this SumTransducer<X, Y, E, A> ta,
        Transducer<A, Transducer<B, B>> fold, 
        B state, 
        Func<A, bool> pred,
        Schedule schedule) =>
        new SumFoldTransducer<E, X, Y, A, B>(fold, state, ta, (x, _) => pred(x), schedule);

    public static SumTransducer<X, Y, E,  B> FoldWhile<E, X, Y, A, B>(
        this SumTransducer<X, Y, E, A> ta,
        Func<A, B, B> fold, 
        B state, 
        Func<A, bool> pred,
        Schedule schedule) =>
        new SumFoldTransducer<E, X, Y, A, B>(Transducer.curry(fold), state, ta, (x, _) => pred(x), schedule);

    public static SumTransducer<X, Y, E, B> FoldUntil<E, X, Y, A, B>(
        this SumTransducer<X, Y, E, A> ta,
        Transducer<A, Transducer<B, B>> fold, 
        B state, 
        Func<A, bool> pred,
        Schedule schedule) =>
        new SumFoldTransducer<E, X, Y, A, B>(fold, state, ta, (x, _) => !pred(x), schedule);

    public static SumTransducer<X, Y, E, B> FoldUntil<E, X, Y, A, B>(
        this SumTransducer<X, Y, E, A> ta,
        Func<A, B, B> fold, 
        B state, 
        Func<A, bool> pred,
        Schedule schedule) =>
        new SumFoldTransducer<E, X, Y, A, B>(Transducer.curry(fold), state, ta, (x, _) => !pred(x), schedule);      
}
