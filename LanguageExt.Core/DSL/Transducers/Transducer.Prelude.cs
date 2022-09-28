#nullable enable
using System;
using System.Collections.Generic;
using LanguageExt.Common;
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

public static partial class Transducer
{
    /// <summary>
    /// Apply an argument to a transducer
    /// </summary>
    /// <remarks>Collects zero or more results</remarks>
    /// <param name="x">Argument</param>
    /// <param name="mf">Transducer</param>
    /// <returns>`PrimPure<B> | PrimMany<B> | PrimFail<B>`</returns>
    public static Prim<B> apply<A, B>(A x, Transducer<A, B> mf)
    {
        var state = TState<Prim<B>>.Create(Prim<B>.None);
        try
        {
#nullable disable
            return mf.Transform<Prim<B>>(static (s, x) => TResult.Continue(s.Value + x))(state, x)
                     .Match(Complete: identity, 
                            Continue: identity, 
                            Fail: Prim.Fail<B>);
#nullable enable
        }
        catch (Exception e)
        {
            return Prim.Fail<B>(e);
        }
        finally
        {
            state.CleanUp();
        }
    }

    /// <summary>
    /// Apply an argument to a transducer
    /// </summary>
    /// <remarks>Collects zero or one results</remarks>
    /// <param name="x">Argument</param>
    /// <param name="mf">Transducer</param>
    /// <returns>`PrimPure<B> | PrimFail<B>`</returns>
    public static Prim<B> apply1<A, B>(A x, Transducer<A, B> mf)
    {
        var state = TState<Prim<B>>.Create(Prim<B>.None);
        try
        {
#nullable disable
            return mf.Transform<Prim<B>>(static (_, x) => TResult.Continue(Prim.Pure(x)))(state, x)
                     .Match(Complete: identity, 
                            Continue: identity, 
                            Fail: Prim.Fail<B>);
#nullable enable
        }
        catch (Exception e)
        {
            return Prim.Fail<B>(e);
        }
        finally
        {
            state.CleanUp();
        }
    }

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

    /// <summary>
    /// Ignores the result of the transducer
    /// </summary>
    public static Transducer<A, Unit> ignore<A, B>(Transducer<A, B> mf) =>
        compose(mf, constant<B, Unit>(default));

    public static Transducer<A, B> scope1<A, B>(Transducer<A, B> t) =>
        new ScopeTransducer<A, B>(t);

    public static Transducer<A, Seq<B>> scope<A, B>(Transducer<A, B> t) =>
        new ScopeManyTransducer<A, B>(t);

    public static Transducer<A, CoProduct<X, B>> scope1<X, A, B>(Transducer<A, CoProduct<X, B>> t) =>
        new ScopeManyTransducer<X, A, B>(t);

    public static Transducer<A, CoProduct<X, Seq<B>>> scope<X, A, B>(Transducer<A, CoProduct<X, B>> t) =>
        new ScopeManyTransducer2<X, A, B>(t);

    public static Transducer<A, C> compose<A, B, C>(Transducer<A, B> tab, Transducer<B, C> tbc) =>
        new ComposeTransducer<A, B, C>(tab, tbc);

    public static Transducer<A, D> compose<A, B, C, D>(Transducer<A, B> t1, Transducer<B, C> t2, Transducer<C, D> t3) =>
        new ComposeTransducer<A, B, C, D>(t1, t2, t3);

    public static Transducer<A, E> compose<A, B, C, D, E>(
        Transducer<A, B> t1, 
        Transducer<B, C> t2, 
        Transducer<C, D> t3, 
        Transducer<D, E> t4) =>
        new ComposeTransducer<A, B, C, D, E>(t1, t2, t3, t4);

    public static Transducer<A, F> compose<A, B, C, D, E, F>(
        Transducer<A, B> t1, 
        Transducer<B, C> t2, 
        Transducer<C, D> t3,
        Transducer<D, E> t4,
        Transducer<E, F> t5) =>
        new ComposeTransducer<A, B, C, D, E, F>(t1, t2, t3, t4, t5);

    public static Transducer<A, G> compose<A, B, C, D, E, F, G>(
        Transducer<A, B> t1, 
        Transducer<B, C> t2, 
        Transducer<C, D> t3,
        Transducer<D, E> t4,
        Transducer<E, F> t5,
        Transducer<F, G> t6) =>
        new ComposeTransducer<A, B, C, D, E, F, G>(t1, t2, t3, t4, t5, t6);

