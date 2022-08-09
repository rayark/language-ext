#nullable enable

using System;
using LanguageExt.Common;

namespace LanguageExt.DSL;

public readonly record struct Either<L, A>(Morphism<Unit, CoProduct<L, A>> Op) : IsMorphism<Unit, CoProduct<L, A>>
{
    public static readonly Either<L, A> Bottom = new(Morphism.constant<Unit, CoProduct<L, A>>(Prim<CoProduct<L, A>>.None));
    
    internal Morphism<Unit, CoProduct<L, A>> OpSafe => Op ?? Bottom.Op;

    static State<Unit> NilState => new (default, null);

    public Morphism<Unit, CoProduct<L, A>> ToMorphism() => 
        OpSafe;

    // -----------------------------------------------------------------------------------------------------------------
    // Map

    public Either<L, B> Map<B>(Func<A, B> f) =>
        BiMap(static x => x, f);

    public Either<L, B> Map<B>(Morphism<A, B> f) =>
        BiMap(Morphism<L>.identity, f);

    // -----------------------------------------------------------------------------------------------------------------
    // BiMap

    public Either<M, B> BiMap<M, B>(Func<L, M> Left, Func<A, B> Right) =>
        Morphism.compose(OpSafe, BiMorphism.bimap(Left, Right));

    public Either<M, B> BiMap<M, B>(Morphism<L, M> Left, Morphism<A, B> Right) =>
        Morphism.compose(OpSafe, BiMorphism.bimap(Left, Right));

    // -----------------------------------------------------------------------------------------------------------------
    // Bind

    public Either<L, B> Bind<B>(Func<A, Either<L, B>> f) =>
        new(Morphism.kleisli<Either<L, B>, Unit, L, A, B>(OpSafe, f));

    public Either<L, B> Bind<B>(Func<A, CoProduct<L, B>> f) =>
        new(Morphism.kleisli(OpSafe, f)); 

    public Either<L, B> Bind<B>(Func<A, Morphism<Unit, CoProduct<L, B>>> f) =>
        new(Morphism.kleisli(OpSafe, f));
    
    public Either<L, B> Bind<B>(Morphism<A, Morphism<Unit, CoProduct<L, B>>> f) =>
        new(Morphism.kleisli(OpSafe, f));
    
    // -----------------------------------------------------------------------------------------------------------------
    // BiBind

    public Either<M, B> BiBind<M, B>(Func<L, Either<M, B>> Left, Func<A, Either<M, B>> Right) =>
        new(Morphism.bikleisli<Either<M, B>, Unit, L, M, A, B>(OpSafe, Left, Right));
    
    public Either<M, B> BiBind<M, B>(Func<L, CoProduct<M, B>> Left, Func<A, CoProduct<M, B>> Right) =>
        new(Morphism.bikleisli(OpSafe, Left, Right));

    public Either<M, B> BiBind<M, B>(Morphism<L, CoProduct<M, B>> Left, Morphism<A, CoProduct<M, B>> Right) =>
        new(Morphism.bikleisli(OpSafe, Left, Right));

    public Either<M, B> BiBind<M, B>(
        Morphism<L, Morphism<Unit, CoProduct<M, B>>> Left,
        Morphism<A, Morphism<Unit, CoProduct<M, B>>> Right) =>
            new(Morphism.bikleisli(OpSafe, Left, Right));

    // -----------------------------------------------------------------------------------------------------------------
    // Select

    public Either<L, B> Select<B>(Func<A, B> f) =>
        Map(f);

    public Either<L, B> Select<B>(Morphism<A, B> f) =>
        Map(f);

    // -----------------------------------------------------------------------------------------------------------------
    // SelectMany
    
    public Either<L, B> SelectMany<B>(Func<A, Either<L, B>> f) =>
        new(Morphism.kleisli<Either<L, B>, Unit, L, A, B>(OpSafe, f));

    public Either<L, B> SelectMany<B>(Func<A, CoProduct<L, B>> f) =>
        new(Morphism.kleisli(OpSafe, f)); 

    public Either<L, B> SelectMany<B>(Func<A, Morphism<Unit, CoProduct<L, B>>> f) =>
        new(Morphism.kleisli(OpSafe, f));
    
    public Either<L, B> SelectMany<B>(Morphism<A, Morphism<Unit, CoProduct<L, B>>> f) =>
        new(Morphism.kleisli(OpSafe, f));

    // -----------------------------------------------------------------------------------------------------------------
    // SelectMany

    public Either<L, C> SelectMany<B, C>(Func<A, Either<L, B>> f, Func<A, B, C> project) =>
        new(Morphism.kleisliProject(OpSafe, f, project));

    public Either<L, C> SelectMany<B, C>(Func<A, CoProduct<L, B>> f, Func<A, B, C> project) =>
        new(Morphism.kleisliProject(OpSafe, f, project)); 

    public Either<L, C> SelectMany<B, C>(Func<A, Morphism<Unit, CoProduct<L, B>>> f, Morphism<A, Morphism<B, C>> project) =>
        new(Morphism.kleisliProject(OpSafe, f, project));
    
    public Either<L, C> SelectMany<B, C>(Morphism<A, Morphism<Unit, CoProduct<L, B>>> f, Morphism<A, Morphism<B, C>> project) =>
        new(Morphism.kleisliProject(OpSafe, f, project));
    

    // -----------------------------------------------------------------------------------------------------------------
    // Filtering

    public Either<L, A> Filter(Func<A, bool> f) =>
        Op.Filter(x => x is CoProductRight<L, A> r && f(r.Value));

    public Either<L, A> Where(Func<A, bool> f) =>
        Filter(f);

    // -----------------------------------------------------------------------------------------------------------------
    // Many item processing
    
    public Either<L, A> Head =>
        Op.Head;

    public Either<L, A> Tail =>
        Op.Tail;

    public Either<L, A> Last =>
        Op.Last;

    public Either<L, A> Skip(int amount) =>
        Op.Skip(amount);

    public Either<L, A> Take(int amount) =>
        Op.Take(amount);
    
    // -----------------------------------------------------------------------------------------------------------------
    // Matching

    public B Match<B>(Func<L, B> Left, Func<A, B> Right)
    {
        return Go(Op.Invoke(NilState, Prim.Unit));

        B Go(Prim<CoProduct<L, A>> ma) =>
            ma switch
            {
                PurePrim<CoProduct<L, A>> {Value: CoProductRight<L, A> r} => Right(r.Value),
                PurePrim<CoProduct<L, A>> {Value: CoProductLeft<L, A> l} => Left(l.Value),
                PurePrim<CoProduct<L, A>> {Value: CoProductFail<L, A> f} => f.Value.Throw<B>(),
                ManyPrim<CoProduct<L, A>> m => Go(m.Head),
                FailPrim<CoProduct<L, A>> f => f.Value.Throw<B>(),
                _ => throw new NotSupportedException()
            };
    }
    
    public Seq<B> MatchMany<B>(Func<L, B> Left, Func<A, B> Right)
    {
        return Go(OpSafe.Invoke(NilState, Prim.Unit));

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
        return Go(OpSafe.Invoke(NilState, Prim.Unit));

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
        Op.Repeat(schedule);

    public Either<L, A> RepeatWhile(Schedule schedule, Func<A, bool> pred) =>
        Op.RepeatWhile(schedule, pred);

    public Either<L, A> RepeatUntil(Schedule schedule, Func<A, bool> pred) =>
        Op.RepeatUntil(schedule, pred);
    
    // -----------------------------------------------------------------------------------------------------------------
    // Retry

    public Either<L, A> Retry(Schedule schedule) =>
        Op.Retry(schedule);

    public Either<L, A> RetryWhile(Schedule schedule, Func<L, bool> pred) =>
        Op.RetryWhile(schedule, pred);

    public Either<L, A> RetryUntil(Schedule schedule, Func<L, bool> pred) =>
        Op.RetryUntil(schedule, pred);
    
    // -----------------------------------------------------------------------------------------------------------------
    // Folding

    public Either<L, S> Fold<S>(Schedule schedule, S state, Func<S, A, S> fold)=>
        Op.Fold(schedule, state, fold);

    public Either<L, S> FoldWhile<S>(Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)=>
        Op.FoldWhile(schedule, state, fold, pred);

    public Either<L, S> FoldUntil<S>(Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)=>
        Op.FoldWhile(schedule, state, fold, pred);

    // -----------------------------------------------------------------------------------------------------------------
    // Operators
    
    public static Either<L, A> operator |(Either<L, A> ma, Either<L, A> mb) =>
        Obj.Choice(ma.Op.Apply(Prim.Unit), mb.Op.Apply(Prim.Unit));

    public static Either<L, A> operator !(Either<L, A> mx) =>
        mx.Op.Invoke(NilState, Prim.Unit);
    
    public static implicit operator Morphism<Unit, CoProduct<L, A>>(Either<L, A> ma) =>
        ma.OpSafe;

    public static implicit operator Either<L, A>(Obj<CoProduct<L, A>> obj) =>
        obj.ToEither();

    public static implicit operator Either<L, A>(CoProduct<L, A> obj) =>
        obj.ToEither();

    public static implicit operator Either<L, A>(Morphism<Unit, CoProduct<L, A>> obj) =>
        obj.ToEither();

    public static implicit operator Either<L, A>(L value) =>
        Prelude.Left<L, A>(value);

    public static implicit operator Either<L, A>(A value) =>
        Prelude.Right<L, A>(value);
}
