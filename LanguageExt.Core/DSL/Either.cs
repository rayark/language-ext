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

public readonly record struct Either<L, A>(Obj<CoProduct<L, A>> Object)
{
    public static readonly Either<L, A> Bottom = new(Prim<CoProduct<L, A>>.None);
    
    static readonly Morphism<L, CoProduct<L, A>> leftId = Morphism.function<L, CoProduct<L, A>>(CoProduct.Left<L, A>);
    
    Obj<CoProduct<L, A>> ObjectSafe => Object ?? Bottom;
    
    internal static readonly State<Unit> NilState = State<Unit>.Create(default);

    public Either<L, B> Map<B>(Func<A, B> f) =>
        BiMap(Morphism<L>.identity, Morphism.function(f));

    public Either<L, B> Map<B>(Morphism<A, B> f) =>
        BiMap(Morphism<L>.identity, f);

    public Either<M, B> BiMap<M, B>(Func<L, M> Left, Func<A, B> Right) =>
        BiMap(Morphism.function(Left), Morphism.function(Right));

    public Either<M, B> BiMap<M, B>(Morphism<L, M> Left, Morphism<A, B> Right) =>
        new(BiMorphism.map(Left, Right).Apply(ObjectSafe));

    public Either<L, B> Bind<B>(Func<A, Either<L, B>> f) =>
        new(BiMorphism.bind(Either<L, B>.leftId, Morphism.bind<A, CoProduct<L, B>>(r => f(r).Object))
            .Apply(ObjectSafe));

    public Either<L, B> SelectMany<B>(Func<A, Either<L, B>> f) =>
        Bind(f);

    public Either<L, C> SelectMany<B, C>(Func<A, Either<L, B>> bind, Func<A, B, C> project) =>
        Bind(x => bind(x).Map(y => project(x, y)));

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
        Bind(x => f(x) ? Prelude.Right<L, A>(x) : Bottom);

    public Either<L, A> Where(Func<A, bool> f) =>
        Filter(f);

    public Either<L, A> Head =>
        Map(Morphism<A>.head);

    public Either<L, A> Tail =>
        Map(Morphism<A>.tail);

    public Either<L, A> Last =>
        Map(Morphism<A>.last);

    public Either<L, A> Skip(int amount) =>
        Map(Morphism.skip<A>(amount));

    public Either<L, A> Take(int amount) =>
        Map(Morphism.take<A>(amount));

    public static Either<L, A> operator |(Either<L, A> ma, Either<L, A> mb) =>
        Obj.Choice<L, A>(ma, mb);

    public static implicit operator Obj<CoProduct<L, A>>(Either<L, A> ma) =>
        ma.ObjectSafe;

    public static implicit operator Either<L, A>(Obj<CoProduct<L, A>> obj) =>
        obj.ToEither();

    public static implicit operator Either<L, A>(L value) =>
        new(Prim.Pure(CoProduct.Left<L, A>(value)));

    public static implicit operator Either<L, A>(A value) =>
        new(Prim.Pure(CoProduct.Right<L, A>(value)));
}

public static partial class Prelude
{
    public static Either<L, A> ToEither<L, A>(this Obj<CoProduct<L, A>> ma) =>
        new(ma.Interpret(Either<L, A>.NilState));
    
    public static Either<L, A> Right<L, A>(A value) =>
        value;
    
    public static Either<L, A> Left<L, A>(L value) =>
        value;

    public static Either<L, A> liftEither<L, A>(IObservable<A> ma) =>
        new(Prim.Observable(ma.Select(x => Prim.Pure(CoProduct.Right<L, A>(x)))));

    public static Either<L, A> liftEither<L, A>(IEnumerable<A> ma) =>
        new(Prim.Many(ma.Map(x => Prim.Pure(CoProduct.Right<L, A>(x))).ToSeq()));

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

    public static Either<L, A> use<L, A>(Either<L, A> ma) where A : IDisposable =>
        ma.Map(Morphism.use<A>());

    public static Either<L, A> use<L, A>(Either<L, A> ma, Func<A, Unit> release) =>
        ma.Map(Morphism.use(Morphism.function(release)));

    public static Either<L, Unit> release<L, A>(Either<L, A> ma) =>
        ma.Map(Morphism<A>.release);

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
