#nullable enable

using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using LanguageExt.Common;
using LanguageExt.TypeClasses;

namespace LanguageExt.DSL;

public readonly struct MFail<L> : Semigroup<L>, Convertable<Exception, L>
{
    public L Append(L x, L y) =>
        x;

    public L Convert(Exception ex) =>
        ex.Rethrow<L>();
}

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
    // Match

    public B Match<B>(Func<L, B> Left, Func<A, B> Right)
    {
        return Go(ObjectSafe.Interpret(NilState));

        B Go(Prim<CoProduct<L, A>> ma) =>
            ma.Interpret(NilState) switch
            {
                PurePrim<CoProduct<L, A>> {Value: CoProductRight<L, A> r} => Right(r.Value),
                PurePrim<CoProduct<L, A>> {Value: CoProductLeft<L, A> l} => Left(l.Value),
                ManyPrim<CoProduct<L, A>> m => Go(m.Head),
                _ => throw new NotSupportedException()
            };
    }
    
    public Seq<B> MatchMany<B>(Func<L, B> Left, Func<A, B> Right)
    {
        return Go(ObjectSafe.Interpret(NilState));

        Seq<B> Go(Prim<CoProduct<L, A>> ma) =>
            ma.Interpret(NilState) switch
            {
                PurePrim<CoProduct<L, A>> {Value: CoProductRight<L, A> r} => LanguageExt.Prelude.Seq1(Right(r.Value)),
                PurePrim<CoProduct<L, A>> {Value: CoProductLeft<L, A> l} => LanguageExt.Prelude.Seq1(Left(l.Value)),
                ManyPrim<CoProduct<L, A>> m => m.Items.Map(Go).Flatten(),
                _ => throw new NotSupportedException()
            };
    }
    
    public Seq<B> MatchMany<B>(Func<L, Seq<B>> Left, Func<A, Seq<B>> Right)
    {
        return Go(ObjectSafe.Interpret(NilState));

        Seq<B> Go(Prim<CoProduct<L, A>> ma) =>
            ma.Interpret(NilState) switch
            {
                PurePrim<CoProduct<L, A>> {Value: CoProductRight<L, A> r} => Right(r.Value),
                PurePrim<CoProduct<L, A>> {Value: CoProductLeft<L, A> l} => Left(l.Value),
                ManyPrim<CoProduct<L, A>> m => m.Items.Map(Go).Flatten(),
                _ => throw new NotSupportedException()
            };
    }

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


    public Either<L, A> Filter(Func<A, bool> f) =>
        Bind(x => f(x) ? Prelude.Right<L, A>(x) : Bottom);

    public Either<L, A> Where(Func<A, bool> f) =>
        Filter(f);

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

public static partial class Prelude
{
    public static Either<L, A> ToEither<L, A>(this Obj<CoProduct<L, A>> ma) =>
        new(ma);
    
    public static Either<L, A> ToEither<L, A>(this Morphism<CoProduct<L, Unit>, CoProduct<L, A>> ma) =>
        new(ma.Apply(Prim.Pure(CoProduct.Right<L, Unit>(default))));
    
    public static Either<L, B> ToEither<L, A, B>(this Morphism<CoProduct<L, A>, CoProduct<L, B>> ma, A value) =>
        new(ma.Apply(Prim.Pure(CoProduct.Right<L, A>(value))));
    
    public static Either<L, A> Right<L, A>(A value) =>
        value;
    
    public static Either<L, A> Left<L, A>(L value) =>
        value;

    public static Morphism<Unit, A> map<A>(IObservable<A> ma) => 
        Morphism.each(ma);

    public static Morphism<CoProduct<X, Unit>, CoProduct<X, A>> bimap<X, A>(IObservable<A> ma) =>
        BiMorphism.bimap(Morphism<X>.identity, Morphism.each(ma));

    public static Obj<A> map<A>(IEnumerable<A> ma) =>
        Obj.Many(ma.Map(Prim.Pure));

