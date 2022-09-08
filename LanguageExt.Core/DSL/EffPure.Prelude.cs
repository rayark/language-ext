#nullable enable

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LanguageExt.Common;
using LanguageExt.DSL.Transducers;
using static LanguageExt.DSL.Transducers.Transducer;

namespace LanguageExt.DSL;

public static partial class Prelude
{
    /// <summary>
    /// Construct an effect that will either succeed, have an exceptional, or unexceptional failure
    /// </summary>
    /// <param name="f">Function to capture the effect</param>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <returns>Synchronous IO monad that captures the effect</returns>
    [Pure, MethodImpl(Opt.Default)]
    public static Eff<A> EffMaybe<A>(Func<DefaultRuntime, Fin<A>> f) =>
        new(map<DefaultRuntime, CoProduct<Error, A>>(rt => f(rt)
            .Match(Succ: CoProduct.Right<Error, A>,
                Fail: CoProduct.Left<Error, A>)));

    /// <summary>
    /// Construct an effect that will either succeed or have an exceptional failure
    /// </summary>
    /// <param name="f">Function to capture the effect</param>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <returns>Synchronous IO monad that captures the effect</returns>
    public static Eff<A> Eff<A>(Func<DefaultRuntime, A> f) =>
        new(map<DefaultRuntime, CoProduct<Error, A>>(rt => CoProduct.Right<Error, A>(f(rt))));
    
    public static Eff<A> SuccessEff<A>(A value) =>
        new(constant<DefaultRuntime, CoProduct<Error, A>>(CoProduct.Right<Error, A>(value)));
    
    public static Eff<A> FailEff<A>(Error value) =>
        new(constant<DefaultRuntime, CoProduct<Error, A>>(CoProduct.Left<Error, A>(value)));

    public static Eff<DefaultRuntime> runtime() =>
        new(map<DefaultRuntime, CoProduct<Error, DefaultRuntime>>(CoProduct.Right<Error, DefaultRuntime>));

    public static Eff<A> localEff<A>(Func<DefaultRuntime, DefaultRuntime> f, Eff<A> ma) =>
        new(map<DefaultRuntime, CoProduct<Error, A>>(rt => ma.Run(f(rt))
            .Match(Succ: CoProduct.Right<Error, A>,
                   Fail: CoProduct.Left<Error, A>)));

    public static Eff<A> scope1<A>(Eff<A> ma) =>
        new(Transducer.scope1(ma.Morphism));

    public static Eff<Seq<A>> scope<A>(Eff<A> ma) =>
        new(Transducer.scope(ma.Morphism));
    
    public static Eff<A> use<A>(Eff<A> ma) where A : IDisposable =>
        ma.Map(TransducerD<A>.use);

    public static Eff<A> use<A>(Eff<A> ma, Func<A, Unit> release) =>
        ma.Map(Transducer.use(release));
    
}
