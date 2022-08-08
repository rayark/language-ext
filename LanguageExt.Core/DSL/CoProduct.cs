using System;
using LanguageExt.Common;

namespace LanguageExt.DSL;

public static class CoProduct
{
    public static CoProduct<A, B> Fail<A, B>(Error value) => new CoProductFail<A, B>(value);
    public static CoProduct<A, B> Left<A, B>(A value) => new CoProductLeft<A, B>(value);
    public static CoProduct<A, B> Right<A, B>(B value) => new CoProductRight<A, B>(value);
}

public abstract record CoProduct<A, B>
{
    public abstract CoProduct<X, B> LeftMap<X>(Func<A, X> Left);
    public abstract CoProduct<A, Y> RightMap<Y>(Func<B, Y> Right);
    public abstract CoProduct<X, Y> BiMap<X, Y>(Func<A, X> Left, Func<B, Y> Right);
    public abstract bool IsRight { get; }
    public abstract bool IsLeft { get; }
    public abstract bool IsError { get; }

    public static readonly Morphism<A, CoProduct<A, B>> leftId = 
        Morphism.function<A, CoProduct<A, B>>(CoProduct.Left<A, B>);
    
    public static readonly Morphism<B, CoProduct<A, B>> rightId = 
        Morphism.function<B, CoProduct<A, B>>(CoProduct.Right<A, B>);
    
    public static readonly Morphism<A, Obj<CoProduct<A, B>>> leftBind = 
        Morphism.function<A, Obj<CoProduct<A, B>>>(static x => Obj.Pure(CoProduct.Left<A, B>(x)));
    
    public static readonly Morphism<B, Obj<CoProduct<A, B>>> rightBind = 
        Morphism.function<B, Obj<CoProduct<A, B>>>(static x => Obj.Pure(CoProduct.Right<A, B>(x)));
}

public record CoProductLeft<A, B>(A Value) : CoProduct<A, B>
{
    public override CoProduct<X, B> LeftMap<X>(Func<A, X> Left) =>
        new CoProductLeft<X, B>(Left(Value));
    
    public override CoProduct<A, Y> RightMap<Y>(Func<B, Y> Right) =>
        new CoProductLeft<A, Y>(Value);
    
    public override CoProduct<X, Y> BiMap<X, Y>(Func<A, X> Left, Func<B, Y> Right) =>
        new CoProductLeft<X, Y>(Left(Value));

    public override bool IsRight => false;
    public override bool IsLeft => true;
    public override bool IsError => false;
}

public record CoProductRight<A, B>(B Value) : CoProduct<A, B>
{
    public override CoProduct<X, B> LeftMap<X>(Func<A, X> Left) =>
        new CoProductRight<X, B>(Value);
    
    public override CoProduct<A, Y> RightMap<Y>(Func<B, Y> Right) =>
        new CoProductRight<A, Y>(Right(Value));
    
    public override CoProduct<X, Y> BiMap<X, Y>(Func<A, X> Left, Func<B, Y> Right) =>
        new CoProductRight<X, Y>(Right(Value));

    public override bool IsRight => true;
    public override bool IsLeft => false;
    public override bool IsError => false;
}

public record CoProductFail<A, B>(Error Value) : CoProduct<A, B>
{
    public override CoProduct<X, B> LeftMap<X>(Func<A, X> Left) =>
        new CoProductFail<X, B>(Value);
    
    public override CoProduct<A, Y> RightMap<Y>(Func<B, Y> Right) =>
        new CoProductFail<A, Y>(Value);
    
    public override CoProduct<X, Y> BiMap<X, Y>(Func<A, X> Left, Func<B, Y> Right) =>
        new CoProductFail<X, Y>(Value);

    public override bool IsRight => false;
    public override bool IsLeft => true;
    public override bool IsError => true;

}