    public static Transducer<A, H> compose<A, B, C, D, E, F, G, H>(
        Transducer<A, B> t1, 
        Transducer<B, C> t2, 
        Transducer<C, D> t3,
        Transducer<D, E> t4,
        Transducer<E, F> t5,
        Transducer<F, G> t6,
        Transducer<G, H> t7) =>
        new ComposeTransducer<A, B, C, D, E, F, G, H>(t1, t2, t3, t4, t5, t6, t7);

    public static Transducer<A, I> compose<A, B, C, D, E, F, G, H, I>(
        Transducer<A, B> t1, 
        Transducer<B, C> t2, 
        Transducer<C, D> t3,
        Transducer<D, E> t4,
        Transducer<E, F> t5,
        Transducer<F, G> t6,
        Transducer<G, H> t7,
        Transducer<H, I> t8) =>
        new ComposeTransducer<A, B, C, D, E, F, G, H, I>(t1, t2, t3, t4, t5, t6, t7, t8);

    public static Transducer<A, J> compose<A, B, C, D, E, F, G, H, I, J>(
        Transducer<A, B> t1, 
        Transducer<B, C> t2, 
        Transducer<C, D> t3,
        Transducer<D, E> t4,
        Transducer<E, F> t5,
        Transducer<F, G> t6,
        Transducer<G, H> t7,
        Transducer<H, I> t8,
        Transducer<I, J> t9) =>
        new ComposeTransducer<A, B, C, D, E, F, G, H, I, J>(t1, t2, t3, t4, t5, t6, t7, t8, t9);

    public static Transducer<A, B> constant<A, B>(B @const) =>
        new ConstantTransducer<A, B>(@const);
    
    public static Transducer<X, CoProduct<A, B>> constantRight<X, A, B>(B @const) =>
        new ConstantRightTransducer<X, A, B>(@const);
    
    public static Transducer<X, CoProduct<A, B>> constantLeft<X, A, B>(A @const) =>
        new ConstantLeftTransducer<X, A, B>(@const);
    
    public static Transducer<X, CoProduct<A, B>> constantFail<X, A, B>(Error @const) =>
        new ConstantFailTransducer<X, A, B>(@const);
    
    public static Transducer<A, B> map<A, B>(Func<A, B> f) =>
        new MapTransducer<A, B>(f);
 
    public static Transducer<CoProduct<X, A>, CoProduct<Y, B>> bimap<X, Y, A, B>(Func<X, Y> Left, Func<A, B> Right) =>
        new BiMapTransducer2<X, Y, A, B>(Left, Right);
    
    public static Transducer<CoProduct<X, A>, CoProduct<Y, B>> bimap<X, Y, A, B>(Transducer<X, Y> Left, Transducer<A, B> Right) =>
        new BiMapTransducer<X, Y, A, B>(Left, Right);

    /// <summary>
    /// Transducer that extracts the right value from the co-product (if possible)
    /// </summary>
    public static Transducer<CoProduct<A, B>, B> rightValue<A, B>() =>
        TransducerStatic2<A, B>.rightValue;
    
    /// <summary>
    /// Transducer that extracts the left value from the co-product (if possible)
    /// </summary>
    public static Transducer<CoProduct<A, B>, A> leftValue<A, B>() =>
        TransducerStatic2<A, B>.leftValue;
    
    /// <summary>
    /// Transducer that makes a co-product from the right value
    /// </summary>
    public static Transducer<B, CoProduct<A, B>> right<A, B>() =>
        TransducerStatic2<A, B>.right;
    
    /// <summary>
    /// Transducer that makes a co-product from the left value
    /// </summary>
    public static Transducer<A, CoProduct<A, B>> left<A, B>() =>
        TransducerStatic2<A, B>.left;
    
    public static Transducer<CoProduct<X, A>, CoProduct<X, B>> mapRight<X, A, B>(Func<A, B> f) =>
        new MapRightTransducer2<X, A, B>(f);
    
    public static Transducer<CoProduct<X, A>, CoProduct<X, B>> mapRight<X, A, B>(Transducer<A, B> f) =>
        new MapRightTransducer<X, A, B>(f);
    
    public static Transducer<CoProduct<X, A>, CoProduct<Y, A>> mapLeft<X, Y, A>(Func<X, Y> f) =>
        new MapLeftTransducer2<X, Y, A>(f);
    
