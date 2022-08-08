#nullable enable

using System;

namespace LanguageExt.DSL;

public readonly struct ObjEither<L, A> : IsObj<Either<L, A>, CoProduct<L, A>>
{
    public Obj<CoProduct<L, A>> ToObject(Either<L, A> value) => 
        value.ObjectSafe;
}

public readonly record struct Either<L, A>(Obj<CoProduct<L, A>> Object)
{
    public static readonly Either<L, A> Bottom = new(Prim<CoProduct<L, A>>.None);
    
    internal Obj<CoProduct<L, A>> ObjectSafe => Object ?? Bottom;

    static readonly State<Unit> NilState = new (default, null);

    // -----------------------------------------------------------------------------------------------------------------
    // Map

    public Either<L, B> Map<B>(Func<A, B> f) =>
        BiMap(Morphism<L>.identity, Morphism.function(f));

    public Either<L, B> Map<B>(Morphism<A, B> f) =>
        BiMap(Morphism<L>.identity, f);

    // -----------------------------------------------------------------------------------------------------------------
    // BiMap

    public Either<M, B> BiMap<M, B>(Func<L, M> Left, Func<A, B> Right) =>
        BiMap(Morphism.function(Left), Morphism.function(Right));

    public Either<M, B> BiMap<M, B>(Morphism<L, M> Left, Morphism<A, B> Right) =>
        new(BiMorphism.bimap(Left, Right).Apply(ObjectSafe));

    // -----------------------------------------------------------------------------------------------------------------
    // Bind

    public Either<L, B> Bind<B>(Func<A, Either<L, B>> f) =>
        new(BiMorphism.rightBind<ObjEither<L, B>, Either<L,B>, L, A, B>(f).Apply(ObjectSafe));
    
    public Either<L, B> Bind<B>(Func<A, CoProduct<L, B>> f) =>
        Bind(Morphism.function(f));

    public Either<L, B> Bind<B>(Func<A, Obj<CoProduct<L, B>>> f) =>
        Bind(Morphism.function(f));
    
    public Either<L, B> Bind<B>(Morphism<A, CoProduct<L, B>> f) =>
        new(BiMorphism.rightBind(f).Apply(ObjectSafe));

    public Either<L, B> Bind<B>(Morphism<A, Obj<CoProduct<L, B>>> f) =>
        new(BiMorphism.rightBind(f).Apply(ObjectSafe));
    
    // -----------------------------------------------------------------------------------------------------------------
    // BiBind
    
    public Either<M, B> BiBind<M, B>(Func<L, Either<M, B>> Left, Func<A, Either<M, B>> Right) =>
        new(BiMorphism.bibind<ObjEither<M, B>, Either<M, B>, L, M, A, B>(
            Morphism.function(Left),
            Morphism.function(Right)).Apply(ObjectSafe));
    
    public Either<M, B> BiBind<M, B>(Func<L, CoProduct<M, B>> Left, Func<A, CoProduct<M, B>> Right) =>
        BiBind(Morphism.function(Left), Morphism.function(Right));

    public Either<M, B> BiBind<M, B>(Func<L, Obj<CoProduct<M, B>>> Left, Func<A, Obj<CoProduct<M, B>>> Right) =>
        BiBind(Morphism.bind(Left), Morphism.bind(Right));

    public Either<M, B> BiBind<M, B>(Morphism<L, CoProduct<M, B>> Left, Morphism<A, CoProduct<M, B>> Right) =>
        new(BiMorphism.bibind(Left, Right).Apply(ObjectSafe));

    public Either<M, B> BiBind<M, B>(Morphism<L, Obj<CoProduct<M, B>>> Left, Morphism<A, Obj<CoProduct<M, B>>> Right) =>
        new(BiMorphism.bibind(Left, Right).Apply(ObjectSafe));

    // -----------------------------------------------------------------------------------------------------------------
    // Select

    public Either<L, B> Select<B>(Func<A, B> f) =>
        Map(f);

    public Either<L, B> Select<B>(Morphism<A, B> f) =>
        Map(f);

    // -----------------------------------------------------------------------------------------------------------------
    // SelectMany
    
    public Either<L, B> SelectMany<B>(Func<A, Either<L, B>> f) =>
        Bind(f);
    
    public Either<L, B> SelectMany<B>(Func<A, CoProduct<L, B>> f) =>
        Bind(f);

    public Either<L, B> SelectMany<B>(Func<A, Obj<CoProduct<L, B>>> f) =>
        Bind(f);
    
    public Either<L, B> SelectMany<B>(Morphism<A, CoProduct<L, B>> f) =>
        Bind(f);

    public Either<L, B> SelectMany<B>(Morphism<A, Obj<CoProduct<L, B>>> f) =>
        Bind(f);

    // -----------------------------------------------------------------------------------------------------------------
    // SelectMany

    public Either<L, C> SelectMany<B, C>(Func<A, Either<L, B>> bind, Func<A, B, C> project) =>
        new(BiMorphism.rightBind<ObjEither<L, B>, Either<L, B>, L, A, B, C>(
            Morphism.function(bind), 
            Morphism.function(project)).Apply(ObjectSafe));
    
    public Either<L, C> SelectMany<B, C>(Func<A, CoProduct<L, B>> bind, Func<A, B, C> project) =>
        SelectMany(Morphism.function(bind), Morphism.function(project));

    public Either<L, C> SelectMany<B, C>(Func<A, Obj<CoProduct<L, B>>> bind, Func<A, B, C> project) =>
        SelectMany(Morphism.function(bind), Morphism.function(project));
    
    public Either<L, C> SelectMany<B, C>(Morphism<A, CoProduct<L, B>> bind, Morphism<A, Morphism<B, C>> project) =>
        new(BiMorphism.rightBind(bind, project).Apply(ObjectSafe));

    public Either<L, C> SelectMany<B, C>(Morphism<A, Obj<CoProduct<L, B>>> bind, Morphism<A, Morphism<B, C>> project) =>
        new(BiMorphism.rightBind(bind, project).Apply(ObjectSafe));

    // -----------------------------------------------------------------------------------------------------------------
    // Filtering

    public Either<L, A> Filter(Func<A, bool> f) =>
        Bind(x => f(x) ? Prelude.Right<L, A>(x) : Bottom);

    public Either<L, A> Where(Func<A, bool> f) =>
        Filter(f);

    // -----------------------------------------------------------------------------------------------------------------
    // Many item processing
    
    public Either<L, A> Head =>
        ObjectSafe.Head;

    public Either<L, A> Tail =>
        ObjectSafe.Tail;

    public Either<L, A> Last =>
        ObjectSafe.Last;

    public Either<L, A> Skip(int amount) =>
        ObjectSafe.Skip(amount);

    public Either<L, A> Take(int amount) =>
        ObjectSafe.Take(amount);
    
    // -----------------------------------------------------------------------------------------------------------------
    // Matching

    public B Match<B>(Func<L, B> Left, Func<A, B> Right)
    {
        return Go(ObjectSafe.Interpret(NilState));

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
        return Go(ObjectSafe.Interpret(NilState));

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
        return Go(ObjectSafe.Interpret(NilState));

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
        Morphism.constant<CoProduct<L, Unit>, CoProduct<L, A>>(ObjectSafe).Repeat(schedule)
            .Apply(Prim.Pure(CoProduct.Right<L, Unit>(default)));

    public Either<L, A> RepeatWhile(Schedule schedule, Func<A, bool> pred) =>
        Morphism.constant<CoProduct<L, Unit>, CoProduct<L, A>>(ObjectSafe).RepeatWhile(schedule, pred)
            .Apply(Prim.Pure(CoProduct.Right<L, Unit>(default)));

    public Either<L, A> RepeatUntil(Schedule schedule, Func<A, bool> pred) =>
        Morphism.constant<CoProduct<L, Unit>, CoProduct<L, A>>(ObjectSafe).RepeatUntil(schedule, pred)
            .Apply(Prim.Pure(CoProduct.Right<L, Unit>(default)));
    
    // -----------------------------------------------------------------------------------------------------------------
    // Retry

    public Either<L, A> Retry(Schedule schedule) =>
        Morphism.constant<CoProduct<L, Unit>, CoProduct<L, A>>(ObjectSafe)
            .Repeat(schedule)
            .Apply(Prim.Pure(CoProduct.Right<L, Unit>(default)));

    public Either<L, A> RetryWhile(Schedule schedule, Func<A, bool> pred) =>
        Morphism.constant<CoProduct<L, Unit>, CoProduct<L, A>>(ObjectSafe)
            .RepeatWhile(schedule, pred)
            .Apply(Prim.Pure(CoProduct.Right<L, Unit>(default)));

    public Either<L, A> RetryUntil(Schedule schedule, Func<A, bool> pred) =>
        Morphism.constant<CoProduct<L, Unit>, CoProduct<L, A>>(ObjectSafe)
            .RepeatUntil(schedule, pred)
            .Apply(Prim.Pure(CoProduct.Right<L, Unit>(default)));
    
    // -----------------------------------------------------------------------------------------------------------------
    // Folding

    public Either<L, S> Fold<S>(Schedule schedule, S state, Func<S, A, S> fold)=>
        Morphism.constant<CoProduct<L, Unit>, CoProduct<L, A>>(ObjectSafe)
            .Fold(schedule, state, fold)
            .Apply(Prim.Pure(CoProduct.Right<L, Unit>(default)));

    public Either<L, S> FoldWhile<S>(Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)=>
        Morphism.constant<CoProduct<L, Unit>, CoProduct<L, A>>(ObjectSafe)
            .FoldWhile(schedule, state, fold, pred)
            .Apply(Prim.Pure(CoProduct.Right<L, Unit>(default)));

    public Either<L, S> FoldUntil<S>(Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)=>
        Morphism.constant<CoProduct<L, Unit>, CoProduct<L, A>>(ObjectSafe)
            .FoldWhile(schedule, state, fold, pred)
            .Apply(Prim.Pure(CoProduct.Right<L, Unit>(default)));

    // -----------------------------------------------------------------------------------------------------------------
    // Operators
    
    public static Either<L, A> operator |(Either<L, A> ma, Either<L, A> mb) =>
        Obj.Choice<L, A>(ma, mb);

    public static implicit operator Obj<CoProduct<L, A>>(Either<L, A> ma) =>
        ma.ObjectSafe;

    public static implicit operator Either<L, A>(Obj<CoProduct<L, A>> obj) =>
        obj.ToEither();

    public static implicit operator Either<L, A>(L value) =>
        new(Prim.Pure(CoProduct.Left<L, A>(value)));

    public static implicit operator Either<L, A>(A value) =>
        new(Prim.Pure(CoProduct.Right<L, A>(value)));
}
