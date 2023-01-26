#nullable enable
using System;
using System.Collections.Generic;

namespace LanguageExt.DSL2;

// ---------------------------------------------------------------------------------------------------------------------

public static class Product
{
    public static ProductTransducer<X, Y, A, B> bimap<X, Y, A, B>(Transducer<X, Y> first, Transducer<A, B> second) => 
        new ProductBiFunctorTransducer<X, Y, A, B>(first, second);

    public static ProductTransducer<X, Y, A, B> bimap<X, Y, A, B>(Func<X, Y> first, Func<A, B> second) => 
        bimap(Transducer.map(first), Transducer.map(second));
    
    public static ProductTransducer<X, Z, A, B> mapFirst<X, Y, Z, A, B>(
        ProductTransducer<X, Y, A, B> Transducer, 
        Transducer<Y, Z> First) => 
        new ProductMapFirstTransducer<X,Y,Z,A,B>(Transducer, First);

    public static ProductTransducer<X, Y, A, C> mapSecond<X, Y, A, B, C>(
        ProductTransducer<X, Y, A, B> Transducer,
        Transducer<B, C> Second) =>
        new ProductMapSecondTransducer<X, Y, A, B, C>(Transducer, Second);

    public static ProductTransducer<X, X, A, A> identity<X, A>() =>
        bimap(Transducer.identity<X>(), Transducer.identity<A>());

    public static ProductTransducer<J, L, A, C> compose<J, K, L, A, B, C>(
        ProductTransducer<J, K, A, B> tab,
        ProductTransducer<K, L, B, C> tbc) =>
        new ProductComposeTransducer<J, K, L, A, B, C>(tab, tbc);

    public static ProductTransducer<J, M, A, D> compose<J, K, L, M, A, B, C, D>(
        ProductTransducer<J, K, A, B> tab, 
        ProductTransducer<K, L, B, C> tbc, 
        ProductTransducer<L, M, C, D> tcd) =>
        compose(compose(tab, tbc), tcd);

    public static ProductTransducer<J, N, A, E> compose<J, K, L, M, N, A, B, C, D, E>(
        ProductTransducer<J, K, A, B> tab, 
        ProductTransducer<K, L, B, C> tbc, 
        ProductTransducer<L, M, C, D> tcd, 
        ProductTransducer<M, N, D, E> tde) =>
        compose(compose(tab, tbc), compose(tcd, tde));

    public static ProductTransducer<X, Y, A, B> expand<X, Y, A, B>(Transducer<(X, A), (Y, B)> t) =>
        new ProductExpandTransducer<X, Y, A, B>(t);

    public static ProductTransducer<X, X, E, B> action<E, X, A, B>(
        ProductTransducer<X, X, E, A> fa, 
        ProductTransducer<X, X, E, B> fb) =>
        new ActionProductTransducer1<E, X, A, B>(fa, fb);

    public static ProductTransducer<X, X, E, B> apply<E, X, A, B>(
        ProductTransducer<X, X, E, Transducer<A, B>> ff, 
        ProductTransducer<X, X, E, A> fa) =>
        new ApplyProductTransducer1<E, X, A, B>(ff, fa);
    
    public static ProductTransducer<X, X, E, B> apply<E, X, A, B>(
        ProductTransducer<X, X, E, Func<A, B>> ff, 
        ProductTransducer<X, X, E, A> fa) =>
        new ApplyProductTransducer2<E, X, A, B>(ff, fa);

    public static ProductTransducer<X, Y, E, A> flatten<X, Y, E, A>(
        ProductTransducer<X, Y, E, ProductTransducer<X, Y, E, A>> ffa) =>
        new ProductFlattenTransducer<X, Y, E, A>(ffa);

    public static ProductTransducer<X, Y, A, B> flatten<X, Y, A, B>(
        ProductTransducer<X, Y, A, ProductTransducer<X, Y, Unit, B>> f) =>
        new ProductFlattenTransducerUnit<X, Y, A, B>(f);

    public static ProductTransducer<X, X, E, B> bind<E, X, A, B>(
        ProductTransducer<X, X, E, A> ma,
        ProductTransducer<X, X, A, ProductTransducer<X, X, E, B>> bind) =>
        new BindProductTransducer1<X, E, A, B>(ma, bind);

    public static ProductTransducer<X, X, E, B> bind<E, X, A, B>(
        ProductTransducer<X, X, E, A> ma,
        Func<A, ProductTransducer<X, X, E, B>> bind) =>
        new BindProductTransducer2<X, E, A, B>(ma, bind);

    public static ProductTransducer<X, X, E, C> bindMap<E, X, A, B, C>(
        ProductTransducer<X, X, E, A> ma,
        ProductTransducer<X, X, A, ProductTransducer<X, X, E, B>> bind,
        ProductTransducer<X, X, A, ProductTransducer<X, X, B, C>> map) =>
        new SelectManyProductTransducer1<X, E, A, B, C>(ma, bind, map);

    public static ProductTransducer<X, X, E, C> bindMap<E, X, A, B, C>(
        ProductTransducer<X, X, E, A> ma,
        ProductTransducer<X, X, A, ProductTransducer<X, X, E, B>> bind,
        Func<A, B, C> map) =>
        new SelectManyProductTransducer2<X, E, A, B, C>(ma, bind, map);

    public static ProductTransducer<X, X, E, C> bindMap<E, X, A, B, C>(
        ProductTransducer<X, X, E, A> ma,
        Func<A, ProductTransducer<X, X, E, B>> bind,
        Func<A, B, C> map) =>
        new SelectManyProductTransducer3<X, E, A, B, C>(ma, bind, map);
}
