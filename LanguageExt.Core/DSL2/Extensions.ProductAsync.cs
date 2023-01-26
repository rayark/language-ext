using System;
using static LanguageExt.DSL2.ProductAsync;

namespace LanguageExt.DSL2;

public static partial class ProductAsyncTransducerExtensions
{
    public static ProductTransducerAsync<X, Z, A, C> BiMap<X, Y, Z, A, B, C>(
        this ProductTransducerAsync<X, Y, A, B> f, 
        ProductTransducerAsync<Y, Z, B, C> g) =>
        compose(f, g);
    
    public static ProductTransducerAsync<X, Z, A, C> BiMap<X, Y, Z, A, B, C>(
        this ProductTransducerAsync<X, Y, A, B> f, Func<Y, Z> Left, Func<B, C> Right) =>
        compose(f, bimap(Left, Right));
    
    public static ProductTransducerAsync<X, Z, A, C> BiMap<X, Y, Z, A, B, C>(
        this ProductTransducerAsync<X, Y, A, B> f, TransducerAsync<Y, Z> Left, TransducerAsync<B, C> Right) =>
        compose(f, bimap(Left, Right));
        
    public static ProductTransducerAsync<X, Y, A, C> MapSecond<X, Y, A, B, C>(
        this ProductTransducerAsync<X, Y, A, B> f, 
        Func<B, C> g) =>
        mapSecond(f, TransducerAsync.map(g));
    
    public static ProductTransducerAsync<X, Y, A, C> MapSecond<X, Y, A, B, C>(
        this ProductTransducerAsync<X, Y, A, B> f, 
        TransducerAsync<B, C> g) =>
        mapSecond(f, g);

    public static ProductTransducerAsync<X, Z, A, A> MapFirst<X, Y, Z, A>(
        this ProductTransducerAsync<X, Y, A, A> f,
        Func<Y, Z> g) =>
        mapFirst(f, TransducerAsync.map(g));
    
    public static ProductTransducerAsync<X, Z, A, A> MapFirst<X, Y, Z, A>(
        this ProductTransducerAsync<X, Y, A, A> f, 
        TransducerAsync<Y, Z> g) =>
        mapFirst(f, g);
    
    public static ProductTransducerAsync<X, X, A, C> Map<X, A, B, C>(
        this ProductTransducerAsync<X, X, A, B> f, 
        Func<B, C> g) =>
        compose(f, bimap<X, X, B, C>(static x => x, g));
    
    public static ProductTransducerAsync<X, X, A, C> Select<X, A, B, C>(
        this ProductTransducerAsync<X, X, A, B> f, 
        Func<B, C> g) =>
        compose(f, bimap<X, X, B, C>(static x => x, g));
    
    public static ProductTransducerAsync<X, X, E, B> Action<E, X, A, B>(
        this ProductTransducerAsync<X, X, E, A> fa, 
        ProductTransducerAsync<X, X, E, B> fb) =>
        action(fa, fb);

    public static ProductTransducerAsync<X, X, E, B> Apply<E, X, A, B>(
        this ProductTransducerAsync<X, X, E, TransducerAsync<A, B>> ff, 
        ProductTransducerAsync<X, X, E, A> fa) =>
        apply(ff, fa);
    
    public static ProductTransducerAsync<X, X, E, B> Apply<E, X, A, B>(
        this ProductTransducerAsync<X, X, E, Transducer<A, B>> ff, 
        ProductTransducerAsync<X, X, E, A> fa) =>
        apply(ff, fa);
    
    public static ProductTransducerAsync<X, X, E, B> Apply<E, X, A, B>(
        this ProductTransducerAsync<X, X, E, Func<A, B>> ff, 
        ProductTransducerAsync<X, X, E, A> fa) =>
        apply(ff, fa);    

    public static ProductTransducerAsync<X, Y, E, A> Flatten<X, Y, E, A>(
        this ProductTransducerAsync<X, Y, E, ProductTransducerAsync<X, Y, E, A>> ffa) =>
        flatten(ffa);

    public static ProductTransducerAsync<X, Y, A, B> Flatten<X, Y, A, B>(
        this ProductTransducerAsync<X, Y, A, ProductTransducerAsync<X, Y, Unit, B>> f) =>
        flatten(f);

    public static ProductTransducerAsync<X, Y, E, A> Flatten<X, Y, E, A>(
        this ProductTransducerAsync<X, Y, E, ProductTransducer<X, Y, E, A>> ffa) =>
        flatten(ffa);

    public static ProductTransducerAsync<X, Y, A, B> Flatten<X, Y, A, B>(
        this ProductTransducerAsync<X, Y, A, ProductTransducer<X, Y, Unit, B>> f) =>
        flatten(f);

    public static ProductTransducerAsync<X, X, E, B> Bind<E, X, A, B>(this ProductTransducerAsync<X, X, E, A> fa, ProductTransducerAsync<X, X,  A, ProductTransducerAsync<X, X, E, B>> fab) =>
        bind(fa, fab);

    public static ProductTransducerAsync<X, X, E, B> Bind<E, X, A, B>(this ProductTransducerAsync<X, X, E, A> fa, Func<A, ProductTransducerAsync<X, X, E, B>> fab) =>
        bind(fa, fab);

    public static ProductTransducerAsync<X, X, E, B> SelectMany<E, X, A, B>(this ProductTransducerAsync<X, X, E, A> fa, ProductTransducerAsync<X, X, A, ProductTransducerAsync<X, X, E, B>> fab) =>
        bind(fa, fab);

    public static ProductTransducerAsync<X, X, E, B> SelectMany<E, X, A, B>(this ProductTransducerAsync<X, X, E, A> fa, Func<A, ProductTransducerAsync<X, X, E, B>> fab) =>
        bind(fa, fab);

    public static ProductTransducerAsync<X, X, E, C> SelectMany<E, X, A, B, C>(
        this ProductTransducerAsync<X, X, E, A> fa, 
        ProductTransducerAsync<X, X, A, ProductTransducerAsync<X, X, E, B>> fab,
        ProductTransducerAsync<X, X, A, ProductTransducerAsync<X, X, B, C>> fabc) =>
        bindMap(fa, fab, fabc);

    public static ProductTransducerAsync<X, X, E, C> SelectMany<E, X, A, B, C>(
        this ProductTransducerAsync<X, X, E, A> fa, 
        ProductTransducerAsync<X, X, A, ProductTransducerAsync<X, X, E, B>> fab,
        Func<A, B, C> fabc) =>
        bindMap(fa, fab, fabc);

    public static ProductTransducerAsync<X, X, E, C> SelectMany<E, X, A, B, C>(
        this ProductTransducerAsync<X, X, E, A> fa, 
        Func<A, ProductTransducerAsync<X, X, E, B>> fab,
        Func<A, B, C> fabc) =>
        bindMap(fa, fab, fabc);
}
