#nullable enable
using System;

namespace LanguageExt.DSL2;

/// <Productmary>
/// Async product category
/// </Productmary>
public static class ProductAsync
{
    public static ProductTransducerAsync<X, Y, A, B> bimap<X, Y, A, B>(TransducerAsync<X, Y> first, TransducerAsync<A, B> second) => 
        new ProductBiFunctorTransducerAsync<X, Y, A, B>(first, second);

    public static ProductTransducerAsync<X, Y, A, B> bimap<X, Y, A, B>(Func<X, Y> first, Func<A, B> second) => 
        bimap(TransducerAsync.map(first), TransducerAsync.map(second));

    public static ProductTransducerAsync<X, Z, A, B> mapFirst<X, Y, Z, A, B>(
        ProductTransducerAsync<X, Y, A, B> Transducer,
        TransducerAsync<Y, Z> First) =>
        new ProductMapFirstTransducerAsync<X, Y, Z, A, B>(Transducer, First);

    public static ProductTransducerAsync<X, Y, A, C> mapSecond<X, Y, A, B, C>(
        ProductTransducerAsync<X, Y, A, B> Transducer,
        TransducerAsync<B, C> Second) =>
        new ProductMapSecondTransducerAsync<X, Y, A, B, C>(Transducer, Second);
    
    public static ProductTransducerAsync<X, X, A, A> identity<X, A>() =>
        bimap(TransducerAsync.identity<X>(), TransducerAsync.identity<A>());

    public static ProductTransducerAsync<J, L, A, C> compose<J, K, L, A, B, C>(
        ProductTransducerAsync<J, K, A, B> tab,
        ProductTransducerAsync<K, L, B, C> tbc) =>
        new ProductComposeTransducerAsync<J, K, L, A, B, C>(tab, tbc);

    public static ProductTransducerAsync<J, M, A, D> compose<J, K, L, M, A, B, C, D>(
        ProductTransducerAsync<J, K, A, B> tab, 
        ProductTransducerAsync<K, L, B, C> tbc, 
        ProductTransducerAsync<L, M, C, D> tcd) =>
        compose(compose(tab, tbc), tcd);

    public static ProductTransducerAsync<J, N, A, E> compose<J, K, L, M, N, A, B, C, D, E>(
        ProductTransducerAsync<J, K, A, B> tab, 
        ProductTransducerAsync<K, L, B, C> tbc, 
        ProductTransducerAsync<L, M, C, D> tcd, 
        ProductTransducerAsync<M, N, D, E> tde) =>
        compose(compose(tab, tbc), compose(tcd, tde));

    public static ProductTransducerAsync<X, Y, A, B> expand<X, Y, A, B>(TransducerAsync<(X, A), (Y, B)> t) =>
        new ProductExpandTransducerAsync<X, Y, A, B>(t);

    public static ProductTransducerAsync<X, X, E, B> action<E, X, A, B>(
        ProductTransducerAsync<X, X, E, A> fa, 
        ProductTransducerAsync<X, X, E, B> fb) =>
        new ActionProductTransducerAsync1<E, X, A, B>(fa, fb);

    public static ProductTransducerAsync<X, X, E, B> apply<E, X, A, B>(
        ProductTransducerAsync<X, X, E, TransducerAsync<A, B>> ff, 
        ProductTransducerAsync<X, X, E, A> fa) =>
        new ApplyProductTransducerAsync1<E, X, A, B>(ff, fa);
    
    public static ProductTransducerAsync<X, X, E, B> apply<E, X, A, B>(
        ProductTransducerAsync<X, X, E, Transducer<A, B>> ff, 
        ProductTransducerAsync<X, X, E, A> fa) =>
        new ApplyProductTransducerAsyncSync1<E, X, A, B>(ff, fa);
    
    public static ProductTransducerAsync<X, X, E, B> apply<E, X, A, B>(
        ProductTransducerAsync<X, X, E, Func<A, B>> ff, 
        ProductTransducerAsync<X, X, E, A> fa) =>
        new ApplyProductTransducerAsync2<E, X, A, B>(ff, fa);

