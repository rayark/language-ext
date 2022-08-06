#nullable enable

using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using LanguageExt.TypeClasses;

namespace LanguageExt.DSL;

public readonly struct MFail<L> : Semigroup<L>, Convertable<Exception, L>
{
    public L Append(L x, L y) =>
        x;

    public L Convert(Exception ex) =>
        ex.Rethrow<L>();
}

public record struct Either<L, A>(DSL<MFail<L>, L>.Prim<A> Object)
{
    public static readonly Either<L, A> Bottom = new(DSL<MFail<L>, L>.Prim<A>.None);

    DSL<MFail<L>, L>.Obj<A> ObjectSafe => Object ?? Bottom;
    
    internal static readonly State<Unit> NilState = State<Unit>.Create(default);
    
    public Either<L, B> Map<B>(Func<A, B> f) =>
        DSL<MFail<L>, L>.Morphism.function(f).Apply(ObjectSafe).ToEither();

    public Either<L, B> Bind<B>(Func<A, Either<L, B>> f) =>
        DSL<MFail<L>, L>.Morphism.bind<A, B>(x => f(x)).Apply(ObjectSafe).ToEither();

    public Either<L, B> SelectMany<B>(Func<A, Either<L, B>> f) =>
        DSL<MFail<L>, L>.Morphism.bind<A, B>(x => f(x)).Apply(ObjectSafe).ToEither();

    public Either<L, C> SelectMany<B, C>(Func<A, Either<L, B>> bind, Func<A, B, C> project) =>
        DSL<MFail<L>, L>.Morphism.bind(x => bind(x), project).Apply(ObjectSafe).ToEither();

    public Either<L, B> Bind<B>(Func<A, IEnumerable<B>> f) =>
        Bind(a => Prelude.liftEither<L, B>(f(a)));

    public Either<L, B> SelectMany<B>(Func<A, IEnumerable<B>> f) =>
        Bind(f);

    public Either<L, C> SelectMany<B, C>(Func<A, IEnumerable<B>> bind, Func<A, B, C> project) =>
        Bind(x => bind(x).Map(y => project(x, y)));

    public Either<L, B> Bind<B>(Func<A, IObservable<B>> f) =>
        Bind(a => Prelude.liftEither<L, B>(f(a)));

    public Either<L, B> SelectMany<B>(Func<A, IObservable<B>> f) =>
        Bind(f);

    public Either<L, C> SelectMany<B, C>(Func<A, IObservable<B>> bind, Func<A, B, C> project) =>
        Bind(x => bind(x).Select(y => project(x, y)));

    public Either<L, A> Filter(Func<A, bool> f) =>
        DSL<MFail<L>, L>.Morphism.filter(f).Apply(ObjectSafe).ToEither();

    public Either<L, A> Where(Func<A, bool> f) =>
        DSL<MFail<L>, L>.Morphism.filter(f).Apply(ObjectSafe).ToEither();

    public Either<M, B> BiMap<M, B>(Func<L, M> Left, Func<A, B> Right) =>
        DSL<MFail<L>, L>.BiMorphism.function<MFail<M>, M, A, B>(Left, Right).Apply(Object);

    public Either<L, A> Head =>
        DSL<MFail<L>, L>.Morphism<A>.head.Apply(ObjectSafe).ToEither();

    public Either<L, A> Tail =>
        DSL<MFail<L>, L>.Morphism<A>.tail.Apply(ObjectSafe).ToEither();

    public Either<L, A> Last =>
        DSL<MFail<L>, L>.Morphism<A>.last.Apply(ObjectSafe).ToEither();

    public Either<L, A> Skip(int amount) =>
        DSL<MFail<L>, L>.Morphism.skip<A>(amount).Apply(ObjectSafe).ToEither();

    public Either<L, A> Take(int amount) =>
        DSL<MFail<L>, L>.Morphism.take<A>(amount).Apply(ObjectSafe).ToEither();

    public static Either<L, A> operator |(Either<L, A> ma, Either<L, A> mb) =>
        DSL<MFail<L>, L>.Obj.Choice<A>(ma, mb).ToEither();