    public static Transducer<CoProduct<X, A>, CoProduct<Y, A>> mapLeft<X, Y, A>(Transducer<X, Y> f) =>
        new MapLeftTransducer<X, Y, A>(f);
    
    public static Transducer<CoProduct<X, A>, B> mapRightValue<X, A, B>(Func<A, B> f) =>
        new MapRightBackTransducer2<X, A, B>(f);
    
    public static Transducer<CoProduct<X, A>, B> mapRightValue<X, A, B>(Transducer<A, B> f) =>
        new MapRightBackTransducer<X, A, B>(f);
    
    public static Transducer<CoProduct<X, A>, Y> mapLeftValue<X, Y, A>(Func<X, Y> f) =>
        new MapLeftBackTransducer2<X, Y, A>(f);
    
    public static Transducer<CoProduct<X, A>, Y> mapLeftValue<X, Y, A>(Transducer<X, Y> f) =>
        new MapLeftBackTransducer<X, Y, A>(f);
    
    public static Transducer<A, B> flatten<A, B>(Transducer<A, Transducer<A, B>> f) =>
        new FlattenTransducer<A, B>(f);
    
    public static Transducer<A, B> flatten<A, B>(Transducer<A, Transducer<Unit, B>> f) =>
        new FlattenTransducer2<A, B>(f);
    
    public static Transducer<A, B> flatten<A, B>(Transducer<Unit, Transducer<A, B>> f) =>
        new FlattenTransducer3<A, B>(f);
    
    public static Transducer<Unit, B> flatten<B>(Transducer<Unit, Transducer<Unit, B>> f) =>
        new FlattenTransducer<Unit, B>(f);
    
    public static Transducer<A, A> filter<A>(Func<A, bool> f) =>
        new FilterTransducer<A>(f);
    
    public static Transducer<A, S> fold<S, A>(S state, Func<S, A, S> fold) =>
        new FoldTransducer<S, A>(state, fold);
    
    public static Transducer<A, S> foldUntil<S, A>(S state, Func<S, A, S> fold, Func<S, bool> predicate) =>
        new FoldUntilTransducer<S, A>(state, fold, predicate);
    
    public static Transducer<A, S> foldWhile<S, A>(S state, Func<S, A, S> fold, Func<S, bool> predicate) =>
        new FoldWhileTransducer<S, A>(state, fold, predicate);

    public static Transducer<A, S> foldUntil2<S, A, B>(
        Transducer<A, B> morphism, 
        S state, 
        Func<S, B, S> fold, 
        Func<S, bool> predicate, 
        Schedule schedule) =>
        new ScheduleFoldUntilTransducer2<S, A, B>(morphism, state, fold, predicate, schedule);
    
    public static Transducer<A, S> foldWhile2<S, A, B>(
        Transducer<A, B> morphism, 
        S state, 
        Func<S, B, S> fold, 
        Func<S, bool> predicate, 
        Schedule schedule) =>
        new ScheduleFoldWhileTransducer2<S, A, B>(morphism, state, fold, predicate, schedule);
    
    public static Transducer<A, S> foldUntil<S, A, B>(
        Transducer<A, B> morphism, 
        S state, 
        Func<S, B, S> fold, 
        Func<B, bool> predicate, 
        Schedule schedule) =>
        new ScheduleFoldUntilTransducer<S, A, B>(morphism, state, fold, predicate, schedule);
    
    public static Transducer<A, S> foldWhile<S, A, B>(
        Transducer<A, B> morphism, 
        S state, 
        Func<S, B, S> fold, 
        Func<B, bool> predicate, 
        Schedule schedule) =>
        new ScheduleFoldWhileTransducer<S, A, B>(morphism, state, fold, predicate, schedule);    

    public static Transducer<A, B> choice<A, B>(Transducer<A, B> first, Transducer<A, B> second) =>
        new ChoiceTransducer<A, B>(first, second);

    public static Transducer<A, CoProduct<X, B>> choice<X, A, B>(
        Transducer<A, CoProduct<X, B>> first, 
        Transducer<A, CoProduct<X, B>> second) =>
        new ChoiceTransducer<X, A, B>(first, second);
    
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
        compose(first, TransducerStatic2<X, A>.rightValue, second);