    public static Either<L, B> Apply<L, A, B>(this Either<L, Func<A, B>> ff, Either<L, A> fa) =>
        ff.Bind(fa.Map);

    public static Either<L, Func<B, C>> Apply<L, A, B, C>(this Either<L, Func<A, B, C>> ff, Either<L, A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Either<L, Func<B, Func<C, D>>> Apply<L, A, B, C, D>(this Either<L, Func<A, B, C, D>> ff, Either<L, A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Either<L, Func<B, Func<C, Func<D, E>>>> Apply<L, A, B, C, D, E>(this Either<L, Func<A, B, C, D, E>> ff, Either<L, A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Either<L, Func<B, Func<C, Func<D, Func<E, F>>>>> Apply<L, A, B, C, D, E, F>(this Either<L, Func<A, B, C, D, E, F>> ff, Either<L, A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Either<L, A> use<L, A>(Either<L, A> ma) where A : IDisposable =>
        ma.Map(Morphism.use<A>());

    public static Either<L, A> use<L, A>(Either<L, A> ma, Func<A, Unit> release) =>
        ma.Map(Morphism.use(Morphism.function(release)));

    public static Either<L, Unit> release<L, A>(Either<L, A> ma) =>
        ma.Map(Morphism<A>.release);

    public static Either<L, B> Bind<L, A, B>(this Obj<CoProduct<L, A>> ma, Func<A, Either<L, B>> f) =>
        new Either<L, A>(ma).Bind(f);

    public static Either<L, B> SelectMany<L, A, B>(this Obj<CoProduct<L, A>> ma, Func<A, Either<L, B>> f) =>
        new Either<L, A>(ma).Bind(f);

    public static Either<L, C> SelectMany<L, A, B, C>(this Obj<CoProduct<L, A>> ma, Func<A, Either<L, B>> bind, Func<A, B, C> project) =>
        new Either<L, A>(ma).SelectMany(bind, project);

    
    public static Morphism<CoProduct<L, A>, CoProduct<L, C>> Bind<L, A, B, C>(
        this Morphism<CoProduct<L, A>, CoProduct<L, B>> ma, 
        Func<B, Either<L, C>> f) =>
        ma.Compose(BiMorphism.rightBind<ObjEither<L, C>, Either<L, C>, L, B, C>(f));
    
    public static Morphism<CoProduct<L, A>, CoProduct<L, C>> SelectMany<L, A, B, C>(
        this Morphism<CoProduct<L, A>, CoProduct<L, B>> ma, 
        Func<B, Either<L, C>> f) =>
        ma.Bind(f);
    
    public static Morphism<CoProduct<L, A>, CoProduct<L, D>> SelectMany<L, A, B, C, D>(
        this Morphism<CoProduct<L, A>, CoProduct<L, B>> ma, 
        Func<B, Either<L, C>> bind,
        Func<B, C, D> project) =>
        ma.Compose(BiMorphism.rightBind<ObjEither<L, C>, Either<L, C>, L, B, C, D>(
            bind, 
            Morphism.function(project)));


    public static Morphism<CoProduct<L, A>, CoProduct<L, C>> Bind<L, A, B, C>(
        this Morphism<A, B> ma,
        Func<B, Either<L, C>> f) =>
        BiMorphism.rightMap<L, A, B>(ma).Bind(f);
    
    public static Morphism<CoProduct<L, A>, CoProduct<L, C>> SelectMany<L, A, B, C>(
        this Morphism<A, B> ma,
        Func<B, Either<L, C>> f) =>
        BiMorphism.rightMap<L, A, B>(ma).Bind(f);
    
    public static Morphism<CoProduct<L, A>, CoProduct<L, D>> SelectMany<L, A, B, C, D>(
        this Morphism<A, B> ma,
        Func<B, Either<L, C>> bind,
        Func<B, C, D> project) =>
        BiMorphism.rightMap<L, A, B>(ma).SelectMany(bind, project);
}
