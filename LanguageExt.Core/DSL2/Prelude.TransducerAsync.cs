#nullable enable
using System;
using static LanguageExt.Prelude;

namespace LanguageExt.DSL2;

public static class TransducerAsync
{
    public static TransducerAsync<A, TransducerAsync<B, C>> curry<A, B, C>(Func<A, B, C> f) =>
        new CurryTransducerAsync2<A, B, C>(f);

    public static TransducerAsync<(X, A), (Y, B)> merge<X, Y, A, B>(ProductTransducerAsync<X, Y, A, B> t) =>
        new ProductMergeTransducerAsync<X, Y, A, B>(t);

    public static TransducerAsync<Sum<X, A>, Sum<Y, B>> merge<X, Y, A, B>(SumTransducerAsync<X, Y, A, B> t) =>
        new SumMergeTransducerAsync<X, Y, A, B>(t);

    public static TransducerAsync<A, B> map<A, B>(Func<A, B> f) => 
        new FunctorTransducerAsync<A, B>(f);

    public static TransducerAsync<A, A> filter<A>(Func<A, bool> f) =>
        new FilterTransducerAsync<A>(map(f));
    
    public static TransducerAsync<A, A> filter<A>(Transducer<A, bool> f) =>
        new FilterTransducerAsync<A>(f.ToAsync());
    
    public static TransducerAsync<A, A> filter<A>(TransducerAsync<A, bool> f) =>
        new FilterTransducerAsync<A>(f);
    
    public static TransducerAsync<A, A> identity<A>() => 
        map<A, A>(static x => x);
    
    public static TransducerAsync<A, B> constant<A, B>(B value) => 
        map<A, B>(_ => value);

    public static TransducerAsync<A, C> compose<A, B, C>(
        TransducerAsync<A, B> tab, 
        TransducerAsync<B, C> tbc) =>
        new ComposeTransducerAsync<A, B, C>(tab, tbc);

    public static TransducerAsync<A, D> compose<A, B, C, D>(
        TransducerAsync<A, B> tab, 
        TransducerAsync<B, C> tbc, 
        TransducerAsync<C, D> tcd) =>
        compose(compose(tab, tbc), tcd);

    public static TransducerAsync<A, E> compose<A, B, C, D, E>(
        TransducerAsync<A, B> tab, 
        TransducerAsync<B, C> tbc, 
        TransducerAsync<C, D> tcd, 
        TransducerAsync<D, E> tde) =>
        compose(compose(tab, tbc), compose(tcd, tde));
    
    public static TransducerAsync<E, B> action<E, A, B>(
        TransducerAsync<E, A> fa, 
        TransducerAsync<E, B> fb) =>
        new ActionTransducerAsync1<E, A, B>(fa, fb);

    public static TransducerAsync<E, B> apply<E, A, B>(
        TransducerAsync<E, TransducerAsync<A, B>> ff, 
        TransducerAsync<E, A> fa) =>
        new ApplyTransducerAsync1<E, A, B>(ff, fa);
    
    public static TransducerAsync<E, B> apply<E, A, B>(
        TransducerAsync<E, Transducer<A, B>> ff, 
        TransducerAsync<E, A> fa) =>
        new ApplyTransducerAsyncSync1<E, A, B>(ff, fa);
    
    public static TransducerAsync<E, B> apply<E, A, B>(
        TransducerAsync<E, Func<A, B>> ff, 
        TransducerAsync<E, A> fa) =>
        new ApplyTransducerAsync2<E, A, B>(ff, fa);

    public static TransducerAsync<A, B> flatten<A, B>(TransducerAsync<A, TransducerAsync<A, B>> f) => 
        new FlattenTransducerAsync<A, B>(f);
    
    public static TransducerAsync<A, B> flatten<A, B>(TransducerAsync<A, TransducerAsync<Unit, B>> f) => 
        new FlattenTransducerAsyncUnit<A, B>(f);
 
    public static TransducerAsync<A, B> flatten<A, B>(TransducerAsync<A, Transducer<A, B>> f) => 
        new FlattenTransducerAsyncSync<A, B>(f);
    
    public static TransducerAsync<A, B> flatten<A, B>(TransducerAsync<A, Transducer<Unit, B>> f) => 
        new FlattenTransducerAsyncSyncUnit<A, B>(f);

    public static TransducerAsync<E, B> bind<E, A, B>(
        TransducerAsync<E, A> ma,
        TransducerAsync<A, TransducerAsync<E, B>> bind) =>
        new BindTransducerAsync1<E, A, B>(ma, bind);

