using System;
using LanguageExt.Common;

namespace LanguageExt.DSL;

public readonly record struct Eff<RT, A>(Morphism<RT, CoProduct<Error, A>> Op) : IsMorphism<RT, CoProduct<Error, A>>
{
    public static readonly Eff<RT, A> Bottom = new(Morphism.constant<RT, CoProduct<Error, A>>(Prim<CoProduct<Error, A>>.None));
    
    internal Morphism<RT, CoProduct<Error, A>> OpSafe => Op ?? Bottom.Op;

    public Morphism<RT, CoProduct<Error, A>> ToMorphism() => 
        OpSafe;

    // -----------------------------------------------------------------------------------------------------------------
    // Map

    public Eff<RT, B> Map<B>(Func<A, B> f) =>
        BiMap(static x => x, f);

    public Eff<RT, B> Map<B>(Morphism<A, B> f) =>
        BiMap(Morphism<Error>.identity, f);

    // -----------------------------------------------------------------------------------------------------------------
    // BiMap

    public Eff<RT, B> BiMap<B>(Func<Error, Error> Left, Func<A, B> Right) =>
        Morphism.compose(OpSafe, BiMorphism.bimap(Left, Right));

    public Eff<RT, B> BiMap<B>(Morphism<Error, Error> Left, Morphism<A, B> Right) =>
        Morphism.compose(OpSafe, BiMorphism.bimap(Left, Right));

    // -----------------------------------------------------------------------------------------------------------------
    // Bind

    public Eff<RT, B> Bind<B>(Func<A, Eff<RT, B>> f) =>
        new(Morphism.kleisli<Eff<RT, B>, RT, Error, A, B>(OpSafe, f));

    public Eff<RT, B> Bind<B>(Func<A, CoProduct<Error, B>> f) =>
        new(Morphism.kleisli(OpSafe, f)); 

    public Eff<RT, B> Bind<B>(Func<A, Morphism<RT, CoProduct<Error, B>>> f) =>
        new(Morphism.kleisli(OpSafe, f));
    
    public Eff<RT, B> Bind<B>(Morphism<A, Morphism<RT, CoProduct<Error, B>>> f) =>
        new(Morphism.kleisli(OpSafe, f));
    
    // -----------------------------------------------------------------------------------------------------------------
    // BiBind

    public Eff<RT, B> BiBind<B>(Func<Error, Eff<RT,  B>> Left, Func<A, Eff<RT, B>> Right) =>
        new(Morphism.bikleisli<Eff<RT, B>, RT, Error, Error, A, B>(OpSafe, Left, Right));
    
    public Eff<RT, B> BiBind<B>(Func<Error, CoProduct<Error, B>> Left, Func<A, CoProduct<Error, B>> Right) =>
        new(Morphism.bikleisli(OpSafe, Left, Right));

    public Eff<RT, B> BiBind<B>(Morphism<Error, CoProduct<Error, B>> Left, Morphism<A, CoProduct<Error, B>> Right) =>
        new(Morphism.bikleisli(OpSafe, Left, Right));

    public Eff<RT, B> BiBind<B>(
        Morphism<Error, Morphism<RT, CoProduct<Error, B>>> Left,
        Morphism<A, Morphism<RT, CoProduct<Error, B>>> Right) =>
            new(Morphism.bikleisli(OpSafe, Left, Right));

    // -----------------------------------------------------------------------------------------------------------------
    // Select

    public Eff<RT, B> Select<B>(Func<A, B> f) =>
        Map(f);

    public Eff<RT, B> Select<B>(Morphism<A, B> f) =>
        Map(f);

    // -----------------------------------------------------------------------------------------------------------------
    // SelectMany
    
    public Eff<RT, B> SelectMany<B>(Func<A, Eff<RT, B>> f) =>
        new(Morphism.kleisli<Eff<RT, B>, RT, Error, A, B>(OpSafe, f));

    public Eff<RT, B> SelectMany<B>(Func<A, CoProduct<Error, B>> f) =>
        new(Morphism.kleisli(OpSafe, f)); 

    public Eff<RT, B> SelectMany<B>(Func<A, Morphism<RT, CoProduct<Error, B>>> f) =>
        new(Morphism.kleisli(OpSafe, f));
    
    public Eff<RT, B> SelectMany<B>(Morphism<A, Morphism<RT, CoProduct<Error, B>>> f) =>
        new(Morphism.kleisli(OpSafe, f));

    // -----------------------------------------------------------------------------------------------------------------
    // SelectMany

    public Eff<RT, C> SelectMany<B, C>(Func<A, Eff<RT, B>> f, Func<A, B, C> project) =>
        new(Morphism.kleisliProject(OpSafe, f, project));

    public Eff<RT, C> SelectMany<B, C>(Func<A, CoProduct<Error, B>> f, Func<A, B, C> project) =>
        new(Morphism.kleisliProject(OpSafe, f, project)); 

    public Eff<RT, C> SelectMany<B, C>(Func<A, Morphism<RT, CoProduct<Error, B>>> f, Morphism<A, Morphism<B, C>> project) =>
        new(Morphism.kleisliProject(OpSafe, f, project));
    
    public Eff<RT, C> SelectMany<B, C>(Morphism<A, Morphism<RT, CoProduct<Error, B>>> f, Morphism<A, Morphism<B, C>> project) =>
        new(Morphism.kleisliProject(OpSafe, f, project));
    

    // -----------------------------------------------------------------------------------------------------------------
    // Filtering

    public Eff<RT, A> Filter(Func<A, bool> f) =>
        Op.Filter(x => x is CoProductRight<Error, A> r && f(r.Value));

    public Eff<RT, A> Where(Func<A, bool> f) =>
        Filter(f);

    // -----------------------------------------------------------------------------------------------------------------
    // Many item processing
    
    public Eff<RT, A> Head =>
        Op.Head;

    public Eff<RT, A> Tail =>
        Op.Tail;

    public Eff<RT, A> Last =>
        Op.Last;

    public Eff<RT, A> Skip(int amount) =>
        Op.Skip(amount);

    public Eff<RT, A> Take(int amount) =>
        Op.Take(amount);
    
    // -----------------------------------------------------------------------------------------------------------------
    // Run

    public Fin<A> Run(RT runtime)
    {
        var state = State<RT>.Create(runtime);
        try
        {
            return Go(Op.Invoke(state, Prim.Pure(runtime)));
        }
        catch(Exception e)
        {
            return Fin<A>.Fail(e);
        }
        finally
        {
            state.CleanUp();
        }

        Fin<A> Go(Prim<CoProduct<Error, A>> ma) =>
            ma switch
            {
                PurePrim<CoProduct<Error, A>> {Value: CoProductRight<Error, A> r} => Fin<A>.Succ(r.Value),
                PurePrim<CoProduct<Error, A>> {Value: CoProductLeft<Error, A> l} => Fin<A>.Fail(l.Value),
                PurePrim<CoProduct<Error, A>> {Value: CoProductFail<Error, A> f} => Fin<A>.Fail(f.Value),
                ManyPrim<CoProduct<Error, A>> m => Go(m.Head),
                FailPrim<CoProduct<Error, A>> f => Fin<A>.Fail(f.Value),
                _ => throw new NotSupportedException()
            };
    }
    
    public Fin<Seq<A>> RunMany(RT runtime)
    {
        var state = State<RT>.Create(runtime);
        try
        {
            return Go(Op.Invoke(state, Prim.Pure(runtime)));
        }
        catch(Exception e)
        {
            return Fin<Seq<A>>.Fail(e);
        }
        finally
        {
            state.CleanUp();
        }

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

    public Eff<RT, A> Repeat(Schedule schedule) =>
        Op.Repeat(schedule);

    public Eff<RT, A> RepeatWhile(Schedule schedule, Func<A, bool> pred) =>
        Op.RepeatWhile(schedule, pred);

    public Eff<RT, A> RepeatUntil(Schedule schedule, Func<A, bool> pred) =>
        Op.RepeatUntil(schedule, pred);
    
    // -----------------------------------------------------------------------------------------------------------------
    // Retry

    public Eff<RT, A> Retry(Schedule schedule) =>
        Op.Retry(schedule);

    public Eff<RT, A> RetryWhile(Schedule schedule, Func<Error, bool> pred) =>
        Op.RetryWhile(schedule, pred);

    public Eff<RT, A> RetryUntil(Schedule schedule, Func<Error, bool> pred) =>
        Op.RetryUntil(schedule, pred);
    
    // -----------------------------------------------------------------------------------------------------------------
    // Folding

    public Eff<RT, S> Fold<S>(Schedule schedule, S state, Func<S, A, S> fold)=>
        Op.Fold(schedule, state, fold);

    public Eff<RT, S> FoldWhile<S>(Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)=>
        Op.FoldWhile(schedule, state, fold, pred);

    public Eff<RT, S> FoldUntil<S>(Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)=>
        Op.FoldWhile(schedule, state, fold, pred);

    // -----------------------------------------------------------------------------------------------------------------
    // Operators
    
    public static Eff<RT, A> operator |(Eff<RT, A> ma, Eff<RT, A> mb) =>
        Morphism.map<RT, CoProduct<Error, A>>(rt => Obj.Choice(ma.Op.Apply(rt), mb.Op.Apply(rt)));
    
    public static implicit operator Morphism<RT, CoProduct<Error, A>>(Eff<RT, A> ma) =>
        ma.OpSafe;

    public static implicit operator Eff<RT, A>(Obj<CoProduct<Error, A>> obj) =>
        obj.ToEff<RT, A>();

    public static implicit operator Eff<RT, A>(CoProduct<Error, A> obj) =>
        obj.ToEff<RT, A>();

    public static implicit operator Eff<RT, A>(Morphism<RT, CoProduct<Error, A>> obj) =>
        obj.ToEff();

    public static implicit operator Eff<RT, A>(Error value) =>
        new(Morphism.constant<RT, CoProduct<Error, A>>(Prim.Pure(CoProduct.Left<Error, A>(value))));

    public static implicit operator Eff<RT, A>(A value) =>
        new(Morphism.constant<RT, CoProduct<Error, A>>(Prim.Pure(CoProduct.Right<Error, A>(value))));

}
