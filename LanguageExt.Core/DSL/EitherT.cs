#nullable enable
#if !NET_STANDARD

using System;
using LanguageExt.Common;
using LanguageExt.DSL.Transducers;
using static LanguageExt.DSL.Transducers.Transducer;
using static LanguageExt.Prelude;

namespace LanguageExt.DSL;

// Transducer<Unit, CoProduct<Unit, A>>         -- Maybe
// Transducer<Unit, CoProduct<L, A>>            -- Either
// Transducer<Unit, CoProduct<Seq<L>, A>>       -- Validation
// Transducer<Unit, A>                          -- Seq      
// Transducer<Env,  CoProduct<Error, A>>        -- Reader
// Transducer<Env,  CoProduct<Error, A>>        -- Eff
// Transducer<Env,  Task<CoProduct<Error, A>>>  -- Aff

// Bind transformer     -- Transducer<E, MA> where MA : IsTransducer<E | Unit,   
// Pure transformer

public static class Test
{
    // Example of lifting an Eff into an EitherT, the Eff becomes the wrapper monad around
    // an inner Either monad, taking on the bind operations and capabilities of both
    public static EitherT<Error, Eff, RT, A> LiftEff<RT, A>(Eff<RT, A> ma) =>
        EitherT<Error, Eff, RT, A>.Lift(ma);
}

