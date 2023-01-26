#nullable enable
using System;

namespace LanguageExt.DSL2;

/// <summary>
/// Async co-product category
/// </summary>
public static class SumAsync
{
    public static TransducerAsync<A, Sum<X, A>> right<X, A>() =>
        TransducerAsync.map<A, Sum<X, A>>(Sum<X, A>.Right);
    
    public static TransducerAsync<X, Sum<X, A>> left<X, A>() =>
        TransducerAsync.map<X, Sum<X, A>>(Sum<X, A>.Left);

    public static TransducerAsync<A, Sum<X, A>> constRight<X, A>(A value) =>
        TransducerAsync.map<A, Sum<X, A>>(Sum<X, A>.Right);
    
    public static TransducerAsync<X, Sum<X, A>> constLeft<X, A>(X value) =>
        TransducerAsync.map<X, Sum<X, A>>(Sum<X, A>.Left);

    public static SumTransducerAsync<X, Z, A, C> compose<X, Y, Z, A, B, C>(
        SumTransducerAsync<X, Y, A, B> tab, 
        SumTransducerAsync<Y, Z, B, C> tbc) =>
        new SumComposeTransducerAsync<X, Y, Z, A, B, C>(tab, tbc);

    public static SumTransducerAsync<W, Z, A, D> compose<W, X, Y, Z, A, B, C, D>(
        SumTransducerAsync<W, X, A, B> tab, 
        SumTransducerAsync<X, Y, B, C> tbc, 
        SumTransducerAsync<Y, Z, C, D> tcd) =>
        compose(compose(tab, tbc), tcd);

    public static SumTransducerAsync<V, Z, A, E> compose<V, W, X, Y, Z, A, B, C, D, E>(
        SumTransducerAsync<V, W, A, B> tab, 
        SumTransducerAsync<W, X, B, C> tbc, 
        SumTransducerAsync<X, Y, C, D> tcd, 
        SumTransducerAsync<Y, Z, D, E> tde) =>
        compose(compose(tab, tbc), compose(tcd, tde));

    public static SumTransducerAsync<X, Y, A, A> mapLeft<X, Y, A>(TransducerAsync<X, Y> left) => 
        bimap(left, TransducerAsync.identity<A>());

    public static SumTransducerAsync<X, Y, A, A> mapLeft<X, Y, A>(Func<X, Y> left) => 
        bimap(TransducerAsync.map(left), TransducerAsync.identity<A>());

    public static SumTransducerAsync<X, X, A, B> mapRight<X, A, B>(TransducerAsync<A, B> right) => 
        bimap(TransducerAsync.identity<X>(), right);

    public static SumTransducerAsync<X, X, A, B> mapRight<X, A, B>(Func<A, B> right) => 
        bimap(TransducerAsync.identity<X>(), TransducerAsync.map(right));

    public static SumTransducerAsync<X, Y, A, B> bimap<X, Y, A, B>(TransducerAsync<X, Y> left, TransducerAsync<A, B> right) => 
        new SumBiFunctorTransducerAsync<X, Y, A, B>(left, right);

    public static SumTransducerAsync<X, Y, A, B> bimap<X, Y, A, B>(Func<X, Y> left, Func<A, B> right) => 
        bimap(TransducerAsync.map(left), TransducerAsync.map(right));
    
    public static SumTransducerAsync<X, X, A, A> identity<X, A>() =>
        bimap(TransducerAsync.identity<X>(), TransducerAsync.identity<A>());

    public static SumTransducerAsync<X, Y, A, B> expand<X, Y, A, B>(TransducerAsync<Sum<X, A>, Sum<Y, B>>  t) =>
        new SumExpandTransducerAsync<X, Y, A, B>(t);
 
    public static SumTransducerAsync<X, X, E, B> action<E, X, A, B>(
        SumTransducerAsync<X, X, E, A> fa, 
        SumTransducerAsync<X, X, E, B> fb) =>
        new ActionSumTransducerAsync1<E, X, A, B>(fa, fb);

    public static SumTransducerAsync<X, X, E, B> apply<E, X, A, B>(
        SumTransducerAsync<X, X, E, TransducerAsync<A, B>> ff, 
        SumTransducerAsync<X, X, E, A> fa) =>
        new ApplySumTransducerAsync1<E, X, A, B>(ff, fa);
    
    public static SumTransducerAsync<X, X, E, B> apply<E, X, A, B>(
        SumTransducerAsync<X, X, E, Transducer<A, B>> ff, 
        SumTransducerAsync<X, X, E, A> fa) =>
        new ApplySumTransducerAsyncSync1<E, X, A, B>(ff, fa);
    
    public static SumTransducerAsync<X, X, E, B> apply<E, X, A, B>(
        SumTransducerAsync<X, X, E, Func<A, B>> ff, 
        SumTransducerAsync<X, X, E, A> fa) =>
        new ApplySumTransducerAsync2<E, X, A, B>(ff, fa);
 
    public static SumTransducerAsync<X, Y, A, B> flatten<X, Y, A, B>(SumTransducerAsync<X, Y, A, SumTransducerAsync<X, Y, A, B>> f) => 
        new SumFlattenTransducerAsync<X, Y, A, B>(f);
        
    public static SumTransducerAsync<X, Y, A, B> flatten<X, Y, A, B>(SumTransducerAsync<X, Y, A, SumTransducerAsync<X, Y, Unit, B>> f) => 
        new SumFlattenTransducerAsyncUnit<X, Y, A, B>(f);
 
    public static SumTransducerAsync<X, Y, A, B> flatten<X, Y, A, B>(SumTransducerAsync<X, Y, A, SumTransducer<X, Y, A, B>> f) => 
        new SumFlattenTransducerAsyncSync<X, Y, A, B>(f);
        