    public static ProductTransducerAsync<X, Y, E, A> flatten<X, Y, E, A>(
        ProductTransducerAsync<X, Y, E, ProductTransducerAsync<X, Y, E, A>> ffa) =>
        new ProductFlattenTransducerAsync<X, Y, E, A>(ffa);

    public static ProductTransducerAsync<X, Y, A, B> flatten<X, Y, A, B>(
        ProductTransducerAsync<X, Y, A, ProductTransducerAsync<X, Y, Unit, B>> f) =>
        new ProductFlattenTransducerAsyncUnit<X, Y, A, B>(f);

    public static ProductTransducerAsync<X, Y, E, A> flatten<X, Y, E, A>(
        ProductTransducerAsync<X, Y, E, ProductTransducer<X, Y, E, A>> ffa) =>
        new ProductFlattenTransducerAsyncSync<X, Y, E, A>(ffa);

    public static ProductTransducerAsync<X, Y, A, B> flatten<X, Y, A, B>(
        ProductTransducerAsync<X, Y, A, ProductTransducer<X, Y, Unit, B>> f) =>
        new ProductFlattenTransducerAsyncSyncUnit<X, Y, A, B>(f);

    public static ProductTransducerAsync<X, X, E, B> bind<E, X, A, B>(
        ProductTransducerAsync<X, X, E, A> ma,
        ProductTransducerAsync<X, X, A, ProductTransducerAsync<X, X, E, B>> bind) =>
        new BindProductTransducerAsync1<X, E, A, B>(ma, bind);

    public static ProductTransducerAsync<X, X, E, B> bind<E, X, A, B>(
        ProductTransducerAsync<X, X, E, A> ma,
        Func<A, ProductTransducerAsync<X, X, E, B>> bind) =>
        new BindProductTransducerAsync2<X, E, A, B>(ma, bind);

    public static ProductTransducerAsync<X, X, E, B> bind<E, X, A, B>(
        ProductTransducerAsync<X, X, E, A> ma,
        Func<A, ProductTransducer<X, X, E, B>> bind) =>
        new BindProductTransducerAsyncSync2<X, E, A, B>(ma, bind);

    public static ProductTransducerAsync<X, X, E, C> bindMap<E, X, A, B, C>(
        ProductTransducerAsync<X, X, E, A> ma,
        ProductTransducerAsync<X, X, A, ProductTransducerAsync<X, X, E, B>> bind,
        ProductTransducerAsync<X, X, A, ProductTransducerAsync<X, X, B, C>> map) =>
        new SelectManyProductTransducerAsync1<X, E, A, B, C>(ma, bind, map);

    public static ProductTransducerAsync<X, X, E, C> bindMap<E, X, A, B, C>(
        ProductTransducerAsync<X, X, E, A> ma,
        ProductTransducerAsync<X, X, A, ProductTransducer<X, X, E, B>> bind,
        ProductTransducerAsync<X, X, A, ProductTransducer<X, X, B, C>> map) =>
        new SelectManyProductTransducerAsyncSync1<X, E, A, B, C>(ma, bind, map);

    public static ProductTransducerAsync<X, X, E, C> bindMap<E, X, A, B, C>(
        ProductTransducerAsync<X, X, E, A> ma,
        ProductTransducerAsync<X, X, A, ProductTransducerAsync<X, X, E, B>> bind,
        Func<A, B, C> map) =>
        new SelectManyProductTransducerAsync2<X, E, A, B, C>(ma, bind, map);

    public static ProductTransducerAsync<X, X, E, C> bindMap<E, X, A, B, C>(
        ProductTransducerAsync<X, X, E, A> ma,
        Func<A, ProductTransducerAsync<X, X, E, B>> bind,
        Func<A, B, C> map) =>
        new SelectManyProductTransducerAsync3<X, E, A, B, C>(ma, bind, map);    
    
}
