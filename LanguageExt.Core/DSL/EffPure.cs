using System;
using LanguageExt.Common;
using LanguageExt.DSL.Transducers;
using static LanguageExt.DSL.Transducers.Transducer;

namespace LanguageExt.DSL;

public readonly record struct DefaultRuntime;

public readonly record struct Eff<A>(Transducer<DefaultRuntime, CoProduct<Error, A>> MorphismUnsafe) : 
    IsTransducer<DefaultRuntime, CoProduct<Error, A>>
{
    public static readonly Eff<A> Bottom = 
        new(constant<DefaultRuntime, CoProduct<Error, A>>(CoProduct.Fail<Error, A>(Errors.Bottom)));
    
    public Transducer<DefaultRuntime, CoProduct<Error, A>> Morphism => 
        MorphismUnsafe ?? Bottom.Morphism;
    
    public Transducer<DefaultRuntime, CoProduct<Error, A>> ToTransducer() => 
        Morphism;

    // -----------------------------------------------------------------------------------------------------------------
    // Map

    public Eff<B> Map<B>(Func<A, B> f) =>
        new(compose(Morphism, mapRight<Error, A, B>(f)));

    public Eff<B> Map<B>(Transducer<A, B> f) =>
        new(compose(Morphism, mapRight<Error, A, B>(f)));

    // -----------------------------------------------------------------------------------------------------------------
    // BiMap

    public Eff<B> BiMap<B>(Func<Error, Error> Left, Func<A, B> Right) =>
        new(compose(Morphism, bimap(Left, Right)));

    public Eff<B> BiMap<B>(Transducer<Error, Error> Left, Transducer<A, B> Right) =>
        new(compose(Morphism, bimap(Left, Right)));

    // -----------------------------------------------------------------------------------------------------------------
    // Bind

    public Eff<B> Bind<B>(Func<A, Eff<B>> f) =>
        new(bind<DefaultRuntime, Error, A, B, Eff<B>>(Morphism, f));

    public Eff<B> Bind<B>(Func<A, CoProduct<Error, B>> f) =>
        new(bind(Morphism, f));

    public Eff<B> Bind<B>(Func<A, Transducer<DefaultRuntime, CoProduct<Error, B>>> f) =>
        new(bind(Morphism, f));
    
    public Eff<B> Bind<B>(Transducer<A, Transducer<DefaultRuntime, CoProduct<Error, B>>> f) =>
        new(bind(Morphism, f));

    public Eff<B> Bind<B>(Transducer<Unit, B> f) =>
        new(bind(Morphism, f));

    public Eff<B> Bind<B>(Func<A, Transducer<Unit, B>> f) =>
        new(bindProduce(Morphism, f));

    // -----------------------------------------------------------------------------------------------------------------
    // Select

    public Eff<B> Select<B>(Func<A, B> f) =>
        Map(f);

    public Eff<B> Select<B>(Transducer<A, B> f) =>
        Map(f);

    // -----------------------------------------------------------------------------------------------------------------
    // SelectMany

    public Eff<B> SelectMany<B>(Func<A, Eff<B>> f) =>
        Bind(f);

    public Eff<B> SelectMany<B>(Func<A, CoProduct<Error, B>> f) =>
        Bind(f);

    public Eff<B> SelectMany<B>(Func<A, Transducer<DefaultRuntime, CoProduct<Error, B>>> f) =>
        Bind(f);

    public Eff<B> SelectMany<B>(Transducer<A, Transducer<DefaultRuntime, CoProduct<Error, B>>> f) =>
        Bind(f);

    public Eff<B> SelectMany<B>(Transducer<Unit, B> f) =>
        Bind(f);

    public Eff<B> SelectMany<B>(Func<A, Transducer<Unit, B>> f) =>
        Bind(f);

    // -----------------------------------------------------------------------------------------------------------------
    // SelectMany

    public Eff<C> SelectMany<B, C>(Func<A, Eff<B>> f, Func<A, B, C> project) =>
        Bind(a => f(a).Map(b => project(a, b)));

    public Eff<C> SelectMany<B, C>(Func<A, Transducer<Unit, B>> f, Func<A, B, C> project) =>
        new(bindMap(Morphism, f, project));

    // -----------------------------------------------------------------------------------------------------------------
    // Filtering

    public Eff<A> Filter(Func<A, bool> f) =>
        Map(filter(f));

    public Eff<A> Where(Func<A, bool> f) =>
        Map(filter(f));

    // -----------------------------------------------------------------------------------------------------------------
    // Many item processing
    
    public Eff<A> Head =>
        Map(Transducer<A>.head);

    public Eff<A> Tail =>
        Map(Transducer<A>.tail);

    public Eff<A> Skip(int amount) =>
        Map(skip<A>(amount));

    public Eff<A> SkipWhile(Func<A, bool> predicate) =>
        Map(skipWhile(predicate));

    public Eff<A> SkipUntil(Func<A, bool> predicate) =>
        Map(skipUntil(predicate));

    public Eff<A> Take(int amount) =>
        Map(take<A>(amount));

    public Eff<A> TakeWhile(Func<A, bool> predicate) =>
        Map(takeWhile(predicate));

    public Eff<A> TakeUntil(Func<A, bool> predicate) =>
        Map(takeUntil(predicate));
    
    // -----------------------------------------------------------------------------------------------------------------
    // Run

    public Fin<A> Run(DefaultRuntime runtime = default)
    {
        return Go(Morphism.Apply1(runtime));

        Fin<A> Go(Prim<CoProduct<Error, A>> ma) =>
            ma switch
            {
                PurePrim<CoProduct<Error, A>> {Value: CoProductRight<Error, A> r} => Fin<A>.Succ(r.Value),
                PurePrim<CoProduct<Error, A>> {Value: CoProductLeft<Error, A> l} => Fin<A>.Fail(l.Value),
                PurePrim<CoProduct<Error, A>> {Value: CoProductFail<Error, A> f} => Fin<A>.Fail(f.Value),
                ManyPrim<CoProduct<Error, A>> m => Go(m.Items.Head),
                FailPrim<CoProduct<Error, A>> f => Fin<A>.Fail(f.Value),
                _ => throw new NotSupportedException()
            };
    }
    
    public Fin<Seq<A>> RunMany(DefaultRuntime runtime = default)
    {
        return Go(Morphism.Apply(runtime));

        Fin<Seq<A>> Go(Prim<CoProduct<Error, A>> ma) =>
            ma switch
            {
                PurePrim<CoProduct<Error, A>> {Value: CoProductRight<Error, A> r} => Fin<Seq<A>>.Succ(LanguageExt.Prelude.Seq1(r.Value)),
                PurePrim<CoProduct<Error, A>> {Value: CoProductLeft<Error, A> l} => Fin<Seq<A>>.Fail(l.Value),
                PurePrim<CoProduct<Error, A>> {Value: CoProductFail<Error, A> f} => Fin<Seq<A>>.Fail(f.Value),
                ManyPrim<CoProduct<Error, A>> m => m.Items.Sequence(Go).Map(static x => x.Flatten()),
                FailPrim<CoProduct<Error, A>> f => Fin<Seq<A>>.Fail(f.Value),
                _ => throw new NotSupportedException()
            };
    }
    
    // -----------------------------------------------------------------------------------------------------------------
    // Repeat

    
    public Eff<A> Repeat(Schedule schedule) =>
        new(Morphism.Repeat(schedule));

    public Eff<A> RepeatWhile(Schedule schedule, Func<A, bool> pred) =>
        new(Morphism.RepeatWhile(schedule, pred));

    public Eff<A> RepeatUntil(Schedule schedule, Func<A, bool> pred) =>
        new(Morphism.RepeatUntil(schedule, pred));
    
    // -----------------------------------------------------------------------------------------------------------------
    // Retry

    public Eff<A> Retry(Schedule schedule) =>
        new(Morphism.Retry(schedule));

    public Eff<A> RetryWhile(Schedule schedule, Func<Error, bool> pred) =>
        new(Morphism.RetryWhile(schedule, pred));

    public Eff<A> RetryUntil(Schedule schedule, Func<Error, bool> pred) =>
        new(Morphism.RetryUntil(schedule, pred));
    
    // -----------------------------------------------------------------------------------------------------------------
    // Folding

    public Eff<S> Fold<S>(Schedule schedule, S state, Func<S, A, S> fold)=>
        new(Morphism.Fold(schedule, state, fold));

    public Eff<S> FoldWhile<S>(Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)=>
        new(Morphism.FoldWhile(schedule, state, fold, pred));

    public Eff<S> FoldUntil<S>(Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)=>
        new(Morphism.FoldUntil(schedule, state, fold, pred));

    // -----------------------------------------------------------------------------------------------------------------
    // Operators
    
    public static Eff<A> operator |(Eff<A> ma, Eff<A> mb) =>
        new(choice(ma.Morphism, mb.Morphism));
    
    public static implicit operator Eff<A>(CoProduct<Error, A> obj) =>
        obj.ToEff();

    public static implicit operator Eff<A>(Error value) =>
        new(constantLeft<DefaultRuntime, Error, A>(value));

    public static implicit operator Eff<A>(A value) =>
        new(constantRight<DefaultRuntime, Error, A>(value));
}