    public static SumTransducerAsync<X, Y, A, B> flatten<X, Y, A, B>(SumTransducerAsync<X, Y, A, SumTransducer<X, Y, Unit, B>> f) => 
        new SumFlattenTransducerAsyncSyncUnit<X, Y, A, B>(f);

    public static SumTransducerAsync<X, X, RT, B> bind<RT, X, A, B, MXB>(SumTransducerAsync<X, X, RT, A> ma, Func<A, MXB> f)
        where MXB : IsSumTransducerAsync<X, X, RT, B> =>
        flatten(
            compose(
                ma, 
                mapRight<X, A, SumTransducerAsync<X, X, RT, B>>(x => f(x).ToSumTransducerAsync())));

    public static SumTransducerAsync<X, X, E, B> bind<E, X, A, B>(
        SumTransducerAsync<X, X, E, A> ma,
        SumTransducerAsync<X, X, A, SumTransducerAsync<X, X, E, B>> bind) =>
        new BindSumTransducerAsync1<X, E, A, B>(ma, bind);

    public static SumTransducerAsync<X, X, E, B> bind<E, X, A, B>(
        SumTransducerAsync<X, X, E, A> ma,
        Func<A, SumTransducerAsync<X, X, E, B>> bind) =>
        new BindSumTransducerAsync2<X, E, A, B>(ma, bind);

    public static SumTransducerAsync<X, X, E, B> bind<E, X, A, B>(
        SumTransducerAsync<X, X, E, A> ma,
        Func<A, SumTransducer<X, X, E, B>> bind) =>
        new BindSumTransducerAsyncSync2<X, E, A, B>(ma, bind);

    public static SumTransducerAsync<X, X, E, C> bindMap<E, X, A, B, C>(
        SumTransducerAsync<X, X, E, A> ma,
        SumTransducerAsync<X, X, A, SumTransducerAsync<X, X, E, B>> bind,
        SumTransducerAsync<X, X, A, SumTransducerAsync<X, X, B, C>> map) =>
        new SelectManySumTransducerAsync1<X, E, A, B, C>(ma, bind, map);

    public static SumTransducerAsync<X, X, E, C> bindMap<E, X, A, B, C>(
        SumTransducerAsync<X, X, E, A> ma,
        SumTransducerAsync<X, X, A, SumTransducer<X, X, E, B>> bind,
        SumTransducerAsync<X, X, A, SumTransducer<X, X, B, C>> map) =>
        new SelectManySumTransducerAsyncSync1<X, E, A, B, C>(ma, bind, map);

    public static SumTransducerAsync<X, X, E, C> bindMap<E, X, A, B, C>(
        SumTransducerAsync<X, X, E, A> ma,
        SumTransducerAsync<X, X, A, SumTransducerAsync<X, X, E, B>> bind,
        Func<A, B, C> map) =>
        new SelectManySumTransducerAsync2<X, E, A, B, C>(ma, bind, map);

    public static SumTransducerAsync<X, X, E, C> bindMap<E, X, A, B, C>(
        SumTransducerAsync<X, X, E, A> ma,
        Func<A, SumTransducerAsync<X, X, E, B>> bind,
        Func<A, B, C> map) =>
        new SelectManySumTransducerAsync3<X, E, A, B, C>(ma, bind, map);

    public static SumTransducerAsync<X, Y, E, B> fold<E, X, Y, A, B>(
        TransducerAsync<A, TransducerAsync<B, B>> fold, 
        B state, 
        SumTransducerAsync<X, Y, E, A> ta,
        Schedule schedule) =>
        new SumFoldTransducerAsync<E, X, Y, A, B>(fold, state, ta, (_, _) => true, schedule);

    public static SumTransducerAsync<X, Y, E, B> fold<E, X, Y, A, B>(
        Func<A, B, B> fold, 
        B state, 
        SumTransducerAsync<X, Y, E, A> ta,
        Schedule schedule) =>
        new SumFoldTransducerAsync<E, X, Y, A, B>(TransducerAsync.curry(fold), state, ta, (_, _) => true, schedule);

    public static SumTransducerAsync<X, Y, E, B> foldWhile<E, X, Y, A, B>(
        TransducerAsync<A, TransducerAsync<B, B>> fold, 
        B state, 
        SumTransducerAsync<X, Y, E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        new SumFoldTransducerAsync<E, X, Y, A, B>(fold, state, ta, (x, _) => pred(x), schedule);

    public static SumTransducerAsync<X, Y, E,  B> foldWhile<E, X, Y, A, B>(
        Func<A, B, B> fold, 
        B state, 
        SumTransducerAsync<X, Y, E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        new SumFoldTransducerAsync<E, X, Y, A, B>(TransducerAsync.curry(fold), state, ta, (x, _) => pred(x), schedule);

    public static SumTransducerAsync<X, Y, E, B> foldUntil<E, X, Y, A, B>(
        TransducerAsync<A, TransducerAsync<B, B>> fold, 
        B state, 
        SumTransducerAsync<X, Y, E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        new SumFoldTransducerAsync<E, X, Y, A, B>(fold, state, ta, (x, _) => !pred(x), schedule);

    public static SumTransducerAsync<X, Y, E, B> foldUntil<E, X, Y, A, B>(
        Func<A, B, B> fold, 
        B state, 
        SumTransducerAsync<X, Y, E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        new SumFoldTransducerAsync<E, X, Y, A, B>(TransducerAsync.curry(fold), state, ta, (x, _) => !pred(x), schedule);      
    
    
    /*
    public static SumTransducerAsync<X, X, A, C> kleisli<X, A, B, C>(
        TransducerAsync<A, Sum<X, B>> mf,
        TransducerAsync<B, Sum<X, C>> mg) =>
        compose(bind(mf), bind(mg));
*/
}

