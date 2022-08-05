using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using LanguageExt.ClassInstances;
using LanguageExt.Common;

namespace LanguageExt.Core.DSL;

using static DSL<MError, Error>;

public record Eff<RT, A>(Morphism<RT, A> Op) : Morphism<RT, A>
{
    public static Eff<RT, A> operator |(Eff<RT, A> ma, Eff<RT, A> mb) =>
        new(Morphism.map<RT, A>(rt => Obj.Choice(ma.Op.Apply(rt), mb.Op.Apply(rt))));

    protected override Prim<A> InvokeProtected<RT1>(State<RT1> state, Prim<RT> value) =>
        value.Bind(nrt => Op.Invoke(state.LocalRuntime(_ => nrt), Prim.Pure(nrt)));
    
    public Result<Error, A> Run(RT runtime)
    {
        var state = State<RT>.Create(runtime);
        return Op.Invoke(state, Prim.Pure(runtime)).ToResult();
    }

    public Eff<RT, B> Map<B>(Func<A, B> f) =>
        new(Morphism.compose(Op, Morphism.function(f)));

    public Eff<RT, B> Bind<B>(Func<A, Eff<RT, B>> f) =>
        new(Morphism.bind(Op, f));
    
    public Eff<RT, B> SelectMany<B>(Func<A, Eff<RT, B>> f) =>
        new(Morphism.bind(Op, f));
    
    public Eff<RT, C> SelectMany<B, C>(Func<A, Eff<RT, B>> bind, Func<A, B, C> project) =>
        new(Morphism.bind(Op, bind, project));

    public Eff<RT, A> Filter(Func<A, bool> f) =>
        new(Morphism.lambda<RT, A>(Morphism.filter(f).Apply(Op.Apply(Obj<RT>.This))));

    public Eff<RT, A> Where(Func<A, bool> f) =>
        new(Morphism.lambda<RT, A>(Morphism.filter(f).Apply(Op.Apply(Obj<RT>.This))));

    public Eff<RT, A> Head =>
        new(Morphism.compose(Op, Morphism<A>.head));
    
    public Eff<RT, A> Tail => 
        new(Morphism.compose(Op, Morphism<A>.tail));
    
    public Eff<RT, A> Last => 
        new(Morphism.compose(Op, Morphism<A>.last));
    
    public Eff<RT, A> Skip(int amount)=> 
        new(Morphism.compose(Op, Morphism.skip<A>(amount)));
    
    public Eff<RT, A> Take(int amount)=> 
        new(Morphism.compose(Op, Morphism.take<A>(amount)));
}

public static class PreludeExample
{
    public static Eff<RT, RT> runtime<RT>() =>
        new (Morphism<RT>.identity);

    public static Eff<OuterRT, A> localEff<OuterRT, InnerRT, A>(Func<OuterRT, InnerRT> f, Eff<InnerRT, A> ma) =>
        new(Morphism.lambda<OuterRT, A>(ma.Apply(Morphism.function(f).Apply(Obj<OuterRT>.This))));
    
    public static Eff<RT, A> SuccessEff<RT, A>(A value) =>
        new (Morphism.constant<RT, A>(Obj.Pure(value)));
    
    public static Eff<RT, A> FailEff<RT, A>(Error value) =>
        new (Morphism.constant<RT, A>(Obj.Left<A>(value)));

    public static Eff<RT, A> EffMaybe<RT, A>(Func<RT, Fin<A>> f) =>
        new(Morphism.bind<RT, A>(rt => f(rt).Match(Succ: Obj.Pure, Fail: Obj.Left<A>)));

    public static Eff<RT, A> Eff<RT, A>(Func<RT, A> f) =>
        new(Morphism.function(f));

    public static Eff<RT, A> liftEff<RT, A>(IObservable<A> ma) =>
        new(Morphism.constant<RT, A>(Prim.Observable(ma.Select(Prim.Pure))));

    public static Eff<RT, A> liftEff<RT, A>(IEnumerable<A> ma) =>
        new(Morphism.constant<RT, A>(Prim.Many(ma.Map(Prim.Pure).ToSeq())));
    
    public static Eff<RT, B> Apply<RT, A, B>(this Eff<RT, Func<A, B>> ff, Eff<RT, A> fa) =>
        new(Morphism.lambda<RT, B>(Morphism.function(ff.Op.Apply(Obj<RT>.This)).Apply(fa.Op.Apply(Obj<RT>.This))));

    public static Eff<RT, Func<B, C>> Apply<RT, A, B, C>(this Eff<RT, Func<A, B, C>> ff, Eff<RT, A> fa) =>
        ff.Map(Prelude.curry).Apply(fa);

    public static Eff<RT, Func<B, Func<C, D>>> Apply<RT, A, B, C, D>(this Eff<RT, Func<A, B, C, D>> ff, Eff<RT, A> fa) =>
        ff.Map(Prelude.curry).Apply(fa);

    public static Eff<RT, Func<B, Func<C, Func<D, E>>>> Apply<RT, A, B, C, D, E>(this Eff<RT, Func<A, B, C, D, E>> ff, Eff<RT, A> fa) =>
        ff.Map(Prelude.curry).Apply(fa);

    public static Eff<RT, Func<B, Func<C, Func<D, Func<E, F>>>>> Apply<RT, A, B, C, D, E, F>(this Eff<RT, Func<A, B, C, D, E, F>> ff, Eff<RT, A> fa) =>
        ff.Map(Prelude.curry).Apply(fa);

    public static Eff<RT, A> use<RT, A>(Eff<RT, A> ma) where A : IDisposable =>
        new(Morphism.lambda<RT, A>(Obj.Use(ma.Op.Apply(Obj<RT>.This))));

    public static Eff<RT, A> use<RT, A>(Eff<RT, A> ma, Func<A, Unit> release) where A : IDisposable =>
        new(Morphism.lambda<RT, A>(Obj.Use(ma.Op.Apply(Obj<RT>.This), Morphism.function(release))));

    public static Eff<RT, A> use<RT, A>(Eff<RT, A> ma, Func<A, Eff<RT, Unit>> release) where A : IDisposable =>
        new(Morphism.map<RT, A>(rt => Obj.Use(ma.Op.Apply(rt), Morphism.bind<A, Unit>(x => release(x).Op.Apply(rt)))));
}
