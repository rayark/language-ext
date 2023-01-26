using System;
using LanguageExt.Common;
using static LanguageExt.DSL2.Sum;

namespace LanguageExt.DSL2;

public readonly record struct Eff<RT, A>(SumTransducer<Error, Error, RT, A> MorphismUnsafe) :
    IsSumTransducer<Error, Error, RT, A>
{
    public static readonly Eff<RT, A> Bottom = 
        new(expand(Transducer.constant<Sum<Error, RT>, Sum<Error, A>>(Sum<Error, A>.Left(Errors.Bottom))));
    
    // -----------------------------------------------------------------------------------------------------------------
    // Transducer 

    public SumTransducer<Error, Error, RT, A> Morphism => 
        MorphismUnsafe ?? Bottom.Morphism;
    
    public SumTransducer<Error, Error, RT, A> ToSumTransducer() => 
        Morphism;

    // -----------------------------------------------------------------------------------------------------------------
    // Map

    public Eff<RT, B> Map<B>(Func<A, B> f) =>
        new(Morphism.MapRight(f));

    public Eff<RT, B> Map<B>(Transducer<A, B> f) =>
        new(Morphism.MapRight(f));

    // -----------------------------------------------------------------------------------------------------------------
    // BiMap

    public Eff<RT, B> BiMap<B>(Func<Error, Error> Left, Func<A, B> Right) =>
        new(Morphism.BiMap(Left, Right));

    public Eff<RT, B> BiMap<B>(Transducer<Error, Error> Left, Transducer<A, B> Right) =>
        new(Morphism.BiMap(Left, Right));

    // -----------------------------------------------------------------------------------------------------------------
    // Bind

    public Eff<RT, B> Bind<B>(Func<A, Eff<RT, B>> f) =>
        new(Morphism.Bind<RT, Error, A, B, Eff<RT, B>>(f));

    public Eff<RT, B> Bind<B>(SumTransducer<Error, Error, A, SumTransducer<Error, Error, RT, B>> f) =>
        new(Morphism.Bind(f));

    // -----------------------------------------------------------------------------------------------------------------
    // Select

    public Eff<RT, B> Select<B>(Func<A, B> f) =>
        Map(f);

    public Eff<RT, B> Select<B>(Transducer<A, B> f) =>
        Map(f);

    // -----------------------------------------------------------------------------------------------------------------
    // SelectMany

    public Eff<RT, B> SelectMany<B>(Func<A, Eff<RT, B>> f) =>
        Bind(f);

    public Eff<RT, B> SelectMany<B>(SumTransducer<Error, Error, A, SumTransducer<Error, Error, RT, B>> f) =>
        Bind(f);
    

    // -----------------------------------------------------------------------------------------------------------------
    // SelectMany

    public Eff<RT, C> SelectMany<B, C>(Func<A, Eff<RT, B>> f, Func<A, B, C> project) =>
        Bind(a => f(a).Map(b => project(a, b)));

    // -----------------------------------------------------------------------------------------------------------------
    // Filtering

    public Eff<RT, A> Filter(Func<A, bool> f) =>
        new(Morphism.Filter(f));

    public Eff<RT, A> Where(Func<A, bool> f) =>
        new(Morphism.Filter(f));
    
    // -----------------------------------------------------------------------------------------------------------------
    // Run

    public Fin<A> Run(RT runtime) =>
        Morphism.Invoke1(runtime);
    
    public Fin<Seq<A>> RunMany(RT runtime) =>
        Morphism.InvokeMany(runtime);

    
    // -----------------------------------------------------------------------------------------------------------------
    // Folding

    public Eff<RT, S> Fold<S>(Schedule schedule, S state, Func<S, A, S> fold)=>
        new(Morphism.Fold((a, s) => fold(s, a), state, schedule));

    public Eff<RT, S> FoldWhile<S>(Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)=>
        new(Morphism.FoldWhile((a, s) => fold(s, a), state, pred, schedule));

    public Eff<RT, S> FoldUntil<S>(Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)=>
        new(Morphism.FoldUntil((a, s) => fold(s, a), state, pred, schedule));

    // -----------------------------------------------------------------------------------------------------------------
    // Operators
    
    /*
    public static Eff<RT, A> operator |(Eff<RT, A> ma, Eff<RT, A> mb) =>
        new(choice(ma.Morphism, mb.Morphism));
        */
    
    public static implicit operator Eff<RT, A>(Sum<Error, A> obj) =>
        new(expand(Transducer.constant<Sum<Error, RT>, Sum<Error, A>>(obj)));

    public static implicit operator Eff<RT, A>(Error value) =>
        (Eff<RT, A>)Sum<Error, A>.Left(value);

    public static implicit operator Eff<RT, A>(A value) =>
        (Eff<RT, A>)Sum<Error, A>.Right(value);
    
    
    // -----------------------------------------------------------------------------------------------------------------
    // Repeat

    
    /*
    public Eff<RT, A> Repeat(Schedule schedule) =>
        new(Morphism.Repeat(schedule));

    public Eff<RT, A> RepeatWhile(Schedule schedule, Func<A, bool> pred) =>
        new(Morphism.RepeatWhile(schedule, pred));

    public Eff<RT, A> RepeatUntil(Schedule schedule, Func<A, bool> pred) =>
        new(Morphism.RepeatUntil(schedule, pred));
        */
    
    // -----------------------------------------------------------------------------------------------------------------
    // Retry

    /*
    public Eff<RT, A> Retry(Schedule schedule) =>
        new(Morphism.Retry(schedule));

    public Eff<RT, A> RetryWhile(Schedule schedule, Func<Error, bool> pred) =>
        new(Morphism.RetryWhile(schedule, pred));

    public Eff<RT, A> RetryUntil(Schedule schedule, Func<Error, bool> pred) =>
        new(Morphism.RetryUntil(schedule, pred));
        */
    

    // -----------------------------------------------------------------------------------------------------------------
    // BiBind

    /*
    public Eff<RT, B> BiBind<B>(Func<Error, Eff<RT,  B>> Left, Func<A, Eff<RT, B>> Right) =>
        new(Morphism.bikleisli<Eff<RT, B>, RT, Error, Error, A, B>(OpSafe, Left, Right));
    
    public Eff<RT, B> BiBind<B>(Func<Error, Sum<Error, B>> Left, Func<A, Sum<Error, B>> Right) =>
        new(Transducer.kleisli(Op, Left, Right));

    public Eff<RT, B> BiBind<B>(Morphism<Error, Sum<Error, B>> Left, Morphism<A, Sum<Error, B>> Right) =>
        new(Morphism.bikleisli(OpSafe, Left, Right));

    public Eff<RT, B> BiBind<B>(
        Morphism<Error, Morphism<RT, Sum<Error, B>>> Left,
        Morphism<A, Morphism<RT, Sum<Error, B>>> Right) =>
            new(Morphism.bikleisli(OpSafe, Left, Right));*/
    
    // SelectMany
    
    /*
    public Eff<RT, B> SelectMany<B>(Func<A, Transducer<RT, Sum<Error, B>>> f) =>
        Bind(f);

    public Eff<RT, B> SelectMany<B>(Transducer<A, Transducer<RT, Sum<Error, B>>> f) =>
        Bind(f);

    public Eff<RT, B> SelectMany<B>(Transducer<Unit, B> f) =>
        Bind(f);

    public Eff<RT, B> SelectMany<B>(Func<A, Transducer<Unit, B>> f) =>
        Bind(f);
        
    public Eff<RT, C> SelectMany<B, C>(Func<A, Transducer<Unit, B>> f, Func<A, B, C> project) =>
        new(bindMap(Morphism, f, project));
        */

    // -----------------------------------------------------------------------------------------------------------------
    // Many item processing
    
    /*public Eff<RT, A> Head =>
        Map(Transducer<A>.head);

    public Eff<RT, A> Tail =>
        Map(Transducer<A>.tail);

    public Eff<RT, A> Skip(int amount) =>
        Map(skip<A>(amount));

    public Eff<RT, A> SkipWhile(Func<A, bool> predicate) =>
        Map(skipWhile(predicate));

    public Eff<RT, A> SkipUntil(Func<A, bool> predicate) =>
        Map(skipUntil(predicate));

    public Eff<RT, A> Take(int amount) =>
        Map(take<A>(amount));

    public Eff<RT, A> TakeWhile(Func<A, bool> predicate) =>
        Map(takeWhile(predicate));

    public Eff<RT, A> TakeUntil(Func<A, bool> predicate) =>
        Map(takeUntil(predicate));*/
    
}
