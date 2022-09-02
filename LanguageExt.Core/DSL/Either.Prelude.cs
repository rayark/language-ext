/*
#nullable enable

using System;

namespace LanguageExt.DSL;

public static partial class Prelude
{
    public static Either<L, A> ToEither<L, A>(this Morphism<Unit, CoProduct<L, A>> ma) =>
        new(ma);

    public static Either<L, A> ToEither<L, A>(this Obj<CoProduct<L, A>> ma) =>
        Morphism.constant<Unit, CoProduct<L, A>>(ma).ToEither();

    public static Either<L, A> ToEither<L, A>(this CoProduct<L, A> ma) =>
        Morphism.constant<Unit, CoProduct<L, A>>(Prim.Pure(ma)).ToEither();
    
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

    public static Morphism<Unit, CoProduct<L, B>> Bind<L, A, B>(
        this Morphism<Unit, CoProduct<L, A>> ma,
        Func<A, Either<L, B>> f) =>
        Morphism.kleisli<Either<L, B>, Unit, L, A, B>(ma, f);
    
    public static Morphism<Unit, CoProduct<L, B>> SelectMany<L, A, B>(
        this Morphism<Unit, CoProduct<L, A>> ma,
        Func<A, Either<L, B>> f) =>
        Morphism.kleisli<Either<L, B>, Unit, L, A, B>(ma, f);
    
    public static Morphism<Unit, CoProduct<L, C>> SelectMany<L, A, B, C>(
        this Morphism<Unit, CoProduct<L, A>> ma,
        Func<A, Either<L, B>> bind,
        Func<A, B, C> project) =>
        Morphism.kleisliProject(ma, bind, project);

    public static Morphism<Unit, CoProduct<L, B>> Bind<L, A, B>(
        this CoProduct<L, A> ma,
        Func<A, Either<L, B>> f) =>
        Morphism.constant<Unit, CoProduct<L, A>>(Prim.Pure(ma)).Bind(f);
    
    public static Morphism<Unit, CoProduct<L, B>> SelectMany<L, A, B>(
        this CoProduct<L, A> ma,
        Func<A, Either<L, B>> f) =>
        Morphism.constant<Unit, CoProduct<L, A>>(Prim.Pure(ma)).Bind(f);
    
    public static Morphism<Unit, CoProduct<L, C>> SelectMany<L, A, B, C>(
        this CoProduct<L, A> ma,
        Func<A, Either<L, B>> bind,
        Func<A, B, C> project) =>
        Morphism.constant<Unit, CoProduct<L, A>>(Prim.Pure(ma)).SelectMany(bind, project);
}
*/