    public static Transducer<E, CoProduct<X, B>> kleisli<E, X, A, B>(
        Transducer<E, CoProduct<X, A>> first,
        Func<A, CoProduct<X, B>> second) =>
        compose(first, TransducerStatic2<X, A>.rightValue, map(second));

    public static Transducer<E, CoProduct<X, B>> kleisli<E, X, A, B>(
        Func<E, CoProduct<X, A>> first,
        Func<A, CoProduct<X, B>> second) =>
        compose(map(first), TransducerStatic2<X, A>.rightValue, map(second));

    public static Transducer<E, CoProduct<X, B>> kleisli<E, X, A, B>(
        Transducer<E, CoProduct<X, A>> first,
        Transducer<A, Transducer<E, CoProduct<X, B>>> second) =>
        flatten(compose(first, TransducerStatic2<X, A>.rightValue, second));

    public static Transducer<E, CoProduct<X, B>> kleisli<E, X, A, B>(
        Transducer<E, CoProduct<X, A>> first,
        Func<A, Transducer<E, CoProduct<X, B>>> second) =>
        flatten(compose(first, TransducerStatic2<X, A>.rightValue, map(second)));

    public static Transducer<E, B> match<E, X, A, B>(
        Transducer<E, CoProduct<X, A>> M,
        Transducer<X, B> Left,
        Transducer<A, B> Right) =>
        new MatchTransducer<E, X, A, B>(M, Left, Right);

    public static Transducer<E, B> match<E, X, A, B>(
        Transducer<E, CoProduct<X, A>> M,
        Func<X, B> Left,
        Func<A, B> Right) =>
        new MatchTransducer2<E, X, A, B>(M, Left, Right);
    
    public static Transducer<MA, Transducer<E, CoProduct<X, A>>> toTransducer<MA, E, X, A>()
        where MA : IsTransducer<E, CoProduct<X, A>> =>
        ToTransducer<MA, E, CoProduct<X, A>>.Default;
    
    public static Transducer<E, CoProduct<X, B>> bind<E, X, A, B, MB>(
        Transducer<E, CoProduct<X, A>> first,
        Func<A, MB> second) where MB : IsTransducer<E, CoProduct<X, B>> =>
        flatten(
            compose(
                first, 
                rightValue<X, A>(),
                map(second), 
                toTransducer<MB, E, X, B>()));

     public static Transducer<RT, CoProduct<E, B>> bind<RT, E, A, B>(
         Transducer<RT, CoProduct<E, A>> op, 
         Func<A, CoProduct<E, B>> f) =>
         compose(op, mapRightValue<E, A, CoProduct<E, B>>(f));

     public static Transducer<RT, CoProduct<E, B>> bind<RT, E, A, B>(
         Transducer<RT, CoProduct<E, A>> op,
         Func<A, Transducer<RT, CoProduct<E, B>>> f) =>
         flatten(compose(op, mapRightValue<E, A, Transducer<RT, CoProduct<E, B>>>(f)));
    
     public static Transducer<RT, CoProduct<E, B>> bind<RT, E, A, B>(
         Transducer<RT, CoProduct<E, A>> op,
         Transducer<A, Transducer<RT, CoProduct<E, B>>> f) =>
         flatten(compose(op, mapRightValue<E, A, Transducer<RT, CoProduct<E, B>>>(f)));

     public static Transducer<RT, CoProduct<E, B>> bind<RT, E, A, B>(
         Transducer<RT, CoProduct<E, A>> op,
         Transducer<Unit, B> f) =>
         compose(compose(ignore(op), f), TransducerStatic2<E, B>.right);

     public static Transducer<RT, CoProduct<E, B>> bindProduce<RT, E, A, B>(
         Transducer<RT, CoProduct<E, A>> op,
         Func<A, Transducer<Unit, B>> f) =>
         compose(flatten(compose(op, mapRightValue<E, A, Transducer<Unit, B>>(f))), TransducerStatic2<E, B>.right);
     
    public static Transducer<RT, CoProduct<E, C>> bindMap<RT, E, A, B, C>(
        Transducer<RT, CoProduct<E, A>> first,
        Func<A, Transducer<Unit, B>> second,
        Func<A, B, C> third) =>
        new BindMapTransducer<RT, E, A, B, C>(first, second, third);

    
    public static Transducer<A, B> schedule<A, B>(Transducer<A, B> morphism, Schedule schedule, Func<B, bool> pred) =>
        new ScheduleTransducer<A, B>(morphism, schedule, pred);
    
