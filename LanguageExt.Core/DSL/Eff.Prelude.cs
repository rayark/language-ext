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
    public static Eff<RT, A> EffMaybe<RT, A>(Func<RT, Fin<A>> f) =>
        new(map<RT, CoProduct<Error, A>>(rt => f(rt)
            .Match(Succ: CoProduct.Right<Error, A>,
                Fail: CoProduct.Left<Error, A>)));

    /// <summary>
    /// Construct an effect that will either succeed or have an exceptional failure
    /// </summary>
    /// <param name="f">Function to capture the effect</param>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <returns>Synchronous IO monad that captures the effect</returns>
    public static Eff<RT, A> Eff<RT, A>(Func<RT, A> f) =>
        new(map<RT, CoProduct<Error, A>>(rt => CoProduct.Right<Error, A>(f(rt))));
    
    public static Eff<RT, A> SuccessEff<RT, A>(A value) =>
        new(constant<RT, CoProduct<Error, A>>(CoProduct.Right<Error, A>(value)));
    
    public static Eff<RT, A> FailEff<RT, A>(Error value) =>
        new(constant<RT, CoProduct<Error, A>>(CoProduct.Left<Error, A>(value)));

    public static Eff<RT, RT> runtime<RT>() =>
        new(map<RT, CoProduct<Error, RT>>(CoProduct.Right<Error, RT>));

    public static Eff<OuterRT, A> localEff<OuterRT, InnerRT, A>(Func<OuterRT, InnerRT> f, Eff<InnerRT, A> ma) =>
        new(map<OuterRT, CoProduct<Error, A>>(rt => ma.Run(f(rt))
            .Match(Succ: CoProduct.Right<Error, A>,
                   Fail: CoProduct.Left<Error, A>)));

    public static Eff<RT, A> scope1<RT, A>(Eff<RT, A> ma) =>
        new(Transducer.scope1(ma.Morphism));

    public static Eff<RT, Seq<A>> scope<RT, A>(Eff<RT, A> ma) =>
        new(Transducer.scope(ma.Morphism));
    
    public static Eff<RT, A> use<RT, A>(Eff<RT, A> ma) where A : IDisposable =>
        ma.Map(TransducerD<A>.use);

    public static Eff<RT, A> use<RT, A>(Eff<RT, A> ma, Func<A, Unit> release) =>
        ma.Map(Transducer.use(release));
    
}
