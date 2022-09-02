using System;
using LanguageExt.Common;
using LanguageExt.DSL.Transducers;

namespace LanguageExt.DSL;

public readonly record struct Eff<RT, A>(Transducer<RT, CoProduct<Error, A>> Morphism) : IsTransducer<RT, CoProduct<Error, A>>
{
    public static readonly Eff<RT, A> Bottom = new(Transducer.constant<RT, CoProduct<Error, A>>(CoProduct.Fail<Error, A>(Errors.Bottom)));
    
    internal Transducer<RT, CoProduct<Error, A>> Op => Morphism ?? Bottom.Morphism;
    
    public Transducer<RT, CoProduct<Error, A>> ToTransducer() => 
        Morphism;

    // -----------------------------------------------------------------------------------------------------------------
    // Map

    public Eff<RT, B> Map<B>(Func<A, B> f) =>
        new(Transducer.kleisli(Op, BiTransducer.mapRight<Error, A, B>(f)));

    public Eff<RT, B> Map<B>(Transducer<A, B> f) =>
        new(Transducer.kleisli(Op, BiTransducer.mapRight<Error, A, B>(f)));

    // -----------------------------------------------------------------------------------------------------------------
    // BiMap

    public Eff<RT, B> BiMap<B>(Func<Error, Error> Left, Func<A, B> Right) =>
        new(Transducer.kleisli(Op, BiTransducer.bimap(Left, Right)));

    public Eff<RT, B> BiMap<B>(Transducer<Error, Error> Left, Transducer<A, B> Right) =>
        new(Transducer.kleisli(Op, BiTransducer.bimap(Left, Right)));

    // -----------------------------------------------------------------------------------------------------------------
    // Bind

    public Eff<RT, B> Bind<B>(Func<A, Eff<RT, B>> f) =>
        new(Transducer.bind<RT, Error, A, B, Eff<RT, B>>(Op, f));

    public Eff<RT, B> Bind<B>(Func<A, CoProduct<Error, B>> f) =>
        new(Transducer.kleisli(Op, f));

    public Eff<RT, B> Bind<B>(Func<A, Transducer<RT, CoProduct<Error, B>>> f) =>
        new(Transducer.kleisli(Op, f));
    
    public Eff<RT, B> Bind<B>(Transducer<A, Transducer<RT, CoProduct<Error, B>>> f) =>
        new(Transducer.kleisli(Op, f));
    
    // -----------------------------------------------------------------------------------------------------------------
    // BiBind

    /*public Eff<RT, B> BiBind<B>(Func<Error, Eff<RT,  B>> Left, Func<A, Eff<RT, B>> Right) =>
        new(Morphism.bikleisli<Eff<RT, B>, RT, Error, Error, A, B>(OpSafe, Left, Right));
    
    public Eff<RT, B> BiBind<B>(Func<Error, CoProduct<Error, B>> Left, Func<A, CoProduct<Error, B>> Right) =>
        new(Transducer.kleisli(Op, Left, Right));

    public Eff<RT, B> BiBind<B>(Morphism<Error, CoProduct<Error, B>> Left, Morphism<A, CoProduct<Error, B>> Right) =>
        new(Morphism.bikleisli(OpSafe, Left, Right));

    public Eff<RT, B> BiBind<B>(
        Morphism<Error, Morphism<RT, CoProduct<Error, B>>> Left,
        Morphism<A, Morphism<RT, CoProduct<Error, B>>> Right) =>
            new(Morphism.bikleisli(OpSafe, Left, Right));*/

    // -----------------------------------------------------------------------------------------------------------------
    // Select

    public Eff<RT, B> Select<B>(Func<A, B> f) =>
        Map(f);

    public Eff<RT, B> Select<B>(Transducer<A, B> f) =>
        Map(f);

    // -----------------------------------------------------------------------------------------------------------------
    // SelectMany
    
    public Eff<RT, B> SelectMany<B>(Func<A, Eff<RT, B>> f) =>
        new(Transducer.bind<RT, Error, A, B, Eff<RT, B>>(Op, f));

    public Eff<RT, B> SelectMany<B>(Func<A, CoProduct<Error, B>> f) =>
        new(Transducer.kleisli(Op, f));

    public Eff<RT, B> SelectMany<B>(Func<A, Transducer<RT, CoProduct<Error, B>>> f) =>
        new(Transducer.kleisli(Op, f));
    
    public Eff<RT, B> SelectMany<B>(Transducer<A, Transducer<RT, CoProduct<Error, B>>> f) =>
        new(Transducer.kleisli(Op, f));

    // -----------------------------------------------------------------------------------------------------------------
    // SelectMany

    public Eff<RT, C> SelectMany<B, C>(Func<A, Eff<RT, B>> f, Func<A, B, C> project) =>
        Bind(a => f(a).Map(b => project(a, b)));

    /*
    public Eff<RT, C> SelectMany<B, C>(Func<A, CoProduct<Error, B>> f, Func<A, B, C> project) =>
        Bind(a => f(a).Map(b => project(a, b)));

    public Eff<RT, C> SelectMany<B, C>(Func<A, Transducer<RT, CoProduct<Error, B>>> f, Func<A, Func<B, C>> project) =>
        Bind(a => f(a).Map(b => project(a, b)));
    
    public Eff<RT, C> SelectMany<B, C>(Transducer<A, Transducer<RT, CoProduct<Error, B>>> f, Func<A, Func<B, C>> project) =>
        Bind(a => f(a).Map(b => project(a, b)));
        */

    // -----------------------------------------------------------------------------------------------------------------
    // Filtering

    public Eff<RT, A> Filter(Func<A, bool> f) =>
        Map(Transducer.filter(f));

    public Eff<RT, A> Where(Func<A, bool> f) =>
        Map(Transducer.filter(f));

    // -----------------------------------------------------------------------------------------------------------------
    // Many item processing
    
    public Eff<RT, A> Head =>
        Map(Transducer<A>.head);

    public Eff<RT, A> Tail =>
        Map(Transducer<A>.tail);

    public Eff<RT, A> Skip(int amount) =>
        Map(Transducer.skip<A>(amount));

    public Eff<RT, A> SkipWhile(Func<A, bool> predicate) =>
        Map(Transducer.skipWhile(predicate));

    public Eff<RT, A> SkipUntil(Func<A, bool> predicate) =>
        Map(Transducer.skipUntil(predicate));

    public Eff<RT, A> Take(int amount) =>
        Map(Transducer.take<A>(amount));

    public Eff<RT, A> TakeWhile(Func<A, bool> predicate) =>
        Map(Transducer.takeWhile(predicate));

    public Eff<RT, A> TakeUntil(Func<A, bool> predicate) =>
        Map(Transducer.takeUntil(predicate));
    
    // -----------------------------------------------------------------------------------------------------------------
    // Run

    public Fin<A> Run(RT runtime)
    {
        return Go(Op.Apply(runtime));

        Fin<A> Go(Prim<CoProduct<Error, A>> ma) =>
            ma switch
            {
                PurePrim<CoProduct<Error, A>> {Value: CoProductRight<Error, A> r} => Fin<A>.Succ(r.Value),
                PurePrim<CoProduct<Error, A>> {Value: CoProductLeft<Error, A> l} => Fin<A>.Fail(l.Value),
                PurePrim<CoProduct<Error, A>> {Value: CoProductFail<Error, A> f} => Fin<A>.Fail(f.Value),
                ManyPrim<CoProduct<Error, A>> m => m.IsEmpty ? Errors.Bottom : Go(m.Head),
                FailPrim<CoProduct<Error, A>> f => Fin<A>.Fail(f.Value),
                _ => throw new NotSupportedException()
            };
    }
    
    public Fin<Seq<A>> RunMany(RT runtime)
    {
        return Go(Op.Apply(runtime));

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

    /*
    public Eff<RT, A> Repeat(Schedule schedule) =>
        Op.Repeat(schedule);

    public Eff<RT, A> RepeatWhile(Schedule schedule, Func<A, bool> pred) =>
        Op.RepeatWhile(schedule, pred);

    public Eff<RT, A> RepeatUntil(Schedule schedule, Func<A, bool> pred) =>
        Op.RepeatUntil(schedule, pred);
        */
    
    // -----------------------------------------------------------------------------------------------------------------
    // Retry

    /*
    public Eff<RT, A> Retry(Schedule schedule) =>
        Op.Retry(schedule);

    public Eff<RT, A> RetryWhile(Schedule schedule, Func<Error, bool> pred) =>
        Op.RetryWhile(schedule, pred);

    public Eff<RT, A> RetryUntil(Schedule schedule, Func<Error, bool> pred) =>
        Op.RetryUntil(schedule, pred);
        */
    
    // -----------------------------------------------------------------------------------------------------------------
    // Folding

    /*
    public Eff<RT, S> Fold<S>(Schedule schedule, S state, Func<S, A, S> fold)=>
        Op.Fold(schedule, state, fold);

    public Eff<RT, S> FoldWhile<S>(Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)=>
        Op.FoldWhile(schedule, state, fold, pred);

    public Eff<RT, S> FoldUntil<S>(Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)=>
        Op.FoldWhile(schedule, state, fold, pred);
        */

    // -----------------------------------------------------------------------------------------------------------------
    // Operators
    
    //public static Eff<RT, A> operator |(Eff<RT, A> ma, Eff<RT, A> mb) =>
    //    Transducer.map<RT, CoProduct<Error, A>>(rt => Obj.Choice(ma.Op.Apply(rt), mb.Op.Apply(rt)));
    
    public static implicit operator Eff<RT, A>(CoProduct<Error, A> obj) =>
        obj.ToEff<RT, A>();

    public static implicit operator Eff<RT, A>(Error value) =>
        new(Transducer.constant<RT, CoProduct<Error, A>>(CoProduct.Left<Error, A>(value)));

    public static implicit operator Eff<RT, A>(A value) =>
        new(Transducer.constant<RT, CoProduct<Error, A>>(CoProduct.Right<Error, A>(value)));
}