    /// <summary>
    /// Yields the values in the primitive
    /// </summary>
    public static Transducer<Unit, A> prim<A>(Prim<A> p) =>
        new PrimTransducer<A>(p);

    public static Transducer<(A, B), (X, Y)> pair<A, B, X, Y>(Transducer<A, X> first, Transducer<B, Y> second) =>
        new PairTransducer<A, B, X, Y>(first, second);

    public static Transducer<A, (A, A)> mkPair<A>() =>
        MakePairTransducer<A>.Default;
        
#if !NET_STANDARD

    public static Transducer<K<M, A>, A> bindT<M, A>() =>
        BindTransducer<M, A>.Default;

    public static Transducer<E, CoProduct<X, K<M, B>>> bindT<E, X, M, A, B>(
        Transducer<E, CoProduct<X, K<M, A>>> op,
        Func<A, CoProduct<X, K<M, B>>> f) =>
        compose(
            op,
            rightValue<X, K<M, A>>(),
            bindT<M, A>(),
            map(f));
    
    public static Transducer<E, CoProduct<X, K<M, B>>> bindT<E, X, M, A, B>(
        Transducer<E, CoProduct<X, K<M, A>>> op,
        Func<A, Transducer<E, CoProduct<X, K<M, B>>>> f) =>
        flatten(
            compose(
                op,
                rightValue<X, K<M, A>>(),
                bindT<M, A>(),
                map(f)));
    
    public static Transducer<E, K<F, S>> foldT<S, F, E, A>(
        Transducer<E, K<F, A>> morphism, 
        S state, 
        Func<S, A, S> fold, 
        Schedule schedule) where F : Applicative<F> =>
        new ScheduleFoldTTransducer<S, F, E, A>(morphism, state, fold, schedule);

    public static Transducer<E, K<F, S>> foldUntilT2<S, F, E, A>(
        Transducer<E, K<F, A>> morphism,
        S state,
        Func<S, A, S> fold,
        Func<S, bool> predicate,
        Schedule schedule) where F : Applicative<F> =>
        new ScheduleFoldUntilTTransducer2<S, F, E, A>(morphism, state, fold, predicate, schedule);
    
    public static Transducer<E, K<F, S>> foldWhileT2<S, F, E, A>(
        Transducer<E, K<F, A>> morphism,
        S state,
        Func<S, A, S> fold,
        Func<S, bool> predicate,
        Schedule schedule) where F : Applicative<F> =>
        foldUntilT2(morphism, state, fold, not(predicate), schedule);

    public static Transducer<E, K<F, S>> foldUntilT<S, F, E, A>(
        Transducer<E, K<F, A>> morphism,
        S state,
        Func<S, A, S> fold,
        Func<A, bool> predicate,
        Schedule schedule) where F : Applicative<F> =>
        new ScheduleFoldUntilTTransducer<S, F, E, A>(morphism, state, fold, predicate, schedule);    

    public static Transducer<E, K<F, S>> foldWhileT<S, F, E, A>(
        Transducer<E, K<F, A>> morphism,
        S state,
        Func<S, A, S> fold,
        Func<A, bool> predicate,
        Schedule schedule) where F : Applicative<F> =>
        foldUntilT(morphism, state, fold, not(predicate), schedule);    

    /*public static Transducer<E, K<F, S>> fold<S, F, E, A>(
        Transducer<E, A> morphism,
        S state,
        Func<S, A, S> fold,
        Schedule schedule) where F : Applicative<F>  =>
        new ScheduleFoldTransducer<S, E, A>(morphism, state, fold, schedule);*/  

    public static Transducer<E, K<F, E, S>> foldT<S, F, E, A>(
        Transducer<E, K<F, E, A>> morphism, 
        S state, 
        Func<S, A, S> fold, 
        Schedule schedule) where F : ApplicativeE<F> =>
        new ScheduleFoldTTransducer2<S, F, E, A>(morphism, state, fold, schedule);

    public static Transducer<E, K<F, E, S>> foldUntilT2<S, F, E, A>(
        Transducer<E, K<F, E, A>> morphism,
        S state,
        Func<S, A, S> fold,
        Func<S, bool> predicate,
        Schedule schedule) where F : ApplicativeE<F> =>
        new ScheduleFoldUntilTTransducer2E<S, F, E, A>(morphism, state, fold, predicate, schedule);
    
