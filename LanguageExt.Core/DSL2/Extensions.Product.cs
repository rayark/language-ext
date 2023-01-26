using System;
using static LanguageExt.DSL2.Product;

namespace LanguageExt.DSL2;

public static partial class ProductTransducerExtensions
{
    public static ProductTransducer<X, Z, A, C> BiMap<X, Y, Z, A, B, C>(
        this ProductTransducer<X, Y, A, B> f, 
        ProductTransducer<Y, Z, B, C> g) =>
        compose(f, g);
    
    public static ProductTransducer<X, Z, A, C> BiMap<X, Y, Z, A, B, C>(
        this ProductTransducer<X, Y, A, B> f, Func<Y, Z> First, Func<B, C> Second) =>
        compose(f, bimap(First, Second));
    
    public static ProductTransducer<X, Z, A, C> BiMap<X, Y, Z, A, B, C>(
        this ProductTransducer<X, Y, A, B> f, Transducer<Y, Z> First, Transducer<B, C> Second) =>
        compose(f, bimap(First, Second));
    
    public static ProductTransducer<X, Y, A, C> MapSecond<X, Y, A, B, C>(
        this ProductTransducer<X, Y, A, B> f, 
        Func<B, C> g) =>
        mapSecond(f, Transducer.map(g));
    
    public static ProductTransducer<X, Y, A, C> MapSecond<X, Y, A, B, C>(
        this ProductTransducer<X, Y, A, B> f, 
        Transducer<B, C> g) =>
        mapSecond(f, g);

    public static ProductTransducer<X, Z, A, A> MapFirst<X, Y, Z, A>(
        this ProductTransducer<X, Y, A, A> f,
        Func<Y, Z> g) =>
        mapFirst(f, Transducer.map(g));
    
    public static ProductTransducer<X, Z, A, A> MapFirst<X, Y, Z, A>(
        this ProductTransducer<X, Y, A, A> f, 
        Transducer<Y, Z> g) =>
        mapFirst(f, g);

    public static ProductTransducer<X, X, A, C> Select<X, A, B, C>(
        this ProductTransducer<X, X, A, B> f,
        Func<B, C> g) =>
        f.MapSecond(g);
    
    public static ProductTransducer<X, X, E, B> Action<E, X, A, B>(
        this ProductTransducer<X, X, E, A> fa, 
        ProductTransducer<X, X, E, B> fb) =>
        action(fa, fb);

    public static ProductTransducer<X, X, E, B> Apply<E, X, A, B>(
        this ProductTransducer<X, X, E, Transducer<A, B>> ff, 
        ProductTransducer<X, X, E, A> fa) =>
        apply(ff, fa);
    
    public static ProductTransducer<X, X, E, B> Apply<E, X, A, B>(
        this ProductTransducer<X, X, E, Func<A, B>> ff, 
        ProductTransducer<X, X, E, A> fa) =>
        apply(ff, fa);
    
    public static ProductTransducer<X, Y, E, A> Flatten<X, Y, E, A>(
        this ProductTransducer<X, Y, E, ProductTransducer<X, Y, E, A>> ffa) =>
        flatten(ffa);

    public static ProductTransducer<X, Y, A, B> Flatten<X, Y, A, B>(
        this ProductTransducer<X, Y, A, ProductTransducer<X, Y, Unit, B>> f) =>
        flatten(f);

    public static ProductTransducer<X, X, E, B> Bind<E, X, A, B>(this ProductTransducer<X, X, E, A> fa, ProductTransducer<X, X,  A, ProductTransducer<X, X, E, B>> fab) =>
        bind(fa, fab);

    public static ProductTransducer<X, X, E, B> Bind<E, X, A, B>(this ProductTransducer<X, X, E, A> fa, Func<A, ProductTransducer<X, X, E, B>> fab) =>
        bind(fa, fab);

    public static ProductTransducer<X, X, E, B> SelectMany<E, X, A, B>(this ProductTransducer<X, X, E, A> fa, ProductTransducer<X, X, A, ProductTransducer<X, X, E, B>> fab) =>
        bind(fa, fab);

    public static ProductTransducer<X, X, E, B> SelectMany<E, X, A, B>(this ProductTransducer<X, X, E, A> fa, Func<A, ProductTransducer<X, X, E, B>> fab) =>
        bind(fa, fab);

    public static ProductTransducer<X, X, E, C> SelectMany<E, X, A, B, C>(
        this ProductTransducer<X, X, E, A> fa, 
        ProductTransducer<X, X, A, ProductTransducer<X, X, E, B>> fab,
        ProductTransducer<X, X, A, ProductTransducer<X, X, B, C>> fabc) =>
        bindMap(fa, fab, fabc);

    public static ProductTransducer<X, X, E, C> SelectMany<E, X, A, B, C>(
        this ProductTransducer<X, X, E, A> fa, 
        ProductTransducer<X, X, A, ProductTransducer<X, X, E, B>> fab,
        Func<A, B, C> fabc) =>
        bindMap(fa, fab, fabc);

    public static ProductTransducer<X, X, E, C> SelectMany<E, X, A, B, C>(
        this ProductTransducer<X, X, E, A> fa, 
        Func<A, ProductTransducer<X, X, E, B>> fab,
        Func<A, B, C> fabc) =>
        bindMap(fa, fab, fabc);
}
