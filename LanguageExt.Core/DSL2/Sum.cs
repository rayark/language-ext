#nullable enable
using System;

namespace LanguageExt.DSL2;

/// <summary>
/// Sum-type.  Represents either a value of type `A` or a value of type `X`.
/// </summary>
/// <remarks>Isomorphic to `Either<L, R>`</remarks>
/// <typeparam name="X">Alternative value type</typeparam>
/// <typeparam name="A">Value type</typeparam>
public abstract record Sum<X, A>
{
    /// <summary>
    /// Constructor of `SumLeft<X, A>`
    /// </summary>
    /// <param name="value"></param>
    /// <returns>`Sum<X, A>`</returns>
    public static Sum<X, A> Left(X value) => new SumLeft<X, A>(value);
    
    /// <summary>
    /// Constructor of `SumRight<X, A>`
    /// </summary>
    /// <param name="value"></param>
    /// <returns>`Sum<X, A>`</returns>
    public static Sum<X, A> Right(A value) => new SumRight<X, A>(value);

    /// <summary>
    /// Functor map
    /// </summary>
    /// <param name="f">Function that maps the bound value</param>
    /// <typeparam name="B">Type of the bound value post-mapping</typeparam>
    /// <returns>Mapped functor</returns>
    public abstract Sum<X, B> Map<B>(Func<A, B> f);

    /// <summary>
    /// Functor bi-map
    /// </summary>
    public abstract Sum<Y, B> BiMap<B, Y>(Func<X, Y> Left, Func<A, B> Right);
}

/// <summary>
/// Left (alternative) case of the `SumType` union
/// </summary>
/// <param name="Value">Value of the case</param>
/// <typeparam name="X">Alternative value type</typeparam>
/// <typeparam name="A">Value type</typeparam>
public record SumLeft<X, A>(X Value) : Sum<X, A>
{
    /// <summary>
    /// Functor map
    /// </summary>
    /// <param name="f">Function that maps the bound value</param>
    /// <typeparam name="B">Type of the bound value post-mapping</typeparam>
    /// <returns>Mapped functor</returns>
    public override Sum<X, B> Map<B>(Func<A, B> f) =>
        new SumLeft<X, B>(Value);

    /// <summary>
    /// Functor bi-map
    /// </summary>
    public override Sum<Y, B> BiMap<B, Y>(Func<X, Y> Left, Func<A, B> Right) =>
        new SumLeft<Y, B>(Left(Value));
}

/// <summary>
/// Right (primary) case of the `SumType` union
/// </summary>
/// <param name="Value">Value of the case</param>
/// <typeparam name="X">Alternative value type</typeparam>
/// <typeparam name="A">Value type</typeparam>
public record SumRight<X, A>(A Value) : Sum<X, A>
{
    /// <summary>
    /// Functor map
    /// </summary>
    /// <param name="f">Function that maps the bound value</param>
    /// <typeparam name="B">Type of the bound value post-mapping</typeparam>
    /// <returns>Mapped functor</returns>
    public override Sum<X, B> Map<B>(Func<A, B> f) =>
        new SumRight<X, B>(f(Value));

    /// <summary>
    /// Functor bi-map
    /// </summary>
    public override Sum<Y, B> BiMap<B, Y>(Func<X, Y> Left, Func<A, B> Right) =>
        new SumRight<Y, B>(Right(Value));
}
