#nullable enable

using System;

namespace LanguageExt.DSL.Transducers;

public static partial class Transducer
{
    /// <summary>
    /// Apply an argument to a transducer
    /// </summary>
    /// <remarks>Collects zero or more results</remarks>
    /// <param name="mf">Transducer</param>
    /// <param name="ma">Argument</param>
    /// <returns>`PrimPure<B> | PrimMany<B> | PrimFail<B>`</returns>
    public static Prim<B> Apply<A, B>(this Transducer<A, B> mf, A ma) =>
        apply(ma, mf);

    /// <summary>
    /// Apply an argument to a transducer
    /// </summary>
    /// <remarks>Collects zero or one results</remarks>
    /// <param name="mf">Transducer</param>
    /// <param name="ma">Argument</param>
    /// <returns>`PrimPure<B> | PrimFail<B>`</returns>
    public static Prim<B> Apply1<A, B>(this Transducer<A, B> mf, A ma) =>
        apply1(ma, mf);

    /// <summary>
    /// Composes a constant transducer that yields `x` with the supplied transducer `mf` 
    /// </summary>
    /// <remarks>
    /// Allows for a value to be injected into a composition without loses the transducer type (for further composition)
    /// </remarks>
    /// <param name="mf">Transducer</param>
    /// <param name="ma">Argument</param>
    /// <returns>Transducer that when invoked with `unit` will run the supplied transducer with the constant value `x`</returns>
    public static Transducer<Unit, B> Inject<A, B>(this Transducer<A, B> mf, A x) =>
        inject(x, mf);

    /// <summary>
    /// Ignores the result of the transducer
    /// </summary>
    public static Transducer<A, Unit> Ignore<A, B>(this Transducer<A, B> mf) =>
        ignore(mf);
    
    public static Transducer<A, C> Map<A, B, C>(this Transducer<A, B> t, Transducer<B, C> f) =>
        compose(t, f);
    
    public static Transducer<A, C> Map<A, B, C>(this Transducer<A, B> t, Func<B, C> f) =>
        Map(t, map(f));

    public static Transducer<A, C> Select<A, B, C>(this Transducer<A, B> t, Transducer<B, C> f) =>
        Map(t, f);

    public static Transducer<A, C> Select<A, B, C>(this Transducer<A, B> t, Func<B, C> f) =>
        Map(t, map(f));
    
    public static Transducer<RT, B> Bind<RT, A, B>(
        this Transducer<RT, A> t,
        Func<A, Transducer<RT, B>> b) =>
        flatten(compose(t, map(b)));

    public static Transducer<RT, B> Bind<RT, A, B>(
        this Transducer<Unit, A> t,
        Func<A, Transducer<RT, B>> b) =>
        flatten(compose(t, map(b)));

    public static Transducer<RT, B> Bind<RT, A, B>(
        this Transducer<RT, A> t,
        Func<A, Transducer<Unit, B>> b) =>
        flatten(compose(t, map(b)));

    public static Transducer<Unit, B> Bind<A, B>(
        this Transducer<Unit, A> t,
        Func<A, Transducer<Unit, B>> b) =>
        flatten(compose(t, map(b)));
    
    public static Transducer<RT, B> SelectMany<RT, A, B>(
        this Transducer<RT, A> t,
        Func<A, Transducer<RT, B>> b) =>
        Bind(t, b);

    public static Transducer<RT, B> SelectMany<RT, A, B>(
        this Transducer<Unit, A> t,
        Func<A, Transducer<RT, B>> b) =>
        Bind(t, b);

    public static Transducer<RT, B> SelectMany<RT, A, B>(
        this Transducer<RT, A> t,
        Func<A, Transducer<Unit, B>> b) =>
        Bind(t, b);

    public static Transducer<RT, C> SelectMany<RT, A, B, C>(
        this Transducer<RT, A> t,
        Func<A, Transducer<RT, B>> b,
        Func<A, B, C> p) =>
        t.Bind(x => b(x).Map(y => p(x, y)));    

    public static Transducer<RT, C> SelectMany<RT, A, B, C>(
        this Transducer<Unit, A> t,
        Func<A, Transducer<RT, B>> b,
        Func<A, B, C> p) =>
        t.Bind(x => b(x).Map(y => p(x, y)));    

    public static Transducer<RT, C> SelectMany<RT, A, B, C>(
        this Transducer<RT, A> t,
        Func<A, Transducer<Unit, B>> b,
        Func<A, B, C> p) =>
        t.Bind(x => b(x).Map(y => p(x, y)));    

    public static Transducer<Unit, C> SelectMany<A, B, C>(
        this Transducer<Unit, A> t,
        Func<A, Transducer<Unit, B>> b,
        Func<A, B, C> p) =>
        t.Bind(x => b(x).Map(y => p(x, y)));    
}
