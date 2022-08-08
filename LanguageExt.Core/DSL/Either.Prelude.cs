#nullable enable

using System;
using System.Collections.Generic;
using LanguageExt.TypeClasses;

namespace LanguageExt.DSL;

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
