using System;
using static LanguageExt.DSL2.SumAsync;

namespace LanguageExt.DSL2;

public static partial class SumAsyncTransducerExtensions
{
    public static SumTransducerAsync<X, Z, A, C> BiMap<X, Y, Z, A, B, C>(
        this SumTransducerAsync<X, Y, A, B> f, 
        SumTransducerAsync<Y, Z, B, C> g) =>
        compose(f, g);
    
    public static SumTransducerAsync<X, Z, A, C> BiMap<X, Y, Z, A, B, C>(
        this SumTransducerAsync<X, Y, A, B> f, Func<Y, Z> Left, Func<B, C> Right) =>
        compose(f, bimap(Left, Right));
    
    public static SumTransducerAsync<X, Z, A, C> BiMap<X, Y, Z, A, B, C>(
        this SumTransducerAsync<X, Y, A, B> f, TransducerAsync<Y, Z> Left, TransducerAsync<B, C> Right) =>
        compose(f, bimap(Left, Right));
    
    public static SumTransducerAsync<X, X, A, C> MapRight<X, A, B, C>(
        this SumTransducerAsync<X, X, A, B> f, 
        TransducerAsync<B, C> g) =>
        compose(f, mapRight<X, B, C>(g));
    
    public static SumTransducerAsync<X, X, A, C> MapRight<X, A, B, C>(
        this SumTransducerAsync<X, X, A, B> f, 
        Func<B, C> g) =>
        compose(f, mapRight<X, B, C>(g));
    
    public static SumTransducerAsync<X, Z, A, A> MapLeft<X, Y, Z, A>(
        this SumTransducerAsync<X, Y, A, A> f, 
        Func<Y, Z> g) =>
        compose(f, mapLeft<Y, Z, A>(g));
    
    public static SumTransducerAsync<X, Z, A, A> MapLeft<X, Y, Z, A>(
        this SumTransducerAsync<X, Y, A, A> f, 
        TransducerAsync<Y, Z> g) =>
        compose(f, mapLeft<Y, Z, A>(g));
    
    public static SumTransducerAsync<X, X, A, C> Select<X, A, B, C>(this SumTransducerAsync<X, X, A, B> f, Func<B, C> g) =>
        compose(f, mapRight<X, B, C>(g));
    
    public static SumTransducerAsync<X, X, A, B> Filter<X, A, B>(this SumTransducerAsync<X, X, A, B> f, TransducerAsync<B, bool> g) =>
        compose(f, mapRight<X, B, B>(TransducerAsync.filter(g)));
    
    public static SumTransducerAsync<X, X, A, B> Filter<X, A, B>(this SumTransducerAsync<X, X, A, B> f, Func<B, bool> g) =>
        compose(f, mapRight<X, B, B>(TransducerAsync.filter(g)));
    
    public static SumTransducerAsync<X, X, A, B> Where<X, A, B>(this SumTransducerAsync<X, X, A, B> f, TransducerAsync<B, bool> g) =>
        f.Filter(g);
    
    public static SumTransducerAsync<X, X, A, B> Where<X, A, B>(this SumTransducerAsync<X, X, A, B> f, Func<B, bool> g) =>
        f.Filter(g);

    public static SumTransducerAsync<X, X, E, B> Action<E, X, A, B>(
        this SumTransducerAsync<X, X, E, A> fa, 
        SumTransducerAsync<X, X, E, B> fb) =>
        action(fa, fb);

    public static SumTransducerAsync<X, X, E, B> Apply<E, X, A, B>(
        this SumTransducerAsync<X, X, E, TransducerAsync<A, B>> ff, 
        SumTransducerAsync<X, X, E, A> fa) =>
        apply(ff, fa);
    
    public static SumTransducerAsync<X, X, E, B> Apply<E, X, A, B>(
        this SumTransducerAsync<X, X, E, Transducer<A, B>> ff, 
        SumTransducerAsync<X, X, E, A> fa) =>
        apply(ff, fa);
    
    public static SumTransducerAsync<X, X, E, B> Apply<E, X, A, B>(
        this SumTransducerAsync<X, X, E, Func<A, B>> ff, 
        SumTransducerAsync<X, X, E, A> fa) =>
        apply(ff, fa);    
 
    public static SumTransducerAsync<X, Y, A, B> Flatten<X, Y, A, B>(
        this SumTransducerAsync<X, Y, A, SumTransducerAsync<X, Y, A, B>> f) => 
        flatten(f);
        
    public static SumTransducerAsync<X, Y, A, B> Flatten<X, Y, A, B>(
        this SumTransducerAsync<X, Y, A, SumTransducerAsync<X, Y, Unit, B>> f) => 
        flatten(f);
 
    public static SumTransducerAsync<X, Y, A, B> Flatten<X, Y, A, B>(
        this SumTransducerAsync<X, Y, A, SumTransducer<X, Y, A, B>> f) => 
        flatten(f);
        
