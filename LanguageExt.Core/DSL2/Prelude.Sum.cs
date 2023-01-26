#nullable enable
using System;

namespace LanguageExt.DSL2;

public static class Sum
{
    public static Transducer<A, Sum<X, A>> right<X, A>() =>
        Transducer.map<A, Sum<X, A>>(Sum<X, A>.Right);
    
    public static Transducer<X, Sum<X, A>> left<X, A>() =>
        Transducer.map<X, Sum<X, A>>(Sum<X, A>.Left);
    
    public static SumTransducer<X, X, A, A> identity<X, A>() =>
        bimap(Transducer.identity<X>(), Transducer.identity<A>());

    public static SumTransducer<X, X, A, B> constRight<X, A, B>(B value) =>
        mapRight<X, A, B>(_ => value);

    public static SumTransducer<X, Y, A, A> constLeft<X, Y, A>(Y value) =>
        mapLeft<X, Y, A>(_ => value);

    public static SumTransducer<X, Z, A, C> compose<X, Y, Z, A, B, C>(
        SumTransducer<X, Y, A, B> tab, 
        SumTransducer<Y, Z, B, C> tbc) =>
        new SumComposeTransducer<X, Y, Z, A, B, C>(tab, tbc);

    public static SumTransducer<W, Z, A, D> compose<W, X, Y, Z, A, B, C, D>(
        SumTransducer<W, X, A, B> tab, 
        SumTransducer<X, Y, B, C> tbc, 
        SumTransducer<Y, Z, C, D> tcd) =>
        compose(compose(tab, tbc), tcd);

    public static SumTransducer<V, Z, A, E> compose<V, W, X, Y, Z, A, B, C, D, E>(
        SumTransducer<V, W, A, B> tab, 
        SumTransducer<W, X, B, C> tbc, 
        SumTransducer<X, Y, C, D> tcd, 
        SumTransducer<Y, Z, D, E> tde) =>
        compose(compose(tab, tbc), compose(tcd, tde));

    public static SumTransducer<X, Y, A, B> expand<X, Y, A, B>(Transducer<Sum<X, A>, Sum<Y, B>>  t) =>
        new SumExpandTransducer<X, Y, A, B>(t);
 
    public static SumTransducer<X, Y, A, A> mapLeft<X, Y, A>(Transducer<X, Y> Left) => 
        bimap(Left, Transducer.identity<A>());

    public static SumTransducer<X, Y, A, A> mapLeft<X, Y, A>(Func<X, Y> Left) => 
        bimap(Transducer.map(Left), Transducer.identity<A>());

    public static SumTransducer<X, X, A, B> mapRight<X, A, B>(Transducer<A, B> Right) => 
        bimap(Transducer.identity<X>(), Right);

    public static SumTransducer<X, X, A, B> mapRight<X, A, B>(Func<A, B> Right) => 
        bimap(Transducer.identity<X>(), Transducer.map(Right));

    public static SumTransducer<X, Y, A, B> bimap<X, Y, A, B>(Transducer<X, Y> Left, Transducer<A, B> Right) => 
        new SumBiFunctorTransducer<X, Y, A, B>(Left, Right);

    public static SumTransducer<X, Y, A, B> bimap<X, Y, A, B>(Func<X, Y> Left, Func<A, B> Right) => 
        bimap(Transducer.map(Left), Transducer.map(Right));

    public static SumTransducer<X, X, E, B> action<E, X, A, B>(
        SumTransducer<X, X, E, A> fa, 
        SumTransducer<X, X, E, B> fb) =>
        new ActionSumTransducer1<E, X, A, B>(fa, fb);

    public static SumTransducer<X, X, E, B> apply<E, X, A, B>(
        SumTransducer<X, X, E, Transducer<A, B>> ff, 
        SumTransducer<X, X, E, A> fa) =>
        new ApplySumTransducer1<E, X, A, B>(ff, fa);
    
    public static SumTransducer<X, X, E, B> apply<E, X, A, B>(
        SumTransducer<X, X, E, Func<A, B>> ff, 
        SumTransducer<X, X, E, A> fa) =>
        new ApplySumTransducer2<E, X, A, B>(ff, fa);
 
    public static SumTransducer<X, Y, A, B> flatten<X, Y, A, B>(SumTransducer<X, Y, A, SumTransducer<X, Y, A, B>> f) => 
        new SumFlattenTransducer<X, Y, A, B>(f);
 
