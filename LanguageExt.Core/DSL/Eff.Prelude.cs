#nullable enable

using System;
using LanguageExt.Common;
using LanguageExt.DSL.Transducers;
using static LanguageExt.DSL.Transducers.Transducer;

namespace LanguageExt.DSL;

public static partial class Prelude
{
    public static Eff<RT, RT> runtime<RT>() =>
        new(map<RT, CoProduct<Error, RT>>(CoProduct.Right<Error, RT>));

    public static Eff<OuterRT, A> localEff<OuterRT, InnerRT, A>(Func<OuterRT, InnerRT> f, Eff<InnerRT, A> ma) =>
        new(map<OuterRT, CoProduct<Error, A>>(rt => ma.Run(f(rt))
            .Match(Succ: CoProduct.Right<Error, A>,
                   Fail: CoProduct.Left<Error, A>)));

    public static Eff<RT, A> scope1<RT, A>(Eff<RT, A> ma) =>
        new(Transducer.scope1(ma.Morphism));

    public static Eff<RT, Seq<A>> scope<RT, A>(Eff<RT, A> ma) =>
        new(compose(Transducer.scope(ma.Morphism), right<Error, Seq<A>>()));
    
    public static Eff<RT, A> use<RT, A>(Eff<RT, A> ma) where A : IDisposable =>
        ma.Map(TransducerD<A>.use);

    public static Eff<RT, A> use<RT, A>(Eff<RT, A> ma, Func<A, Unit> release) =>
        ma.Map(Transducer.use(release));

    public static Eff<RT, Unit> release<RT, A>(Eff<RT, A> ma) =>
        ma.Map(Transducer<A>.release);
    
    public static Eff<RT, A> ToEff<RT, A>(this Transducer<RT, CoProduct<Error, A>> ma) =>
        new(ma);

    public static Eff<RT, A> ToEff<RT, A>(this CoProduct<Error, A> ma) =>
        new(constant<RT, CoProduct<Error, A>>(ma));

    public static Eff<RT, A> SuccessEff<RT, A>(A value) =>
        new(constant<RT, CoProduct<Error, A>>(CoProduct.Right<Error, A>(value)));
    
    public static Eff<RT, A> FailEff<RT, A>(Error value) =>
        new(constant<RT, CoProduct<Error, A>>(CoProduct.Left<Error, A>(value)));

    public static Eff<RT, A> EffMaybe<RT, A>(Func<RT, Fin<A>> f) =>
        new(map<RT, CoProduct<Error, A>>(rt => f(rt)
            .Match(Succ: CoProduct.Right<Error, A>,
                Fail: CoProduct.Left<Error, A>)));

    public static Eff<RT, A> Eff<RT, A>(Func<RT, A> f) =>
        new(map<RT, CoProduct<Error, A>>(rt => CoProduct.Right<Error, A>(f(rt))));
    
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
    
    public static Transducer<RT, CoProduct<Error, B>> Bind<RT, A, B>(
        this Transducer<RT, CoProduct<Error, A>> ma,
        Func<A, Eff<RT, B>> f) =>
        bind<RT, Error, A, B, Eff<RT, B>>(ma, f);
    
    public static Transducer<RT, CoProduct<Error, B>> SelectMany<RT, A, B>(
        this Transducer<RT, CoProduct<Error, A>> ma,
        Func<A, Eff<RT, B>> f) =>
        bind<RT, Error, A, B, Eff<RT, B>>(ma, f);
    
    public static Transducer<RT, CoProduct<Error, C>> SelectMany<RT, A, B, C>(
        this Transducer<RT, CoProduct<Error, A>> ma,
        Func<A, Eff<RT, B>> bind,
        Func<A, B, C> project) =>
        ma.SelectMany(a => bind(a).Map(b => project(a, b)));

    public static Eff<RT, C> SelectMany<RT, A, B, C>(
        this Transducer<Unit, A> ma,
        Func<A, Eff<RT, B>> bind,
        Func<A, B, C> project)
    {
        var ta = compose(map<RT, Unit>(_ => default), compose(ma, right<Error, A>()));
        return ta.ToEff().SelectMany(bind, project);
    }

    public static Transducer<RT, CoProduct<Error, B>> Bind<RT, A, B>(
        this CoProduct<Error, A> ma,
        Func<A, Eff<RT, B>> f) =>
        constant<RT, CoProduct<Error, A>>(ma).Bind(f);
    
    public static Transducer<RT, CoProduct<Error, B>> SelectMany<RT, A, B>(
        this CoProduct<Error, A> ma,
        Func<A, Eff<RT, B>> f) =>
        constant<RT, CoProduct<Error, A>>(ma).Bind(f);
    
    public static Transducer<RT, CoProduct<Error, C>> SelectMany<RT, A, B, C>(
        this CoProduct<Error, A> ma,
        Func<A, Eff<RT, B>> bind,
        Func<A, B, C> project) =>
        constant<RT, CoProduct<Error, A>>(ma).SelectMany(bind, project);
}