    public static Transducer<E, K<F, E, S>> foldWhileT2<S, F, E, A>(
        Transducer<E, K<F, E, A>> morphism,
        S state,
        Func<S, A, S> fold,
        Func<S, bool> predicate,
        Schedule schedule) where F : ApplicativeE<F> =>
        foldUntilT2(morphism, state, fold, not(predicate), schedule);

    public static Transducer<E, K<F, E, S>> foldUntilT<S, F, E, A>(
        Transducer<E, K<F, E, A>> morphism,
        S state,
        Func<S, A, S> fold,
        Func<A, bool> predicate,
        Schedule schedule) where F : ApplicativeE<F> =>
        new ScheduleFoldUntilTTransducer3<S, F, E, A>(morphism, state, fold, predicate, schedule);    

    public static Transducer<E, K<F, E, S>> foldWhileT<S, F, E, A>(
        Transducer<E, K<F, E, A>> morphism,
        S state,
        Func<S, A, S> fold,
        Func<A, bool> predicate,
        Schedule schedule) where F : ApplicativeE<F> =>
        foldUntilT(morphism, state, fold, not(predicate), schedule);    

    public static Transducer<E, K<F, E, S>> fold<S, F, E, A>(
        Transducer<E, K<F, E, A>> morphism,
        S state,
        Func<S, A, S> fold,
        Schedule schedule) where F : ApplicativeE<F> =>
        new ScheduleFoldTTransducer2<S, F, E, A>(morphism, state, fold, schedule);    
    
    public static Transducer<E, K<F, A>> scheduleT<F, E, A>(
        Transducer<E, K<F, A>> morphism, 
        Schedule schedule, 
        Func<A, bool> pred) where F : Applicative<F> =>
        new ScheduleTTransducer<F, E, A>(morphism, schedule, pred);
    