    public static implicit operator DSL<MFail<L>, L>.Obj<A>(Either<L, A> ma) =>
        ma.ObjectSafe;

    public static implicit operator Either<L, A>(DSL<MFail<L>, L>.Obj<A> obj) =>
        obj.ToEither();
}

public static partial class Prelude
{
    public static Either<L, A> ToEither<L, A>(this DSL<MFail<L>, L>.Obj<A> ma) =>
        new(ma.Interpret(Either<L, A>.NilState));
    
    public static Either<L, A> Right<L, A>(A value) =>
        DSL<MFail<L>, L>.Prim.Pure(value);
    
    public static Either<L, A> Left<L, A>(L value) =>
        DSL<MFail<L>, L>.Prim.Left<A>(value);

    public static Either<L, A> liftEither<L, A>(IObservable<A> ma) =>
        DSL<MFail<L>, L>.Prim.Observable(ma.Select(DSL<MFail<L>, L>.Prim.Pure));

    public static Either<L, A> liftEither<L, A>(IEnumerable<A> ma) =>
        DSL<MFail<L>, L>.Prim.Many(ma.Map(DSL<MFail<L>, L>.Prim.Pure).ToSeq());

    public static Either<L, B> Apply<L, A, B>(this Either<L, Func<A, B>> ff, Either<L, A> fa) =>
        DSL<MFail<L>, L>.Morphism.bind<Func<A, B>, B>(f => DSL<MFail<L>, L>.Morphism.function(f).Apply(fa)).Apply(ff);

    public static Either<L, Func<B, C>> Apply<L, A, B, C>(this Either<L, Func<A, B, C>> ff, Either<L, A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Either<L, Func<B, Func<C, D>>> Apply<L, A, B, C, D>(this Either<L, Func<A, B, C, D>> ff, Either<L, A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Either<L, Func<B, Func<C, Func<D, E>>>> Apply<L, A, B, C, D, E>(this Either<L, Func<A, B, C, D, E>> ff, Either<L, A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Either<L, Func<B, Func<C, Func<D, Func<E, F>>>>> Apply<L, A, B, C, D, E, F>(this Either<L, Func<A, B, C, D, E, F>> ff, Either<L, A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Either<L, A> use<L, A>(Either<L, A> ma) where A : IDisposable =>
        DSL<MFail<L>, L>.Obj.Use<A>(ma);

    public static Either<L, A> use<L, A>(Either<L, A> ma, Func<A, Unit> release) =>
        DSL<MFail<L>, L>.Obj.Use(ma, DSL<MFail<L>, L>.Morphism.function(release));

    public static Either<L, A> use<L, A>(Either<L, A> ma, Func<A, Either<L, Unit>> release)  =>
        DSL<MFail<L>, L>.Obj.Use(ma, DSL<MFail<L>, L>.Morphism.bind<A, Unit>(x => release(x)));

    public static Either<L, Unit> release<L, A>(Either<L, A> ma) =>
        DSL<MFail<L>, L>.Obj.Release<A>(ma);

    public static Either<L, B> Bind<L, A, B>(this IEnumerable<A> ma, Func<A, Either<L, B>> f) =>
        liftEither<L, A>(ma).Bind(f);

    public static Either<L, B> SelectMany<L, A, B>(this IEnumerable<A> ma, Func<A, Either<L, B>> f) =>
        liftEither<L, A>(ma).Bind(f);

    public static Either<L, C> SelectMany<L, A, B, C>(this IEnumerable<A> ma, Func<A, Either<L, B>> bind, Func<A, B, C> project) =>
        liftEither<L, A>(ma).SelectMany(bind, project);

    public static Either<L, B> Bind<L, A, B>(this IObservable<A> ma, Func<A, Either<L, B>> f) =>
        liftEither<L, A>(ma).Bind(f);

    public static Either<L, B> SelectMany<L, A, B>(this IObservable<A> ma, Func<A, Either<L, B>> f) =>
        liftEither<L, A>(ma).Bind(f);

    public static Either<L, C> SelectMany<L, A, B, C>(this IObservable<A> ma, Func<A, Either<L, B>> bind, Func<A, B, C> project) =>
        liftEither<L, A>(ma).SelectMany(bind, project);
}
