#nullable enable
using System;
using System.Collections.Generic;
using static LanguageExt.Prelude;

namespace LanguageExt.DSL.Transducers;

public interface Transducer<in A, out B>
{
    Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, B, TResult<S>> reduce);
}

public static class Transducer<A>
{
    public static readonly Transducer<A, A> identity = Transducer.map<A, A>(static x => x);
    public static readonly Transducer<Obj<A>, A> join = new JoinObjTransducer<A>();
    public static readonly Transducer<A, A> head = new TakeTransducer<A>(1);
    public static readonly Transducer<A, A> tail = new SkipTransducer<A>(1);
    public static readonly Transducer<A, Unit> release = new ReleaseTransducer<A>();
    public static readonly Transducer<IObservable<A>, A> observable = new ObservableTransducer<A>();
    public static readonly Transducer<IEnumerable<A>, A> enumerable = new EnumerableTransducer<A>();
}

public static class TransducerD<A> where A : IDisposable
{
    public static Transducer<A, A> use = new UseTransducer<A>();
}

public static class Transducer
{
    public static Prim<B> Apply<A, B>(this Transducer<A, B> mf, A ma) =>
        #nullable disable
        mf.Transform<Prim<B>>(static (s, x) => TResult.Continue(s.Value + x))(TState<Prim<B>>.Create(Prim<B>.None), ma)
          .Match(Complete: identity, Continue: identity, Fail: Prim.Fail<B>);
        #nullable enable

    public static Transducer<Unit, B> Inject<A, B>(this Transducer<A, B> mf, A x) =>
        compose(constant<Unit, A>(x), mf);

    public static Transducer<A, C> compose<A, B, C>(Transducer<A, B> tab, Transducer<B, C> tbc) =>
        new ComposeTransducer<A, B, C>(tab, tbc);

    public static Transducer<A, B> constant<A, B>(B @const) =>
        new ConstantTransducer<A, B>(@const);
    
    public static Transducer<A, B> map<A, B>(Func<A, B> f) =>
        new MapTransducer<A, B>(f);
    
    public static Transducer<A, A> filter<A>(Func<A, bool> f) =>
        new FilterTransducer<A>(f);

    public static Transducer<A, A> use<A>(Action<A> dispose) => 
        new UseTransducer2<A>(dispose);
    
    public static Transducer<A, A> use<A>(Func<A, Unit> dispose) => 
        new UseTransducer2<A>(x => dispose(x));
    
    public static Transducer<A, A> skipWhile<A>(Func<A, bool> predicate) =>
        new SkipWhileTransducer<A>(predicate);

    public static Transducer<A, A> skipUntil<A>(Func<A, bool> predicate) =>
        new SkipWhileTransducer<A>(predicate);

    public static Transducer<A, A> skip<A>(int count) =>
        new SkipTransducer<A>(count);

    public static Transducer<A, A> takeWhile<A>(Func<A, bool> predicate) =>
        new TakeWhileTransducer<A>(predicate);

    public static Transducer<A, A> takeUntil<A>(Func<A, bool> predicate) =>
        new TakeWhileTransducer<A>(predicate);

    public static Transducer<A, A> take<A>(int count) =>
        new TakeTransducer<A>(count);

    public static Transducer<E, CoProduct<X, B>> kleisli<E, X, A, B>(
        Transducer<E, CoProduct<X, A>> first,
        Transducer<A, CoProduct<X, B>> second) =>
        new KleisliTransducer<E, X, A, B>(first, second);

    public static Transducer<E, CoProduct<X, B>> kleisli<E, X, A, B>(
        Transducer<E, CoProduct<X, A>> first,
        Func<A, CoProduct<X, B>> second) =>
        new KleisliTransducer<E, X, A, B>(first, map(second));

    public static Transducer<E, CoProduct<X, B>> kleisli<E, X, A, B>(
        Transducer<E, CoProduct<X, A>> first,
        Transducer<A, Transducer<E, CoProduct<X, B>>> second) =>
        new KleisliTransducer2<E, X, A, B>(first, second);

    public static Transducer<E, CoProduct<X, B>> kleisli<E, X, A, B>(
        Transducer<E, CoProduct<X, A>> first,
        Func<A, Transducer<E, CoProduct<X, B>>> second) =>
        new KleisliTransducer2<E, X, A, B>(first, map(second));

    public static Transducer<E, CoProduct<X, B>> kleisli<E, X, A, B>(
        Func<E, CoProduct<X, A>> first,
        Func<A, CoProduct<X, B>> second) =>
        new KleisliTransducer<E, X, A, B>(map(first), map(second));

    
    public static Transducer<E, CoProduct<X, B>> kleisli<E, X, A, B>(
        Transducer<E, CoProduct<X, A>> first,
        BiTransducer<X, X, A, B> second) =>
        new BiKleisliTransducer<E, X, X, A, B>(first, second);

    public static Transducer<E, CoProduct<X, B>> kleisli<E, X, A, B>(
        Func<E, CoProduct<X, A>> first,
        BiTransducer<X, X, A, B> second) =>
        new BiKleisliTransducer<E, X, X, A, B>(map(first), second);

    public static Transducer<E, CoProduct<X, B>> bind<E, X, A, B, MB>(
        Transducer<E, CoProduct<X, A>> first,
        Func<A, MB> second) where MB : IsTransducer<E, CoProduct<X, B>> =>
        new KleisliTransducer2<E, X, A, B>(first, map<A, Transducer<E, CoProduct<X, B>>>(a => second(a).ToTransducer()));
    
    public static Transducer<A, CoProduct<X, A>> right<X, A>() =>
        map<A, CoProduct<X, A>>(CoProduct.Right<X, A>);
    
    public static Transducer<X, CoProduct<X, A>> left<X, A>() =>
        map<X, CoProduct<X, A>>(CoProduct.Left<X, A>);
}
