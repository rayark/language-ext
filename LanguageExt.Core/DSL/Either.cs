#nullable enable

using System;
using LanguageExt.Common;
using LanguageExt.DSL.Transducers;
using static LanguageExt.DSL.Transducers.Transducer;

namespace LanguageExt.DSL;

public readonly record struct Either<L, A>(Transducer<Unit, CoProduct<L, A>> MorphismUnsafe) : IsTransducer<Unit, CoProduct<L, A>>
{
    public static readonly Either<L, A> Bottom = 
        new(Transducer.constant<Unit, CoProduct<L, A>>(CoProduct.Fail<L, A>(Errors.Bottom)));
    
    public Transducer<Unit, CoProduct<L, A>> Morphism => MorphismUnsafe ?? Bottom.MorphismUnsafe;

    static State<Unit> NilState => new (default, null);

    public Transducer<Unit, CoProduct<L, A>> ToTransducer() => 
        Morphism;

    // -----------------------------------------------------------------------------------------------------------------
    // Map

    public Either<L, B> Map<B>(Func<A, B> f) =>
        new(compose(Morphism, mapRight<L, A, B>(f)));

    public Either<L, B> Map<B>(Transducer<A, B> f) =>
        new(compose(Morphism, mapRight<L, A, B>(f)));

    // -----------------------------------------------------------------------------------------------------------------
    // BiMap

    public Either<M, B> BiMap<M, B>(Func<L, M> Left, Func<A, B> Right) =>
        new(compose(Morphism, bimap(Left, Right)));

    public Either<M, B> BiMap<M, B>(Transducer<L, M> Left, Transducer<A, B> Right) =>
        new(compose(Morphism, bimap(Left, Right)));

    // -----------------------------------------------------------------------------------------------------------------
    // Bind

    public Either<L, B> Bind<B>(Func<A, Either<L, B>> f) =>
        new(bind<Unit, L, A, B, Either<L, B>>(Morphism, f));

    public Either<L, B> Bind<B>(Func<A, CoProduct<L, B>> f) =>
        new(bind(Morphism, f));

    public Either<L, B> Bind<B>(Func<A, Transducer<Unit, CoProduct<L, B>>> f) =>
        new(bind(Morphism, f));
    
    public Either<L, B> Bind<B>(Transducer<Unit, B> f) =>
        new(bind(Morphism, f));

    public Either<L, B> Bind<B>(Func<A, Transducer<Unit, B>> f) =>
        new(bindProduce(Morphism, f));
    
    // -----------------------------------------------------------------------------------------------------------------
    // BiBind

    /*public Either<M, B> BiBind<M, B>(Func<L, Either<M, B>> Left, Func<A, Either<M, B>> Right) =>
        new(Transducer.bikleisli<Either<M, B>, Unit, L, M, A, B>(Morphism, Left, Right));
    
    public Either<M, B> BiBind<M, B>(Func<L, CoProduct<M, B>> Left, Func<A, CoProduct<M, B>> Right) =>
        new(Transducer.bikleisli(Morphism, Left, Right));

    public Either<M, B> BiBind<M, B>(Transducer<L, CoProduct<M, B>> Left, Transducer<A, CoProduct<M, B>> Right) =>
        new(Transducer.bikleisli(Morphism, Left, Right));

    public Either<M, B> BiBind<M, B>(
        Transducer<L, Transducer<Unit, CoProduct<M, B>>> Left,
        Transducer<A, Transducer<Unit, CoProduct<M, B>>> Right) =>
            new(Transducer.bikleisli(Morphism, Left, Right));*/

    // -----------------------------------------------------------------------------------------------------------------
    // Select

    public Either<L, B> Select<B>(Func<A, B> f) =>
        Map(f);

    public Either<L, B> Select<B>(Transducer<A, B> f) =>
        Map(f);

    // -----------------------------------------------------------------------------------------------------------------
    // SelectMany
    
    public Either<L, B> SelectMany<B>(Func<A, Either<L, B>> f) =>
        Bind(f);

    public Either<L, B> SelectMany<B>(Func<A, CoProduct<L, B>> f) =>
        Bind(f);

    public Either<L, B> SelectMany<B>(Func<A, Transducer<Unit, CoProduct<L, B>>> f) =>
        Bind(f);

    public Either<L, B> SelectMany<B>(Transducer<Unit, B> f) =>
        Bind(f);

    public Either<L, B> SelectMany<B>(Func<A, Transducer<Unit, B>> f) =>
        Bind(f);

    // -----------------------------------------------------------------------------------------------------------------
    // SelectMany

    public Either<L, C> SelectMany<B, C>(Func<A, Either<L, B>> f, Func<A, B, C> project) =>
        Bind(a => f(a).Map(b => project(a, b)));

    public Either<L, C> SelectMany<B, C>(Func<A, Transducer<Unit, B>> f, Func<A, B, C> project) =>
        new(bindMap(Morphism, f, project));

    // -----------------------------------------------------------------------------------------------------------------
    // Filtering

    public Either<L, A> Filter(Func<A, bool> f) =>
        Map(filter(f));

    public Either<L, A> Where(Func<A, bool> f) =>
        Map(filter(f));

    // -----------------------------------------------------------------------------------------------------------------
    // Many item processing
    
    public Either<L, A> Head =>
        Map(Transducer<A>.head);

    public Either<L, A> Tail =>
        Map(Transducer<A>.tail);

    public Either<L, A> Skip(int amount) =>
        Map(skip<A>(amount));

    public Either<L, A> SkipWhile(Func<A, bool> predicate) =>
        Map(skipWhile(predicate));

    public Either<L, A> SkipUntil(Func<A, bool> predicate) =>
        Map(skipUntil(predicate));

    public Either<L, A> Take(int amount) =>
        Map(take<A>(amount));

    public Either<L, A> TakeWhile(Func<A, bool> predicate) =>
        Map(takeWhile(predicate));

    public Either<L, A> TakeUntil(Func<A, bool> predicate) =>
        Map(takeUntil(predicate));
    
    // -----------------------------------------------------------------------------------------------------------------
    // Matching

    public B Match<B>(Func<L, B> Left, Func<A, B> Right)
    {
        return Go(Morphism.Apply(default));

        B Go(Prim<CoProduct<L, A>> ma) =>
            ma switch
            {
                PurePrim<CoProduct<L, A>> {Value: CoProductRight<L, A> r} => Right(r.Value),
                PurePrim<CoProduct<L, A>> {Value: CoProductLeft<L, A> l} => Left(l.Value),
                PurePrim<CoProduct<L, A>> {Value: CoProductFail<L, A> f} => f.Value.Throw<B>(),
                ManyPrim<CoProduct<L, A>> m => Go(m.Items.Head),
                FailPrim<CoProduct<L, A>> f => f.Value.Throw<B>(),
                _ => throw new NotSupportedException()
            };
    }
    
    public Seq<B> MatchMany<B>(Func<L, B> Left, Func<A, B> Right)
    {
        return Go(Morphism.Apply(default));

        Seq<B> Go(Prim<CoProduct<L, A>> ma) =>
            ma switch
            {
                PurePrim<CoProduct<L, A>> {Value: CoProductRight<L, A> r} => LanguageExt.Prelude.Seq1(Right(r.Value)),
                PurePrim<CoProduct<L, A>> {Value: CoProductLeft<L, A> l} => LanguageExt.Prelude.Seq1(Left(l.Value)),
                PurePrim<CoProduct<L, A>> {Value: CoProductFail<L, A> f} => f.Value.Throw<Seq<B>>(),
                ManyPrim<CoProduct<L, A>> m => m.Items.Map(Go).Flatten(),
                FailPrim<CoProduct<L, A>> f => f.Value.Throw<Seq<B>>(),
                _ => throw new NotSupportedException()
            };
    }
    
    public Seq<B> MatchMany<B>(Func<L, Seq<B>> Left, Func<A, Seq<B>> Right)
    {
        return Go(Morphism.Apply(default));

        Seq<B> Go(Prim<CoProduct<L, A>> ma) =>
            ma switch
            {
                PurePrim<CoProduct<L, A>> {Value: CoProductRight<L, A> r} => Right(r.Value),
                PurePrim<CoProduct<L, A>> {Value: CoProductLeft<L, A> l} => Left(l.Value),
                PurePrim<CoProduct<L, A>> {Value: CoProductFail<L, A> f} => f.Value.Throw<Seq<B>>(),
                ManyPrim<CoProduct<L, A>> m => m.Items.Map(Go).Flatten(),
                FailPrim<CoProduct<L, A>> f => f.Value.Throw<Seq<B>>(),
                _ => throw new NotSupportedException()
            };
    }
    
    
    // -----------------------------------------------------------------------------------------------------------------
    // Repeat

    public Either<L, A> Repeat(Schedule schedule) =>
        new(Morphism.Repeat(schedule));

    public Either<L, A> RepeatWhile(Schedule schedule, Func<A, bool> pred) =>
        new(Morphism.RepeatWhile(schedule, pred));

    public Either<L, A> RepeatUntil(Schedule schedule, Func<A, bool> pred) =>
        new(Morphism.RepeatUntil(schedule, pred));
    
    // -----------------------------------------------------------------------------------------------------------------
    // Retry

    public Either<L, A> Retry(Schedule schedule) =>
        new(Morphism.Retry(schedule));

    public Either<L, A> RetryWhile(Schedule schedule, Func<L, bool> pred) =>
        new(Morphism.RetryWhile(schedule, pred));

    public Either<L, A> RetryUntil(Schedule schedule, Func<L, bool> pred) =>
        new(Morphism.RetryUntil(schedule, pred));
    
    // -----------------------------------------------------------------------------------------------------------------
    // Folding

    public Either<L, S> Fold<S>(Schedule schedule, S state, Func<S, A, S> fold)=>
        new(Morphism.Fold(schedule, state, fold));

    public Either<L, S> FoldWhile<S>(Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)=>
        new(Morphism.FoldWhile(schedule, state, fold, pred));

    public Either<L, S> FoldUntil<S>(Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)=>
        new(Morphism.FoldUntil(schedule, state, fold, pred));

    // -----------------------------------------------------------------------------------------------------------------
    // Operators
    
    public static Either<L, A> operator |(Either<L, A> ma, Either<L, A> mb) =>
        new(choice(ma.Morphism, mb.Morphism));

    public static Either<L, A> operator !(Either<L, A> mx)
    {
        var rx = mx.Morphism.Apply(default);
        return new(prim(rx));
    }
    
    public static implicit operator Either<L, A>(CoProduct<L, A> obj) =>
        obj.ToEither();

    public static implicit operator Either<L, A>(L value) =>
        new(constantLeft<Unit, L, A>(value));

    public static implicit operator Either<L, A>(A value) =>
        new(constantRight<Unit, L, A>(value));
}
