#nullable enable

using System;
using LanguageExt.Common;
using LanguageExt.DSL.Transducers;
using static LanguageExt.DSL.Transducers.Transducer;

namespace LanguageExt.DSL;

public static partial class Prelude
{
    public static Either<L, A> scope1<L, A>(Either<L, A> ma) =>
        new(Transducer.scope1(ma.Morphism));

    // TODO: Generalise for `L`
    public static Either<Error, Seq<A>> scope<A>(Either<Error, A> ma) =>
        new(compose(Transducer.scope(ma.Morphism), right<Error, Seq<A>>()));
    
    public static Either<L, A> use<L, A>(Either<L, A> ma) where A : IDisposable =>
        ma.Map(TransducerD<A>.use);

    public static Either<L, A> use<L, A>(Either<L, A> ma, Func<A, Unit> release) =>
        ma.Map(Transducer.use(release));

    public static Either<L, Unit> release<L, A>(Either<L, A> ma) =>
        ma.Map(Transducer<A>.release);
    
    public static Either<L, A> ToEither<L, A>(this Transducer<Unit, CoProduct<L, A>> ma) =>
        new(ma);

    public static Either<L, A> ToEither<L, A>(this CoProduct<L, A> ma) =>
        new(constant<Unit, CoProduct<L, A>>(ma));
    
    public static Either<L, A> Right<L, A>(A value) =>
        value;
    
    public static Either<L, A> Left<L, A>(L value) =>
        value;

    public static Either<L, A> RightLazy<L, A>(Func<A> value) =>
        new (map<Unit, CoProduct<L, A>>(_ => CoProduct.Right<L, A>(value())));

    public static Either<L, A> LeftLazy<L, A>(Func<L> value) =>
        new (map<Unit, CoProduct<L, A>>(_ => CoProduct.Left<L, A>(value())));
    
    public static Either<L, B> Apply<L, A, B>(this Either<L, Func<A, B>> ff, Either<L, A> fa) =>
        ff.Bind(fa.Map);

    public static Either<L, Func<B, C>> Apply<L, A, B, C>(this Either<L, Func<A, B, C>> ff, Either<L, A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Either<L, Func<B, Func<C, D>>> Apply<L, A, B, C, D>(this Either<L, Func<A, B, C, D>> ff, Either<L, A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Either<L, Func<B, Func<C, Func<D, E>>>> Apply<L, A, B, C, D, E>(this Either<L, Func<A, B, C, D, E>> ff, Either<L, A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Either<L, Func<B, Func<C, Func<D, Func<E, F>>>>> Apply<L, A, B, C, D, E, F>(this Either<L, Func<A, B, C, D, E, F>> ff, Either<L, A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);


    public static Transducer<Unit, CoProduct<L, B>> Bind<L, A, B>(
        this Transducer<Unit, CoProduct<L, A>> ma,
        Func<A, Either<L, B>> f) =>
        bind<Unit, L, A, B, Either<L, B>>(ma, f);
    
    public static Transducer<Unit, CoProduct<L, B>> SelectMany<L, A, B>(
        this Transducer<Unit, CoProduct<L, A>> ma,
        Func<A, Either<L, B>> f) =>
        bind<Unit, L, A, B, Either<L, B>>(ma, f);
    
    public static Transducer<Unit, CoProduct<L, C>> SelectMany<L, A, B, C>(
        this Transducer<Unit, CoProduct<L, A>> ma,
        Func<A, Either<L, B>> bind,
        Func<A, B, C> project) =>
        ma.SelectMany(a => bind(a).Map(b => project(a, b)));

    public static Either<L, C> SelectMany<L, A, B, C>(
        this Transducer<Unit, A> ma,
        Func<A, Either<L, B>> bind,
        Func<A, B, C> project)
    {
        var ta = compose(ma, right<L, A>());
        return ta.ToEither().SelectMany(bind, project);
    }
    public static Transducer<Unit, CoProduct<L, B>> Bind<L, A, B>(
        this CoProduct<L, A> ma,
        Func<A, Either<L, B>> f) =>
        constant<Unit, CoProduct<L, A>>(ma).Bind(f);
    
    public static Transducer<Unit, CoProduct<L, B>> SelectMany<L, A, B>(
        this CoProduct<L, A> ma,
        Func<A, Either<L, B>> f) =>
        constant<Unit, CoProduct<L, A>>(ma).Bind(f);
    
    public static Transducer<Unit, CoProduct<L, C>> SelectMany<L, A, B, C>(
        this CoProduct<L, A> ma,
        Func<A, Either<L, B>> bind,
        Func<A, B, C> project) =>
        constant<Unit, CoProduct<L, A>>(ma).SelectMany(bind, project);
}
