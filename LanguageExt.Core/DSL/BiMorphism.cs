#nullable enable
using System;

namespace LanguageExt.DSL;

public abstract record BiMorphism<X, Y, A, B> : Morphism<CoProduct<X, A>, CoProduct<Y, B>>;

public static class BiMorphism
{
    public static BiMorphism<X, X, A, B> rightMap<X, A, B>(Morphism<A, B> Right) =>
        new BiMapMorphism<X, X, A, B>(Morphism<X>.identity, Right);

    public static BiMorphism<X, Y, A, A> leftMap<X, Y, A>(Morphism<X, Y> Left) =>
        new BiMapMorphism<X, Y, A, A>(Left, Morphism<A>.identity);

    public static BiMorphism<X, Y, A, B> bimap<X, Y, A, B>(Morphism<X, Y> Left, Morphism<A, B> Right) =>
        new BiMapMorphism<X, Y, A, B>(Left, Right);

    
    public static BiMorphism<X, X, A, B> rightBind<X, A, B>(Morphism<A, CoProduct<X, B>> Right) =>
        new BiBindMorphism<X, X, A, B>(CoProduct<X, B>.leftId, Right);

    public static BiMorphism<X, Y, A, A> leftBind<X, Y, A>(Morphism<X, CoProduct<Y, A>> Left) =>
        new BiBindMorphism<X, Y, A, A>(Left, CoProduct<Y, A>.rightId);

    public static BiMorphism<X, Y, A, B> bibind<X, Y, A, B>(Morphism<X, CoProduct<Y, B>> Left, Morphism<A, CoProduct<Y, B>> Right) =>
        new BiBindMorphism<X, Y, A, B>(Left, Right);

    
    public static BiMorphism<X, X, A, B> rightBind<X, A, B>(Morphism<A, Obj<CoProduct<X, B>>> Right) =>
        new BiBindMorphism2<X, X, A, B>(CoProduct<X, B>.leftBind, Right  );

    public static BiMorphism<X, Y, A, A> leftBind<X, Y, A>(Morphism<X, Obj<CoProduct<Y, A>>> Left) =>
        new BiBindMorphism2<X, Y, A, A>(Left, CoProduct<Y, A>.rightBind);

    public static BiMorphism<X, Y, A, B> bibind<X, Y, A, B>(Morphism<X, Obj<CoProduct<Y, B>>> Left, Morphism<A, Obj<CoProduct<Y, B>>> Right) =>
        new BiBindMorphism2<X, Y, A, B>(Left, Right);


    public static BiMorphism<X, X, A, B> rightBind<ObjB, MB, X, A, B>(Morphism<A, MB> Right)
        where ObjB : struct, IsObj<MB, CoProduct<X, B>> =>
        new BiBindMorphism<X, X, A, B>(CoProduct<X, B>.leftId, Morphism.isObj<ObjB, MB, A, CoProduct<X, B>>(Right));

    public static BiMorphism<X, X, A, B> rightBind<ObjB, MB, X, A, B>(Func<A, MB> Right)
        where ObjB : struct, IsObj<MB, CoProduct<X, B>> =>
        new BiBindMorphism<X, X, A, B>(CoProduct<X, B>.leftId, Morphism.isObj<ObjB, MB, A, CoProduct<X, B>>(Right));

    public static BiMorphism<X, Y, A, A> leftBind<ObjY, MY, X, Y, A>(Morphism<X, MY> Left) 
        where ObjY : struct, IsObj<MY, CoProduct<Y, A>> =>
        new BiBindMorphism<X, Y, A, A>(Morphism.isObj<ObjY, MY, X, CoProduct<Y, A>>(Left), CoProduct<Y, A>.rightId);

    public static BiMorphism<X, Y, A, A> leftBind<ObjY, MY, X, Y, A>(Func<X, MY> Left) 
        where ObjY : struct, IsObj<MY, CoProduct<Y, A>> =>
        new BiBindMorphism<X, Y, A, A>(Morphism.isObj<ObjY, MY, X, CoProduct<Y, A>>(Left), CoProduct<Y, A>.rightId);

    public static BiMorphism<X, Y, A, B> bibind<ObjM, M, X, Y, A, B>(Morphism<X, M> Left, Morphism<A, M> Right)
        where ObjM : struct, IsObj<M, CoProduct<Y, B>> =>
        new BiBindMorphism<X, Y, A, B>(
            Morphism.isObj<ObjM, M, X, CoProduct<Y, B>>(Left), 
            Morphism.isObj<ObjM, M, A, CoProduct<Y, B>>(Right));