    public static SumTransducerAsync<X, Y, A, B> Flatten<X, Y, A, B>(
        this SumTransducerAsync<X, Y, A, SumTransducer<X, Y, Unit, B>> f) => 
        flatten(f);
    
    public static SumTransducerAsync<X, X, E, B> Bind<E, X, A, B>(this SumTransducerAsync<X, X, E, A> fa, SumTransducerAsync<X, X,  A, SumTransducerAsync<X, X, E, B>> fab) =>
        bind(fa, fab);

    public static SumTransducerAsync<X, X, E, B> Bind<E, X, A, B>(this SumTransducerAsync<X, X, E, A> fa, Func<A, SumTransducerAsync<X, X, E, B>> fab) =>
        bind(fa, fab);

    public static SumTransducerAsync<X, X, E, B> SelectMany<E, X, A, B>(this SumTransducerAsync<X, X, E, A> fa, SumTransducerAsync<X, X, A, SumTransducerAsync<X, X, E, B>> fab) =>
        bind(fa, fab);

    public static SumTransducerAsync<X, X, E, B> SelectMany<E, X, A, B>(this SumTransducerAsync<X, X, E, A> fa, Func<A, SumTransducerAsync<X, X, E, B>> fab) =>
        bind(fa, fab);

    public static SumTransducerAsync<X, X, E, C> SelectMany<E, X, A, B, C>(
        this SumTransducerAsync<X, X, E, A> fa, 
        SumTransducerAsync<X, X, A, SumTransducerAsync<X, X, E, B>> fab,
        SumTransducerAsync<X, X, A, SumTransducerAsync<X, X, B, C>> fabc) =>
        bindMap(fa, fab, fabc);

    public static SumTransducerAsync<X, X, E, C> SelectMany<E, X, A, B, C>(
        this SumTransducerAsync<X, X, E, A> fa, 
        SumTransducerAsync<X, X, A, SumTransducerAsync<X, X, E, B>> fab,
        Func<A, B, C> fabc) =>
        bindMap(fa, fab, fabc);

    public static SumTransducerAsync<X, X, E, C> SelectMany<E, X, A, B, C>(
        this SumTransducerAsync<X, X, E, A> fa, 
        Func<A, SumTransducerAsync<X, X, E, B>> fab,
        Func<A, B, C> fabc) =>
        bindMap(fa, fab, fabc);

    public static SumTransducerAsync<X, Y, E, B> Fold<E, X, Y, A, B>(
        this SumTransducerAsync<X, Y, E, A> ta,
        TransducerAsync<A, TransducerAsync<B, B>> fold, 
        B state,
        Schedule schedule) =>
        new SumFoldTransducerAsync<E, X, Y, A, B>(fold, state, ta, (_, _) => true, schedule);

    public static SumTransducerAsync<X, Y, E, B> Fold<E, X, Y, A, B>(
        this SumTransducerAsync<X, Y, E, A> ta,
        Func<A, B, B> fold, 
        B state,
        Schedule schedule) =>
        new SumFoldTransducerAsync<E, X, Y, A, B>(TransducerAsync.curry(fold), state, ta, (_, _) => true, schedule);

    public static SumTransducerAsync<X, Y, E, B> FoldWhile<E, X, Y, A, B>(
        this SumTransducerAsync<X, Y, E, A> ta,
        TransducerAsync<A, TransducerAsync<B, B>> fold, 
        B state, 
        Func<A, bool> pred,
        Schedule schedule) =>
        new SumFoldTransducerAsync<E, X, Y, A, B>(fold, state, ta, (x, _) => pred(x), schedule);

    public static SumTransducerAsync<X, Y, E,  B> FoldWhile<E, X, Y, A, B>(
        this SumTransducerAsync<X, Y, E, A> ta,
        Func<A, B, B> fold, 
        B state, 
        Func<A, bool> pred,
        Schedule schedule) =>
        new SumFoldTransducerAsync<E, X, Y, A, B>(TransducerAsync.curry(fold), state, ta, (x, _) => pred(x), schedule);

    public static SumTransducerAsync<X, Y, E, B> FoldUntil<E, X, Y, A, B>(
        this SumTransducerAsync<X, Y, E, A> ta,
        TransducerAsync<A, TransducerAsync<B, B>> fold, 
        B state, 
        Func<A, bool> pred,
        Schedule schedule) =>
        new SumFoldTransducerAsync<E, X, Y, A, B>(fold, state, ta, (x, _) => !pred(x), schedule);

    public static SumTransducerAsync<X, Y, E, B> FoldUntil<E, X, Y, A, B>(
        this SumTransducerAsync<X, Y, E, A> ta,
        Func<A, B, B> fold, 
        B state, 
        Func<A, bool> pred,
        Schedule schedule) =>
        new SumFoldTransducerAsync<E, X, Y, A, B>(TransducerAsync.curry(fold), state, ta, (x, _) => !pred(x), schedule);      
}
