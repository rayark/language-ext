#nullable enable
using System;
using static LanguageExt.Prelude;

namespace LanguageExt.DSL2;

// ---------------------------------------------------------------------------------------------------------------------

public static class Transducer
{
    public static Transducer<A, Transducer<B, C>> curry<A, B, C>(Func<A, B, C> f) =>
        new CurryTransducer2<A, B, C>(f);

    public static Transducer<(X, A), (Y, B)> merge<X, Y, A, B>(ProductTransducer<X, Y, A, B> t) =>
        new ProductMergeTransducer<X, Y, A, B>(t);

    public static Transducer<Sum<X, A>, Sum<Y, B>> merge<X, Y, A, B>(SumTransducer<X, Y, A, B> t) =>
        new SumMergeTransducer<X, Y, A, B>(t);

    public static Transducer<A, B> map<A, B>(Func<A, B> f) => 
        new FunctorTransducer<A, B>(f);
 
    public static Transducer<A, B> flatten<A, B>(Transducer<A, Transducer<A, B>> f) => 
        new FlattenTransducer<A, B>(f);
    
    public static Transducer<A, B> flatten<A, B>(Transducer<A, Transducer<Unit, B>> f) => 
        new FlattenTransducerUnit<A, B>(f);
    
    public static Transducer<A, A> filter<A>(Transducer<A, bool> f) =>
        new FilterTransducer<A>(f);
    
    public static Transducer<A, A> filter<A>(Func<A, bool> f) =>
        new FilterTransducer<A>(map(f));
    
    public static Transducer<A, A> identity<A>() => 
        map<A, A>(static x => x);
    
    public static Transducer<A, B> constant<A, B>(B value) => 
        map<A, B>(_ => value);

    public static Transducer<A, C> compose<A, B, C>(
        Transducer<A, B> tab, 
        Transducer<B, C> tbc) =>
        new ComposeTransducer<A, B, C>(tab, tbc);

    public static Transducer<A, D> compose<A, B, C, D>(
        Transducer<A, B> tab, 
        Transducer<B, C> tbc, 
        Transducer<C, D> tcd) =>
        compose(compose(tab, tbc), tcd);

    public static Transducer<A, E> compose<A, B, C, D, E>(
        Transducer<A, B> tab, 
        Transducer<B, C> tbc, 
        Transducer<C, D> tcd, 
        Transducer<D, E> tde) =>
        compose(compose(tab, tbc, tcd), tde);
    
    public static Transducer<E, B> action<E, A, B>(
        Transducer<E, A> fa, 
        Transducer<E, B> fb) =>
        new ActionTransducer1<E, A, B>(fa, fb);

    public static Transducer<E, B> apply<E, A, B>(
        Transducer<E, Transducer<A, B>> ff, 
        Transducer<E, A> fa) =>
        new ApplyTransducer1<E, A, B>(ff, fa);
    
    public static Transducer<E, B> apply<E, A, B>(
        Transducer<E, Func<A, B>> ff, 
        Transducer<E, A> fa) =>
        new ApplyTransducer2<E, A, B>(ff, fa);

    public static Transducer<E, B> bind<E, A, B>(
        Transducer<E, A> ma,
        Transducer<A, Transducer<E, B>> bind) =>
        new BindTransducer1<E, A, B>(ma, bind);

    public static Transducer<E, B> bind<E, A, B>(
        Transducer<E, A> ma,
        Func<A, Transducer<E, B>> bind) =>
        new BindTransducer2<E, A, B>(ma, bind);

    public static Transducer<E, C> bindMap<E, A, B, C>(
        Transducer<E, A> ma,
        Transducer<A, Transducer<E, B>> bind,
        Transducer<A, Transducer<B, C>> map) =>
        new SelectManyTransducer1<E, A, B, C>(ma, bind, map);

    public static Transducer<E, C> bindMap<E, A, B, C>(
        Transducer<E, A> ma,
        Transducer<A, Transducer<E, B>> bind,
        Func<A, B, C> map) =>
        new SelectManyTransducer2<E, A, B, C>(ma, bind, map);

    public static Transducer<E, C> bindMap<E, A, B, C>(
        Transducer<E, A> ma,
        Func<A, Transducer<E, B>> bind,
        Func<A, B, C> map) =>
        new SelectManyTransducer3<E, A, B, C>(ma, bind, map);

    public static Transducer<E, B> fold<E, A, B>(Transducer<A, Transducer<B, B>> fold, B state, Transducer<E, A> ta) =>
        new FoldTransducer<E, A, B>(fold, state, ta, (_, _) => true, Schedule.Never);

    public static Transducer<E, B> fold<E, A, B>(Func<A, B, B> fold, B state, Transducer<E, A> ta) =>
        new FoldTransducer2<E, A, B>(fold, state, ta, (_, _) => true, Schedule.Never);

    public static Transducer<E, B> foldWhile<E, A, B>(
        Transducer<A, Transducer<B, B>> fold, 
        B state, 
        Transducer<E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        new FoldTransducer<E, A, B>(fold, state, ta, (x, _) => pred(x), schedule);

    public static Transducer<E, B> foldWhile<E, A, B>(
        Func<A, B, B> fold, 
        B state, 
        Transducer<E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        new FoldTransducer2<E, A, B>(fold, state, ta, (x, _) => pred(x), schedule);

    public static Transducer<E, B> foldUntil<E, A, B>(
        Transducer<A, Transducer<B, B>> fold, 
        B state, 
        Transducer<E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        new FoldTransducer<E, A, B>(fold, state, ta, (x, _) => !pred(x), schedule);

    public static Transducer<E, B> foldUntil<E, A, B>(
        Func<A, B, B> fold, 
        B state, 
        Transducer<E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        new FoldTransducer2<E, A, B>(fold, state, ta, (x, _) => !pred(x), schedule);
}
