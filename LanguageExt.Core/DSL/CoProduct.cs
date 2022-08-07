using System;

namespace LanguageExt.DSL;

public static class CoProduct
{
    public static CoProduct<A, B> Left<A, B>(A value) => new CoProductLeft<A, B>(value);
    public static CoProduct<A, B> Right<A, B>(B value) => new CoProductRight<A, B>(value);
}

public abstract record CoProduct<A, B>
{
    public abstract CoProduct<X, Y> BiMap<X, Y>(Func<A, X> Left, Func<B, Y> Right);
    public abstract bool IsRight { get; }
    public abstract bool IsLeft { get; }
}

public record CoProductLeft<A, B>(A Value) : CoProduct<A, B>
{
    public override CoProduct<X, Y> BiMap<X, Y>(Func<A, X> Left, Func<B, Y> Right) =>
        new CoProductLeft<X, Y>(Left(Value));

    public override bool IsRight => false;
    public override bool IsLeft => true;

}
public record CoProductRight<A, B>(B Value) : CoProduct<A, B>
{
    public override CoProduct<X, Y> BiMap<X, Y>(Func<A, X> Left, Func<B, Y> Right) =>
        new CoProductRight<X, Y>(Right(Value));

    public override bool IsRight => true;
    public override bool IsLeft => false;
}
    