    public static TransducerAsync<E, B> bind<E, A, B>(
        TransducerAsync<E, A> ma,
        Func<A, TransducerAsync<E, B>> bind) =>
        new BindTransducerAsync2<E, A, B>(ma, bind);

    public static TransducerAsync<E, B> bind<E, A, B>(
        TransducerAsync<E, A> ma,
        TransducerAsync<A, Transducer<E, B>> bind) =>
        new BindTransducerAsyncSync1<E, A, B>(ma, bind);

    public static TransducerAsync<E, C> bindMap<E, A, B, C>(
        TransducerAsync<E, A> ma,
        TransducerAsync<A, TransducerAsync<E, B>> bind,
        TransducerAsync<A, TransducerAsync<B, C>> map) =>
        new SelectManyTransducerAsync1<E, A, B, C>(ma, bind, map);

    public static TransducerAsync<E, C> bindMap<E, A, B, C>(
        TransducerAsync<E, A> ma,
        TransducerAsync<A, Transducer<E, B>> bind,
        TransducerAsync<A, Transducer<B, C>> map) =>
        new SelectManyTransducerAsyncSync1<E, A, B, C>(ma, bind, map);

    public static TransducerAsync<E, C> bindMap<E, A, B, C>(
        TransducerAsync<E, A> ma,
        TransducerAsync<A, TransducerAsync<E, B>> bind,
        Func<A, B, C> map) =>
        new SelectManyTransducerAsync2<E, A, B, C>(ma, bind, map);

    public static TransducerAsync<E, C> bindMap<E, A, B, C>(
        TransducerAsync<E, A> ma,
        Func<A, TransducerAsync<E, B>> bind,
        Func<A, B, C> map) =>
        new SelectManyTransducerAsync3<E, A, B, C>(ma, bind, map);

    public static TransducerAsync<E, B> fold<E, A, B>(
        TransducerAsync<A, TransducerAsync<B, B>> fold,
        B state,
        TransducerAsync<E, A> ta) =>
        new FoldTransducerAsync<E, A, B>(fold, state, ta, (_, _) => true, Schedule.Never);

    public static TransducerAsync<E, B> fold<E, A, B>(
        TransducerAsync<A, Transducer<B, B>> fold, 
        B state, 
        TransducerAsync<E, A> ta) =>
        new FoldTransducerAsyncSync<E, A, B>(fold, state, ta, (_, _) => true, Schedule.Never);

    public static TransducerAsync<E, B> fold<E, A, B>(Func<A, B, B> fold, B state, TransducerAsync<E, A> ta) =>
        new FoldTransducerAsync2<E, A, B>(fold, state, ta, (_, _) => true, Schedule.Never);
    

    public static TransducerAsync<E, B> foldWhile<E, A, B>(
        TransducerAsync<A, TransducerAsync<B, B>> fold,
        B state,
        TransducerAsync<E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        new FoldTransducerAsync<E, A, B>(fold, state, ta, (x, _) => pred(x), schedule);

    public static TransducerAsync<E, B> foldWhile<E, A, B>(
        TransducerAsync<A, Transducer<B, B>> fold, 
        B state, 
        TransducerAsync<E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        new FoldTransducerAsyncSync<E, A, B>(fold, state, ta, (x, _) => pred(x), schedule);

    public static TransducerAsync<E, B> foldWhile<E, A, B>(
        Func<A, B, B> fold, 
        B state, 
        TransducerAsync<E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        new FoldTransducerAsync2<E, A, B>(fold, state, ta, (x, _) => pred(x), schedule);

    public static TransducerAsync<E, B> foldUntil<E, A, B>(
        TransducerAsync<A, TransducerAsync<B, B>> fold,
        B state,
        TransducerAsync<E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        new FoldTransducerAsync<E, A, B>(fold, state, ta, (x, _) => !pred(x), schedule);

    public static TransducerAsync<E, B> foldUntil<E, A, B>(
        TransducerAsync<A, Transducer<B, B>> fold, 
        B state, 
        TransducerAsync<E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        new FoldTransducerAsyncSync<E, A, B>(fold, state, ta, (x, _) => !pred(x), schedule);

    public static TransducerAsync<E, B> foldUntil<E, A, B>(
        Func<A, B, B> fold, 
        B state, 
        TransducerAsync<E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        new FoldTransducerAsync2<E, A, B>(fold, state, ta, (x, _) => !pred(x), schedule);
    
}
