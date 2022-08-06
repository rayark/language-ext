#nullable enable
using System;
using LanguageExt.TypeClasses;

namespace LanguageExt.DSL;

public static partial class DSL<MErr, E>
    where MErr : struct, Semigroup<E>, Convertable<Exception, E>
{
    public static class BiMorphism
    {
        public static BiMorphism<MErrF, F, A, B> function<MErrF, F, A, B>(Func<E, F> Left, Func<A, B> Right)
            where MErrF : struct, Semigroup<F>, Convertable<Exception, F> =>
            new BiMapMorphism<MErrF, F, A, B>(Left, Right);
    }

    public abstract record BiMorphism<MErrF, F, A, B>
        where MErrF : struct, Semigroup<F>, Convertable<Exception, F>
    {
        public DSL<MErrF, F>.Obj<B> Apply(Obj<A> value) =>
            new ApplyObj<MErrF, F, A, B>(this, value);

        public abstract DSL<MErrF, F>.Prim<B> Invoke<RT>(State<RT> state, Prim<A> value);
    }

    internal record BiMapMorphism<MErrF, F, A, B>(Func<E, F> Left, Func<A, B> Right) : BiMorphism<MErrF, F, A, B>
        where MErrF : struct, Semigroup<F>, Convertable<Exception, F>
    {
        public override DSL<MErrF, F>.Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
            value.BiMap<MErrF, F, B>(Left, Right);
    }

    internal record ApplyObj<MErrF, F, A, B>(BiMorphism<MErrF, F, A, B> Morphism, Obj<A> Argument) : DSL<MErrF, F>.Obj<B>
        where MErrF : struct, Semigroup<F>, Convertable<Exception, F>
    {
        public override DSL<MErrF, F>.Prim<B> Interpret<RT>(State<RT> state) =>
            Morphism.Invoke(state, Argument.Interpret(state));
    }
}