public readonly record struct EitherT<L, M, E, A>(K<M, E, K<Either<L>, A>> monad) :
    IsTransducer<E, K<Either<L>, A>>,
    K<M, E, A>
    where M : MonadReader<M>
{
    public static readonly EitherT<L, M, E, A> Bottom =
        Lift(CoProduct.Fail<L, A>(Errors.Bottom));
    
    static State<Unit> NilState => new (default, null);

    // -----------------------------------------------------------------------------------------------------------------
    // Transducer 

    public static EitherT<L, M, E, A> Right(A value) =>
        new(M.Pure<E, K<Either<L>, A>>(Either<L>.Pure(value)));
    
    public static EitherT<L, M, E, A> Left(L value) =>
        new(M.Pure<E, K<Either<L>, A>>(Either<L>.Left<A>(value)));
    
    public static EitherT<L, M, E, A> Lift(CoProduct<L, A> value) =>
        new(M.Pure<E, K<Either<L>, A>>(Either<L>.Lift(value)));
    
    public static EitherT<L, M, E, A> Lift(Either<L, A> value) =>
        new(M.Pure<E, K<Either<L>, A>>(Either<L>.Lift(value)));
    
    public static EitherT<L, M, E, A> Lift(K<M, E, A> ma) =>
        new(M.Map(ma, Either<L>.Pure));
    
    public Transducer<E, K<Either<L>, A>> ToTransducer() => 
        monad;

    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, A, TResult<S>> reduce) =>
        flatten(monad).Transform(reduce);
    
    // -----------------------------------------------------------------------------------------------------------------
    // Map

    public EitherT<L, M, E, B> Map<B>(Func<A, B> f) =>
        BiMap(static x => x, f);

    public EitherT<L, M, E, B> Map<B>(Transducer<A, B> f) =>
        BiMap(Transducer<L>.identity, f);

    // -----------------------------------------------------------------------------------------------------------------
    // BiMap

    public EitherT<X, M, E, B> BiMap<X, B>(Func<L, X> Left, Func<A, B> Right) =>
        new(M.Map(monad, ea => Either<L>.BiMap(ea, Left, Right)));

    public EitherT<X, M, E, B> BiMap<X, B>(Transducer<L, X> Left, Transducer<A, B> Right) =>
        new(M.Map(monad, ea => Either<L>.BiMap(ea, Left, Right)));

    // -----------------------------------------------------------------------------------------------------------------
    // Bind

    public EitherT<L, M, E, B> BindT<B>(Func<A, Either<L, B>> f) =>
        new(M.Bind(monad, ea => M.Pure<E, K<Either<L>, B>>(Either<L>.Bind(ea, x => f(x)))));

    public EitherT<L, M, E, B> Bind<B>(Func<A, EitherT<L, M, E, B>> f) =>
        new(M.Map(M.Bind(Match(Left: EitherT<L, M, E, B>.Left, f), x => x), Either<L>.Pure));

    //public EitherT<L, M, E, B> BindT<B>(Func<A, CoProduct<L, Monad<M, E, B>>> f) =>
    //    new(bindT(Morphism, f));

    //public EitherT<L, M, E, B> BindT<B>(Func<A, Transducer<Unit, CoProduct<L, Monad<M, E, B>>>> f) =>
    //    new(bindT(Morphism, f));
    
    //public EitherT<L, M, E, B> BindT<B>(Transducer<Unit, Monad<M, E, B>> f) =>
    //    new(bind(Morphism, f));

    //public EitherT<L, M, E, B> BindT<B>(Func<A, Transducer<Unit, Monad<M, E, B>>> f) =>
    //    new(bindProduceT(Morphism, f));
    
    // -----------------------------------------------------------------------------------------------------------------
    // BiBind

    /*public EitherT<M, B> BiBind<M, B>(Func<L, EitherT<M, B>> Left, Func<A, EitherT<M, B>> Right) =>
        new(Transducer.bikleisli<EitherT<M, B>, Unit, L, M, A, B>(Morphism, Left, Right));
    
    public EitherT<M, B> BiBind<M, B>(Func<L, CoProduct<M, B>> Left, Func<A, CoProduct<M, B>> Right) =>
        new(Transducer.bikleisli(Morphism, Left, Right));

    public EitherT<M, B> BiBind<M, B>(Transducer<L, CoProduct<M, B>> Left, Transducer<A, CoProduct<M, B>> Right) =>
        new(Transducer.bikleisli(Morphism, Left, Right));

    public EitherT<M, B> BiBind<M, B>(
        Transducer<L, Transducer<Unit, CoProduct<M, B>>> Left,
        Transducer<A, Transducer<Unit, CoProduct<M, B>>> Right) =>
            new(Transducer.bikleisli(Morphism, Left, Right));*/

    // -----------------------------------------------------------------------------------------------------------------
    // Select

    public EitherT<L, M, E, B> Select<B>(Func<A, B> f) =>
        Map(f);

    public EitherT<L, M, E, B> Select<B>(Transducer<A, B> f) =>
        Map(f);

    // -----------------------------------------------------------------------------------------------------------------
    // SelectMany
    
    public EitherT<L, M, E, B> SelectMany<B>(Func<A, Either<L, B>> f) =>
        BindT(f);

    public EitherT<L, M, E, B> SelectMany<B>(Func<A, EitherT<L, M, E, B>> f) =>
        Bind(f);

    //public EitherT<L, M, E, B> SelectMany<B>(Func<A, CoProduct<L, Monad<M, E, B>>> f) =>
    //    BindT(f);

    //public EitherT<L, M, E, B> SelectMany<B>(Func<A, Transducer<Unit, CoProduct<L, Monad<M, E, B>>>> f) =>
    //    BindT(f);

    //public EitherT<L, M, E, B> SelectMany<B>(Transducer<Unit, Monad<M, E, B>> f) =>
    //    BindT(f);

    //public EitherT<L, M, E, B> SelectMany<B>(Func<A, Transducer<Unit, Monad<M, E, B>>> f) =>
    //    BindT(f);

    // -----------------------------------------------------------------------------------------------------------------
    // SelectMany

    public EitherT<L, M, E, C> SelectMany<B, C>(Func<A, Either<L, B>> f, Func<A, B, C> project) =>
        BindT(a => f(a).Map(b => project(a, b)));

    public EitherT<L, M, E, C> SelectMany<B, C>(Func<A, EitherT<L, M, E, B>> f, Func<A, B, C> project) =>
        Bind(a => f(a).Map(b => project(a, b)));

    //public EitherT<L, M, E, C> SelectMany<B, C>(Func<A, Transducer<Unit, Monad<M, E, B>>> f, Func<A, B, C> project) =>
    //    new(bindMapT(Morphism, f, project));

    // -----------------------------------------------------------------------------------------------------------------
    // Filtering

    public EitherT<L, M, E, A> Filter(Func<A, bool> f) =>
        Map(filter(f));

    public EitherT<L, M, E, A> Where(Func<A, bool> f) =>
        Map(filter(f));

    // -----------------------------------------------------------------------------------------------------------------
    // Many item processing
    
    public EitherT<L, M, E, A> Head =>
        Map(Transducer<A>.head);

    public EitherT<L, M, E, A> Tail =>
        Map(Transducer<A>.tail);

    public EitherT<L, M, E, A> Skip(int amount) =>
        Map(skip<A>(amount));

    public EitherT<L, M, E, A> SkipWhile(Func<A, bool> predicate) =>
        Map(skipWhile(predicate));

    public EitherT<L, M, E, A> SkipUntil(Func<A, bool> predicate) =>
        Map(skipUntil(predicate));

    public EitherT<L, M, E, A> Take(int amount) =>
        Map(take<A>(amount));

    public EitherT<L, M, E, A> TakeWhile(Func<A, bool> predicate) =>
        Map(takeWhile(predicate));

    public EitherT<L, M, E, A> TakeUntil(Func<A, bool> predicate) =>
        Map(takeUntil(predicate));
    
    // -----------------------------------------------------------------------------------------------------------------
    // Matching

    public K<M, E, B> Match<B>(Func<L, B> Left, Func<A, B> Right) =>
        M.Bind(monad, ea => M.Lift(compose(constant<E, Unit>(unit), Either<L>.Match(ea, Left, Right))));
    
    // -----------------------------------------------------------------------------------------------------------------
    // Repeat

    /*public EitherT<L, M, E, A> Repeat(Schedule schedule) =>
        new(M.Lift(monad.RepeatT(schedule)));

    public EitherT<L, M, E, A> RepeatWhile(Schedule schedule, Func<A, bool> pred) =>
        new(M.Lift(monad.RepeatWhileT(schedule, pred)));

    public EitherT<L, M, E, A> RepeatUntil(Schedule schedule, Func<A, bool> pred) =>
        new(M.Lift(monad.RepeatUntilT(schedule, pred)));
    
    // -----------------------------------------------------------------------------------------------------------------
    // Retry

    public EitherT<L, M, E, A> Retry(Schedule schedule) =>
        new(M.Lift(monad.RetryT(schedule)));

    public EitherT<L, M, E, A> RetryWhile(Schedule schedule, Func<L, bool> pred) =>
        new(M.Lift(monad.RetryWhileT(schedule, pred)));

    public EitherT<L, M, E, A> RetryUntil(Schedule schedule, Func<L, bool> pred) =>
        new(M.Lift(monad.RetryUntilT(schedule, pred)));*/
    
    // -----------------------------------------------------------------------------------------------------------------
    // Folding
    //
    //public EitherT<L, M, E, S> Fold<S>(Schedule schedule, S state, Func<S, A, S> fold) =>
    //    new(M.Lift(monad.FoldT<S, M, E, A>(schedule, state,
    //        (s, p) => p is CoProductRight<L, A> r ? fold(s, r.Value) : s)));
//
    //public EitherT<L, M, E, S> FoldWhile<S>(Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred) =>
    //    new(M.Lift(monad.FoldWhileT(schedule, state,
    //        (s, p) => p is CoProductRight<L, A> r ? fold(s, r.Value) : s,
    //        p => p is CoProductRight<L, A> r && pred(r.Value))));

    //public EitherT<L, M, E, S> FoldUntil<S>(Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred) =>
    //    FoldWhile(schedule, state, fold, not(pred));

    // -----------------------------------------------------------------------------------------------------------------
    // Operators
    
    public static EitherT<L, M, E, A> operator |(EitherT<L, M, E, A> ma, EitherT<L, M, E, A> mb) =>
        new(M.Lift(choice(ma.monad, mb.monad)));

    public static implicit operator EitherT<L, M, E, A>(Either<L, A> value) =>
        Lift(value);
    
    public static implicit operator EitherT<L, M, E, A>(CoProduct<L, A> value) =>
        Lift(value);
    
    public static implicit operator EitherT<L, M, E, A>(L value) =>
        Left(value);

    public static implicit operator EitherT<L, M, E, A>(A value) =>
        Right(value);
}

#endif
