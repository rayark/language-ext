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
    /// <summary>
    /// Apply an argument to a transducer
    /// </summary>
    /// <remarks>Collects zero or more results</remarks>
    /// <param name="mf">Transducer</param>
    /// <param name="ma">Argument</param>
    /// <returns>`PrimPure<B> | PrimMany<B> | PrimFail<B>`</returns>
    public static Prim<B> Apply<A, B>(this Transducer<A, B> mf, A ma) =>
        apply(ma, mf);

    /// <summary>
    /// Apply an argument to a transducer
    /// </summary>
    /// <remarks>Collects zero or one results</remarks>
    /// <param name="mf">Transducer</param>
    /// <param name="ma">Argument</param>
    /// <returns>`PrimPure<B> | PrimFail<B>`</returns>
    public static Prim<B> Apply1<A, B>(this Transducer<A, B> mf, A ma) =>
        apply1(ma, mf);

    /// <summary>
    /// Composes a constant transducer that yields `x` with the supplied transducer `mf` 
    /// </summary>
    /// <remarks>
    /// Allows for a value to be injected into a composition without loses the transducer type (for further composition)
    /// </remarks>
    /// <param name="mf">Transducer</param>
    /// <param name="ma">Argument</param>
    /// <returns>Transducer that when invoked with `unit` will run the supplied transducer with the constant value `x`</returns>
    public static Transducer<Unit, B> Inject<A, B>(this Transducer<A, B> mf, A x) =>
        inject(x, mf);
    
    /// <summary>
    /// Apply an argument to a transducer
    /// </summary>
    /// <remarks>Collects zero or more results</remarks>
    /// <param name="x">Argument</param>
    /// <param name="mf">Transducer</param>
    /// <returns>`PrimPure<B> | PrimMany<B> | PrimFail<B>`</returns>
    public static Prim<B> apply<A, B>(A x, Transducer<A, B> mf) =>
        #nullable disable
        mf.Transform<Prim<B>>(static (s, x) => TResult.Continue(s.Value + x))(TState<Prim<B>>.Create(Prim<B>.None), x)
          .Match(Complete: identity, Continue: identity, Fail: Prim.Fail<B>);
        #nullable enable

    /// <summary>
    /// Apply an argument to a transducer
    /// </summary>
    /// <remarks>Collects zero or one results</remarks>
    /// <param name="x">Argument</param>
    /// <param name="mf">Transducer</param>
    /// <returns>`PrimPure<B> | PrimFail<B>`</returns>
    public static Prim<B> apply1<A, B>(A x, Transducer<A, B> mf) =>
        #nullable disable
        mf.Transform<Prim<B>>(static (_, x) => TResult.Continue(Prim.Pure(x)))(TState<Prim<B>>.Create(Prim<B>.None), x)
          .Match(Complete: identity, Continue: identity, Fail: Prim.Fail<B>);
        #nullable enable    

    /// <summary>
    /// Composes a constant transducer that yields `x` with the supplied transducer `mf` 
    /// </summary>
    /// <remarks>
    /// Allows for a value to be injected into a composition without loses the transducer type (for further composition)
    /// </remarks>
    /// <param name="ma">Argument</param>
    /// <param name="mf">Transducer</param>
    /// <returns>Transducer that when invoked with `unit` will run the supplied transducer with the constant value `x`</returns>
    public static Transducer<Unit, B> inject<A, B>(A x, Transducer<A, B> mf) =>
        compose(constant<Unit, A>(x), mf);

    public static Transducer<A, C> compose<A, B, C>(Transducer<A, B> tab, Transducer<B, C> tbc) =>
        new ComposeTransducer<A, B, C>(tab, tbc);

    public static Transducer<A, B> constant<A, B>(B @const) =>
        new ConstantTransducer<A, B>(@const);
    
    public static Transducer<X, CoProduct<A, B>> constantRight<X, A, B>(B @const) =>
        new ConstantRightTransducer<X, A, B>(@const);
    
    public static Transducer<X, CoProduct<A, B>> constantLeft<X, A, B>(A @const) =>
        new ConstantLeftTransducer<X, A, B>(@const);
    
    public static Transducer<A, B> map<A, B>(Func<A, B> f) =>
        new MapTransducer<A, B>(f);
    
    public static Transducer<CoProduct<X, A>, CoProduct<Y, B>> bimap<X, Y, A, B>(Func<X, Y> Left, Func<A, B> Right) =>
        new BiMapTransducer2<X, Y, A, B>(Left, Right);
    
    public static Transducer<CoProduct<X, A>, CoProduct<Y, B>> bimap<X, Y, A, B>(Transducer<X, Y> Left, Transducer<A, B> Right) =>
        new BiMapTransducer<X, Y, A, B>(Left, Right);
    
    public static Transducer<CoProduct<X, A>, CoProduct<X, B>> mapRight<X, A, B>(Func<A, B> f) =>
        new MapRightTransducer2<X, A, B>(f);
    
    public static Transducer<CoProduct<X, A>, CoProduct<X, B>> mapRight<X, A, B>(Transducer<A, B> f) =>
        new MapRightTransducer<X, A, B>(f);
    
    public static Transducer<CoProduct<X, A>, CoProduct<Y, A>> mapLeft<X, Y, A>(Func<X, Y> f) =>
        new MapLeftTransducer2<X, Y, A>(f);
    
    public static Transducer<CoProduct<X, A>, CoProduct<Y, A>> mapLeft<X, Y, A>(Transducer<X, Y> f) =>
        new MapLeftTransducer<X, Y, A>(f);
    
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
