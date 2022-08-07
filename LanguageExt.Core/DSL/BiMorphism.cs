#nullable enable
using System;

namespace LanguageExt.DSL;

public abstract record BiMorphism<X, Y, A, B> : Morphism<CoProduct<X, A>, CoProduct<Y, B>>;

public static class BiMorphism
{
    public static BiMorphism<X, Y, A, B> map<X, Y, A, B>(Morphism<X, Y> Left, Morphism<A, B> Right) =>
        new BiMapMorphism<X, Y, A, B>(Left, Right);

    public static BiMorphism<X, Y, A, B> bind<X, Y, A, B>(Morphism<X, CoProduct<Y, B>> Left, Morphism<A, CoProduct<Y, B>> Right) =>
        new BiBindMorphism<X, Y, A, B>(Left, Right);
}

public record BiBindMorphism<X, Y, A, B>(Morphism<X, CoProduct<Y, B>> Left, Morphism<A, CoProduct<Y, B>> Right) : BiMorphism<X, Y, A, B>
{
    public override Prim<CoProduct<Y, B>> Invoke<RT>(State<RT> state, Prim<CoProduct<X, A>> value) =>
        value.Bind(p => p switch
        {
            CoProductLeft<X, A> left => Left.Invoke(state, Prim.Pure(left.Value)).Interpret(state),
            CoProductRight<X, A> right => Right.Invoke(state, Prim.Pure(right.Value)).Interpret(state),
            _ => throw new InvalidOperationException()
        });
}

public record BiMapMorphism<X, Y, A, B>(Morphism<X, Y> Left, Morphism<A, B> Right) : BiMorphism<X, Y, A, B>
{
    public override Prim<CoProduct<Y, B>> Invoke<RT>(State<RT> state, Prim<CoProduct<X, A>> value) =>
        value.Bind(p => p switch
        {
            CoProductLeft<X, A> left => Left.Invoke(state, Prim.Pure(left.Value)).Map(CoProduct.Left<Y, B>),
            CoProductRight<X, A> right => Right.Invoke(state, Prim.Pure(right.Value)).Map(CoProduct.Right<Y, B>),
            _ => throw new InvalidOperationException()
        });
}