    public static BiMorphism<X, Y, A, B> bibind<ObjM, M, X, Y, A, B>(Func<X, M> Left, Func<A, M> Right)
        where ObjM : struct, IsObj<M, CoProduct<Y, B>> =>
        new BiBindMorphism<X, Y, A, B>(
            Morphism.isObj<ObjM, M, X, CoProduct<Y, B>>(Left), 
            Morphism.isObj<ObjM, M, A, CoProduct<Y, B>>(Right));

    public static BiMorphism<X, X, A, C> rightBind<ObjB, MB, X, A, B, C>(
        Morphism<A, MB> Right,
        Morphism<A, Morphism<B, C>> project)
        where ObjB : struct, IsObj<MB, CoProduct<X, B>> =>
        rightBind(Morphism.isObj<ObjB, MB, A, CoProduct<X, B>>(Right), project);

    public static BiMorphism<X, X, A, C> rightBind<ObjB, MB, X, A, B, C>(
        Func<A, MB> Right,
        Morphism<A, Morphism<B, C>> project)
        where ObjB : struct, IsObj<MB, CoProduct<X, B>> =>
        rightBind(Morphism.isObj<ObjB, MB, A, CoProduct<X, B>>(Right), project);
    
    
    public static BiMorphism<X, X, A, C> rightBind<X, A, B, C>(
        Morphism<A, CoProduct<X, B>> Right,
        Morphism<A, Morphism<B, C>> project) =>
        new BiBindMorphism<X, X, A, C>(
            CoProduct<X, C>.leftId,
            Morphism.map<A, CoProduct<X, C>>(a =>
                Morphism.bind<Morphism<B, C>, CoProduct<X, C>>(
                        oproject => bimap(Morphism<X>.identity, oproject).Apply(Right.Apply(a)))
                    .Apply(project.Apply(a))));

    public static BiMorphism<X, X, A, C> rightBind<X, A, B, C>(
        Morphism<A, Obj<CoProduct<X, B>>> Right,
        Morphism<A, Morphism<B, C>> project) =>
        new BiBindMorphism<X, X, A, C>(
            CoProduct<X, C>.leftId,
            Morphism.map<A, CoProduct<X, C>>(a =>
                Morphism.bind<Morphism<B, C>, CoProduct<X, C>>(
                        oproject => bimap(Morphism<X>.identity, oproject).Apply(Right.Apply(a).Flatten()))
                    .Apply(project.Apply(a))));

}

public record BiBindMorphism<X, Y, A, B>(Morphism<X, CoProduct<Y, B>> Left, Morphism<A, CoProduct<Y, B>> Right) : BiMorphism<X, Y, A, B>
{
    public override Prim<CoProduct<Y, B>> Invoke<RT>(State<RT> state, Prim<CoProduct<X, A>> value) =>
        value.Bind(p => p switch
        {
            CoProductLeft<X, A> left => Left.Invoke(state, Prim.Pure(left.Value)).Interpret(state),
            CoProductRight<X, A> right => Right.Invoke(state, Prim.Pure(right.Value)).Interpret(state),
            CoProductFail<X, B> f => Prim.Fail<CoProduct<Y, B>>(f.Value),
            _ => throw new InvalidOperationException()
        });
}

public record BiBindMorphism2<X, Y, A, B>(Morphism<X, Obj<CoProduct<Y, B>>> Left, Morphism<A, Obj<CoProduct<Y, B>>> Right) : BiMorphism<X, Y, A, B>
{
    public override Prim<CoProduct<Y, B>> Invoke<RT>(State<RT> state, Prim<CoProduct<X, A>> value) =>
        value.Bind(p => p switch
        {
            CoProductLeft<X, A> left => Left.Invoke(state, Prim.Pure(left.Value)).Interpret(state).Flatten().Interpret(state),
            CoProductRight<X, A> right => Right.Invoke(state, Prim.Pure(right.Value)).Interpret(state).Flatten().Interpret(state),
            CoProductFail<X, B> f => Prim.Fail<CoProduct<Y, B>>(f.Value),
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
            CoProductFail<X, B> f => Prim.Fail<CoProduct<Y, B>>(f.Value),
            _ => throw new InvalidOperationException()
        });
}