    public static Transducer<E, K<F, E, A>> scheduleT<F, E, A>(
        Transducer<E, K<F, E, A>> morphism, 
        Schedule schedule, 
        Func<A, bool> pred) where F : ApplicativeE<F> =>
        new ScheduleTTransducer2<F, E, A>(morphism, schedule, pred);

        
    /*
    public static Transducer<E, K<F, CoProduct<X, S>>> bifoldT<S, F, E, X, A>(
        Transducer<E, K<F, CoProduct<X, A>>> morphism, 
        S state, 
        Func<S, CoProduct<X, A>, S> fold, 
        Schedule schedule) where F : Bi<X>.Applicative<F> =>
        new BiScheduleFoldTTransducer<S, F, E, X, A>(morphism, state, fold, schedule);
    
    public static Transducer<E, K<F, E, CoProduct<X, S>>> bifoldT<S, F, E, X, A>(
        Transducer<E, K<F, E, CoProduct<X, A>>> morphism, 
        S state, 
        Func<S, CoProduct<X, A>, S> fold, 
        Schedule schedule) where F : Bi<X>.Applicative<F, E> =>
        new BiScheduleFoldTTransducer2<S, F, E, X, A>(morphism, state, fold, schedule);

    public static Transducer<E, K<F, CoProduct<X, S>>> bifoldUntilT2<S, F, E, X, A>(
        Transducer<E, K<F, CoProduct<X, A>>> morphism,
        S state,
        Func<S, CoProduct<X, A>, S> fold,
        Func<S, bool> predicate,
        Schedule schedule) where F : Bi<X>.Applicative<F> =>
        new BiScheduleFoldUntilTTransducer2<S, F, E, X, A>(morphism, state, fold, predicate, schedule);

    public static Transducer<E, K<F, E, CoProduct<X, S>>> bifoldUntilT2<S, F, E, X, A>(
        Transducer<E, K<F, E, CoProduct<X, A>>> morphism,
        S state,
        Func<S, CoProduct<X, A>, S> fold,
        Func<S, bool> predicate,
        Schedule schedule) where F : Bi<X>.Applicative<F, E> =>
        new BiScheduleFoldUntilTTransducer2E<S, F, E, X, A>(morphism, state, fold, predicate, schedule);
    
    public static Transducer<E, K<F, CoProduct<X, S>>> bifoldWhileT2<S, F, E, X, A>(
        Transducer<E, K<F, CoProduct<X, A>>> morphism,
        S state,
        Func<S, CoProduct<X, A>, S> fold,
        Func<S, bool> predicate,
        Schedule schedule) where F : Bi<X>.Applicative<F> =>
        bifoldUntilT2(morphism, state, fold, not(predicate), schedule);
    
    public static Transducer<E, K<F, E, CoProduct<X, S>>> bifoldWhileT2<S, F, E, X, A>(
        Transducer<E, K<F, E, CoProduct<X, A>>> morphism,
        S state,
        Func<S, CoProduct<X, A>, S> fold,
        Func<S, bool> predicate,
        Schedule schedule) where F : Bi<X>.Applicative<F, E> =>
        bifoldUntilT2(morphism, state, fold, not(predicate), schedule);

    public static Transducer<E, K<F, CoProduct<X, S>>> bifoldUntilT<S, F, E, X, A>(
        Transducer<E, K<F, CoProduct<X, A>>> morphism,
        S state,
        Func<S, CoProduct<X, A>, S> fold,
        Func<CoProduct<X, A>, bool> predicate,
        Schedule schedule) where F : Bi<X>.Applicative<F> =>
        new BiScheduleFoldUntilTTransducer<S, F, E, X, A>(morphism, state, fold, predicate, schedule);    

    public static Transducer<E, K<F, E, CoProduct<X, S>>> bifoldUntilT<S, F, E, X, A>(
        Transducer<E, K<F, E, CoProduct<X, A>>> morphism,
        S state,
        Func<S, CoProduct<X, A>, S> fold,
        Func<CoProduct<X, A>, bool> predicate,
        Schedule schedule) where F : Bi<X>.Applicative<F, E> =>
        new BiScheduleFoldUntilTTransducer3<S, F, E, X, A>(morphism, state, fold, predicate, schedule);    

    public static Transducer<E, K<F, E, CoProduct<X, S>>> bifoldWhileT<S, F, E, X, A>(
        Transducer<E, K<F, E, CoProduct<X, A>>> morphism,
        S state,
        Func<S, CoProduct<X, A>, S> fold,
        Func<CoProduct<X, A>, bool> predicate,
        Schedule schedule) where F : Bi<X>.Applicative<F, E> =>
        bifoldUntilT(morphism, state, fold, not(predicate), schedule);    

    public static Transducer<E, K<F, CoProduct<X, S>>> bifoldWhileT<S, F, E, X, A>(
        Transducer<E, K<F, CoProduct<X, A>>> morphism,
        S state,
        Func<S, CoProduct<X, A>, S> fold,
        Func<CoProduct<X, A>, bool> predicate,
        Schedule schedule) where F : Bi<X>.Applicative<F> =>
        bifoldUntilT(morphism, state, fold, not(predicate), schedule);    

    public static Transducer<E, K<F, E, CoProduct<X, S>>> bifold<S, F, E, X, A>(
        Transducer<E, K<F, E, CoProduct<X, A>>> morphism,
        S state,
        Func<S, CoProduct<X, A>, S> fold,
        Schedule schedule) where F : Bi<X>.Applicative<F, E> =>
        new BiScheduleFoldTTransducer2<S, F, E, X, A>(morphism, state, fold, schedule); 
    
    public static Transducer<E, K<F, CoProduct<X, A>>> bischeduleT<F, E, X, A>(
        Transducer<E, K<F, CoProduct<X, A>>> morphism, 
        Schedule schedule, 
        Func<CoProduct<X, A>, bool> pred) where F : Bi<X>.Applicative<F> =>
        new BiScheduleTTransducer<F, E, X, A>(morphism, schedule, pred);
    
    public static Transducer<E, K<F, E, CoProduct<X, A>>> bischeduleT<F, E, X, A>(
        Transducer<E, K<F, E, CoProduct<X, A>>> morphism, 
        Schedule schedule, 
        Func<CoProduct<X, A>, bool> pred) where F : Bi<X>.Applicative<F, E> =>
        new BiScheduleTTransducer2<F, E, X, A>(morphism, schedule, pred);
        */
    
    public static Transducer<E, CoProduct<X, K<M, B>>> bindProduceT<E, X, M, A, B>(
        Transducer<E, CoProduct<X, K<M, A>>> op,
        Func<A, Transducer<Unit, K<M, B>>> f) =>
        compose(
            flatten(
                compose(
                    op, 
                    rightValue<X, K<M, A>>(),
                    bindT<M, A>(),
                    map(f))), 
            right<X, K<M, B>>());    
#endif

    
}