    public static SumTransducer<X, Y, A, B> flatten<X, Y, A, B>(SumTransducer<X, Y, A, SumTransducer<X, Y, Unit, B>> f) => 
        new SumFlattenTransducerUnit<X, Y, A, B>(f);
    
    public static SumTransducer<X, X, RT, B> bind<RT, X, A, B, MXB>(SumTransducer<X, X, RT, A> ma, Func<A, MXB> f)
        where MXB : IsSumTransducer<X, X, RT, B> =>
        flatten(
            compose(
                ma, 
                mapRight<X, A, SumTransducer<X, X, RT, B>>(x => f(x).ToSumTransducer())));

    public static SumTransducer<X, X, E, B> bind<E, X, A, B>(
        SumTransducer<X, X, E, A> ma,
        SumTransducer<X, X, A, SumTransducer<X, X, E, B>> bind) =>
        new BindSumTransducer1<X, E, A, B>(ma, bind);

    public static SumTransducer<X, X, E, B> bind<E, X, A, B>(
        SumTransducer<X, X, E, A> ma,
        Func<A, SumTransducer<X, X, E, B>> bind) =>
        new BindSumTransducer2<X, E, A, B>(ma, bind);

    public static SumTransducer<X, X, E, C> bindMap<E, X, A, B, C>(
        SumTransducer<X, X, E, A> ma,
        SumTransducer<X, X, A, SumTransducer<X, X, E, B>> bind,
        SumTransducer<X, X, A, SumTransducer<X, X, B, C>> map) =>
        new SelectManySumTransducer1<X, E, A, B, C>(ma, bind, map);

    public static SumTransducer<X, X, E, C> bindMap<E, X, A, B, C>(
        SumTransducer<X, X, E, A> ma,
        SumTransducer<X, X, A, SumTransducer<X, X, E, B>> bind,
        Func<A, B, C> map) =>
        new SelectManySumTransducer2<X, E, A, B, C>(ma, bind, map);

    public static SumTransducer<X, X, E, C> bindMap<E, X, A, B, C>(
        SumTransducer<X, X, E, A> ma,
        Func<A, SumTransducer<X, X, E, B>> bind,
        Func<A, B, C> map) =>
        new SelectManySumTransducer3<X, E, A, B, C>(ma, bind, map);    

    public static SumTransducer<X, Y, E, B> fold<E, X, Y, A, B>(
        Transducer<A, Transducer<B, B>> fold, 
        B state, 
        SumTransducer<X, Y, E, A> ta,
        Schedule schedule) =>
        new SumFoldTransducer<E, X, Y, A, B>(fold, state, ta, (_, _) => true, schedule);

    public static SumTransducer<X, Y, E, B> fold<E, X, Y, A, B>(
        Func<A, B, B> fold, 
        B state, 
        SumTransducer<X, Y, E, A> ta,
        Schedule schedule) =>
        new SumFoldTransducer<E, X, Y, A, B>(Transducer.curry(fold), state, ta, (_, _) => true, schedule);

    public static SumTransducer<X, Y, E, B> foldWhile<E, X, Y, A, B>(
        Transducer<A, Transducer<B, B>> fold, 
        B state, 
        SumTransducer<X, Y, E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        new SumFoldTransducer<E, X, Y, A, B>(fold, state, ta, (x, _) => pred(x), schedule);

    public static SumTransducer<X, Y, E,  B> foldWhile<E, X, Y, A, B>(
        Func<A, B, B> fold, 
        B state, 
        SumTransducer<X, Y, E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        new SumFoldTransducer<E, X, Y, A, B>(Transducer.curry(fold), state, ta, (x, _) => pred(x), schedule);

    public static SumTransducer<X, Y, E, B> foldUntil<E, X, Y, A, B>(
        Transducer<A, Transducer<B, B>> fold, 
        B state, 
        SumTransducer<X, Y, E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        new SumFoldTransducer<E, X, Y, A, B>(fold, state, ta, (x, _) => !pred(x), schedule);

    public static SumTransducer<X, Y, E, B> foldUntil<E, X, Y, A, B>(
        Func<A, B, B> fold, 
        B state, 
        SumTransducer<X, Y, E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        new SumFoldTransducer<E, X, Y, A, B>(Transducer.curry(fold), state, ta, (x, _) => !pred(x), schedule);    
}
