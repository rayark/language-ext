#nullable enable

using System;
using LanguageExt.Common;
using LanguageExt.DSL.Transducers;
using static LanguageExt.DSL.Transducers.Transducer;

namespace LanguageExt.DSL;

public static partial class Prelude
{
    public static Eff<A> ToEff<A>(this Transducer<DefaultRuntime, CoProduct<Error, A>> ma) =>
        new(ma);

    public static Eff<A> ToEff<A>(this CoProduct<Error, A> ma) =>
        new(constant<DefaultRuntime, CoProduct<Error, A>>(ma));

    public static Eff<B> Apply<A, B>(this Eff<Func<A, B>> ff, Eff<A> fa) =>
        ff.Bind(fa.Map);

    public static Eff<Func<B, C>> Apply<A, B, C>(this Eff<Func<A, B, C>> ff, Eff<A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Eff<Func<B, Func<C, D>>> Apply<A, B, C, D>(this Eff<Func<A, B, C, D>> ff, Eff<A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Eff<Func<B, Func<C, Func<D, E>>>> Apply<A, B, C, D, E>(this Eff<Func<A, B, C, D, E>> ff, Eff<A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Eff<Func<B, Func<C, Func<D, Func<E, F>>>>> Apply<A, B, C, D, E, F>(this Eff<Func<A, B, C, D, E, F>> ff, Eff<A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);    
    
    public static Transducer<DefaultRuntime, CoProduct<Error, B>> Bind<A, B>(
        this Transducer<DefaultRuntime, CoProduct<Error, A>> ma,
        Func<A, Eff<B>> f) =>
        bind<DefaultRuntime, Error, A, B, Eff<B>>(ma, f);
    
    public static Transducer<DefaultRuntime, CoProduct<Error, B>> SelectMany<A, B>(
        this Transducer<DefaultRuntime, CoProduct<Error, A>> ma,
        Func<A, Eff<B>> f) =>
        bind<DefaultRuntime, Error, A, B, Eff<B>>(ma, f);
    
    public static Transducer<DefaultRuntime, CoProduct<Error, C>> SelectMany<A, B, C>(
        this Transducer<DefaultRuntime, CoProduct<Error, A>> ma,
        Func<A, Eff<B>> bind,
        Func<A, B, C> project) =>
        ma.SelectMany(a => bind(a).Map(b => project(a, b)));

    public static Eff<C> SelectMany<A, B, C>(
        this Transducer<Unit, A> ma,
        Func<A, Eff<B>> bind,
        Func<A, B, C> project)
    {
        var ta = compose(map<DefaultRuntime, Unit>(_ => default), compose(ma, TransducerStatic2<Error, A>.right));
        return ta.ToEff().SelectMany(bind, project);
    }

    public static Transducer<DefaultRuntime, CoProduct<Error, B>> Bind<A, B>(
        this CoProduct<Error, A> ma,
        Func<A, Eff<B>> f) =>
        constant<DefaultRuntime, CoProduct<Error, A>>(ma).Bind(f);
    
    public static Transducer<DefaultRuntime, CoProduct<Error, B>> SelectMany<A, B>(
        this CoProduct<Error, A> ma,
        Func<A, Eff<B>> f) =>
        constant<DefaultRuntime, CoProduct<Error, A>>(ma).Bind(f);
    
    public static Transducer<DefaultRuntime, CoProduct<Error, C>> SelectMany<A, B, C>(
        this CoProduct<Error, A> ma,
        Func<A, Eff<B>> bind,
        Func<A, B, C> project) =>
        constant<DefaultRuntime, CoProduct<Error, A>>(ma).SelectMany(bind, project);
}
