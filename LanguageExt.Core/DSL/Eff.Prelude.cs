#nullable enable

using System;
using LanguageExt.Common;

namespace LanguageExt.DSL;

public static partial class Prelude
{
    public static Eff<RT, RT> runtime<RT>() =>
        new(Morphism.function<RT, CoProduct<Error, RT>>(CoProduct.Right<Error, RT>));

    public static Eff<OuterRT, A> localEff<OuterRT, InnerRT, A>(Func<OuterRT, InnerRT> f, Eff<InnerRT, A> ma) =>
        new(Morphism.bind<OuterRT, CoProduct<Error, A>>(ort => ma.Op.Apply(Prim.Pure(f(ort)))));
    
    public static Eff<RT, A> ToEff<RT, A>(this Morphism<RT, CoProduct<Error, A>> ma) =>
        new(ma);

    public static Eff<RT, A> ToEff<RT, A>(this Obj<CoProduct<Error, A>> ma) =>
        new(Morphism.constant<RT, CoProduct<Error, A>>(ma));

    public static Eff<RT, A> ToEff<RT, A>(this CoProduct<Error, A> ma) =>
        new(Morphism.constant<RT, CoProduct<Error, A>>(Prim.Pure(ma)));

    public static Eff<RT, A> SuccessEff<RT, A>(A value) =>
        new(Morphism.constant<RT, CoProduct<Error, A>>(Prim.Pure(CoProduct.Right<Error, A>(value))));
    
    public static Eff<RT, A> FailEff<RT, A>(Error value) =>
        new(Morphism.constant<RT, CoProduct<Error, A>>(Prim.Pure(CoProduct.Left<Error, A>(value))));

    public static Eff<RT, A> EffMaybe<RT, A>(Func<RT, Fin<A>> f) =>
        new(Morphism.bind<RT, CoProduct<Error, A>>(rt => f(rt)
            .Match(Succ: x => Obj.Pure(CoProduct.Right<Error, A>(x)),
                   Fail: x => Obj.Pure(CoProduct.Left<Error, A>(x)))));

    public static Eff<RT, A> Eff<RT, A>(Func<RT, A> f) =>
        new(Morphism.bind<RT, CoProduct<Error, A>>(rt => Prim.Pure(CoProduct.Right<Error, A>(f(rt)))));
    
    public static Eff<RT, B> Apply<RT, A, B>(this Eff<RT, Func<A, B>> ff, Eff<RT, A> fa) =>
        ff.Bind(fa.Map);

    public static Eff<RT, Func<B, C>> Apply<RT, A, B, C>(this Eff<RT, Func<A, B, C>> ff, Eff<RT, A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Eff<RT, Func<B, Func<C, D>>> Apply<RT, A, B, C, D>(this Eff<RT, Func<A, B, C, D>> ff, Eff<RT, A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Eff<RT, Func<B, Func<C, Func<D, E>>>> Apply<RT, A, B, C, D, E>(this Eff<RT, Func<A, B, C, D, E>> ff, Eff<RT, A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Eff<RT, Func<B, Func<C, Func<D, Func<E, F>>>>> Apply<RT, A, B, C, D, E, F>(this Eff<RT, Func<A, B, C, D, E, F>> ff, Eff<RT, A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Eff<RT, A> use<RT, A>(Eff<RT, A> ma) where A : IDisposable =>
        ma.Map(Morphism.use<A>());

    public static Eff<RT, A> use<RT, A>(Eff<RT, A> ma, Func<A, Unit> release) =>
        ma.Map(Morphism.use(Morphism.function(release)));

    public static Eff<RT, Unit> release<RT, A>(Eff<RT, A> ma) =>
        ma.Map(Morphism<A>.release);

    public static Morphism<RT, CoProduct<Error, B>> Bind<RT, A, B>(
        this Morphism<RT, CoProduct<Error, A>> ma,
        Func<A, Eff<RT, B>> f) =>
        Morphism.kleisli<Eff<RT, B>, RT, Error, A, B>(ma, f);
    
    public static Morphism<RT, CoProduct<Error, B>> SelectMany<RT, A, B>(
        this Morphism<RT, CoProduct<Error, A>> ma,
        Func<A, Eff<RT, B>> f) =>
        Morphism.kleisli<Eff<RT, B>, RT, Error, A, B>(ma, f);
    
    public static Morphism<RT, CoProduct<Error, C>> SelectMany<RT, A, B, C>(
        this Morphism<RT, CoProduct<Error, A>> ma,
        Func<A, Eff<RT, B>> bind,
        Func<A, B, C> project) =>
        Morphism.kleisliProject(ma, bind, project);
    
// TODO: This UNIT inputting extension needs its variants (Bind, etc.)
    public static Morphism<RT, CoProduct<Error, C>> SelectMany<RT, A, B, C>(
        this Morphism<Unit, CoProduct<Error, A>> ma,
        Func<A, Eff<RT, B>> bind,
        Func<A, B, C> project) =>
        Morphism.kleisliProject(Morphism.map<RT,CoProduct<Error, A>>(_ => ma.Apply(Prim.Unit)), bind, project);

    public static Morphism<RT, CoProduct<Error, B>> Bind<RT, A, B>(
        this CoProduct<Error, A> ma,
        Func<A, Eff<RT, B>> f) =>
        Morphism.constant<RT, CoProduct<Error, A>>(Prim.Pure(ma)).Bind(f);
    
    public static Morphism<RT, CoProduct<Error, B>> SelectMany<RT, A, B>(
        this CoProduct<Error, A> ma,
        Func<A, Eff<RT, B>> f) =>
        Morphism.constant<RT, CoProduct<Error, A>>(Prim.Pure(ma)).Bind(f);
    
    public static Morphism<RT, CoProduct<Error, C>> SelectMany<RT, A, B, C>(
        this CoProduct<Error, A> ma,
        Func<A, Eff<RT, B>> bind,
        Func<A, B, C> project) =>
        Morphism.constant<RT, CoProduct<Error, A>>(Prim.Pure(ma)).SelectMany(bind, project);
}
