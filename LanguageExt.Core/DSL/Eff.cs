using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using LanguageExt.Common;

namespace LanguageExt.DSL;

public readonly struct MorphismEff<RT, A> : IsMorphism<Eff<RT, A>, RT, CoProduct<Error, A>>
{
    public Morphism<RT, CoProduct<Error, A>> ToMorphism(Eff<RT, A> value) => 
        value.Op;
}

public record Eff<RT, A>(Morphism<RT, CoProduct<Error, A>> Op)
{
    public Fin<A> Run(RT runtime)
    {
        try
        {
            var state = State<RT>.Create(runtime);
            return Op.Invoke(state, Prim.Pure(runtime)).ToFin();
        }
        catch (Exception e)
        {
            return (Error)e;
        }
    }
    
    // -----------------------------------------------------------------------------------------------------------------
    // Map

    public Eff<RT, B> Map<B>(Func<A, B> f) =>
        Map(Morphism.function(f));

    public Eff<RT, B> Map<B>(Morphism<A, B> f) =>
        BiMap(Morphism<Error>.identity, f);

    // -----------------------------------------------------------------------------------------------------------------
    // Bi-map

    public Eff<RT, B> BiMap<B>(Func<Error, Error> Fail, Func<A, B> Succ) =>
        BiMap(Morphism.function(Fail), Morphism.function(Succ));

    public Eff<RT, B> BiMap<B>(Morphism<Error, Error> Fail, Morphism<A, B> Succ) =>
        new(Morphism.compose(Op, BiMorphism.bimap(Fail, Succ)));

    // -----------------------------------------------------------------------------------------------------------------
    // Bind

    //Morphism<RT, CoProduct<Error, A>>

    public Eff<RT, B> Bind<B>(Morphism<A, Morphism<RT, CoProduct<Error, B>>> f) =>
        new(Morphism.kleisli(Op, f));
    
    public Eff<RT, B> Bind<B>(Morphism<A, Eff<RT, B>> f) =>
        new(Morphism.kleisli<MorphismEff<RT, B>, Eff<RT, B>, RT, Error, A, B>(Op, f));

    public Eff<RT, B> Bind<B>(Func<A, Eff<RT, B>> f) =>
        new(Morphism.kleisli<MorphismEff<RT, B>, Eff<RT, B>, RT, Error, A, B>(Op, Morphism.function(f)));
    
    public Eff<RT, B> SelectMany<B>(Func<A, Eff<RT, B>> f) =>
        Bind(f);
    
    public Eff<RT, C> SelectMany<B, C>(Func<A, Eff<RT, B>> bind, Func<A, B, C> project) =>
        Bind(x => bind(x).Map(y => project(x, y)));

    
    
    /*public Eff<RT, B> Bind<B>(Func<A, IEnumerable<B>> f) =>
        Bind(a => Prelude.liftEff<RT, B>(f(a)));
    
    public Eff<RT, B> SelectMany<B>(Func<A, IEnumerable<B>> f) =>
        Bind(f);

    public Eff<RT, C> SelectMany<B, C>(Func<A, IEnumerable<B>> bind, Func<A, B, C> project) =>
        Bind(x => bind(x).Map(y => project(x, y)));
 
    public Eff<RT, B> Bind<B>(Func<A, IObservable<B>> f) =>
        Bind(a => Prelude.liftEff<RT, B>(f(a)));

    public Eff<RT, B> SelectMany<B>(Func<A, IObservable<B>> f) =>
        Bind(f);

    public Eff<RT, C> SelectMany<B, C>(Func<A, IObservable<B>> bind, Func<A, B, C> project) =>
        Bind(x => bind(x).Select(y => project(x, y)));*/

    public Eff<RT, A> Filter(Func<A, bool> f) =>
        Bind(x => f(x) ? Prelude.SuccessEff<RT, A>(x) : Prelude.FailEff<RT, A>("bottom"));

    public Eff<RT, A> Where(Func<A, bool> f) =>
        Filter(f);

    public Eff<RT, A> Head =>
        Map(Morphism<A>.head);
    
    public Eff<RT, A> Tail => 
        Map(Morphism<A>.tail);
    
    public Eff<RT, A> Last => 
        Map(Morphism<A>.last);
    
    public Eff<RT, A> Skip(int amount)=> 
        Map(Morphism.skip<A>(amount));
    
    public Eff<RT, A> Take(int amount)=> 
        Map(Morphism.take<A>(amount));
    
    public static Eff<RT, A> operator |(Eff<RT, A> ma, Eff<RT, A> mb) =>
        new(Morphism.map<RT, CoProduct<Error, A>>(rt => Obj.Choice(ma.Op.Apply(rt), mb.Op.Apply(rt))));
}

public static partial class Prelude
{
    public static Eff<RT, RT> runtime<RT>() =>
        new(Morphism.function<RT, CoProduct<Error, RT>>(CoProduct.Right<Error, RT>));

    public static Eff<OuterRT, A> localEff<OuterRT, InnerRT, A>(Func<OuterRT, InnerRT> f, Eff<InnerRT, A> ma) =>
        new(Morphism.bind<OuterRT, CoProduct<Error, A>>(ort => ma.Op.Apply(Prim.Pure(f(ort)))));

    public static Eff<RT, A> SuccessEff<RT, A>(A value) =>
        new(Morphism.constant<RT, CoProduct<Error, A>>(Prim.Pure(CoProduct.Right<Error, A>(value))));
    
    public static Eff<RT, A> FailEff<RT, A>(Error value) =>
        new(Morphism.constant<RT, CoProduct<Error, A>>(Prim.Pure(CoProduct.Left<Error, A>(value))));

    public static Eff<RT, A> EffMaybe<RT, A>(Func<RT, Fin<A>> f) =>
        new(Morphism.bind<RT, CoProduct<Error, A>>(rt => f(rt)
            .Match(Succ: x => Obj.Pure(CoProduct.Right<Error, A>(x)),
                   Fail: x => Obj.Pure(CoProduct.Left<Error, A>(x)))));

    public static Eff<RT, A> Eff<RT, A>(Func<RT, A> f) =>
        new(Morphism.bind<RT, CoProduct<Error, A>>(rt => Prim.Pure(CoProduct.Right<Error, A>(f(rt)))));
    
    /*
    public static Eff<RT, A> liftEff<RT, A>(IObservable<A> ma) =>
        new(Morphism.constant<RT, A>(Prim.Observable(ma.Select(Prim.Pure))));

    public static Eff<RT, A> liftEff<RT, A>(IEnumerable<A> ma) =>
        new(Morphism.constant<RT, A>(Prim.Many(ma.Map(Prim.Pure).ToSeq())));
    
    public static Eff<RT, B> Apply<RT, A, B>(this Eff<RT, Func<A, B>> ff, Eff<RT, A> fa) =>
        new(Morphism.lambda<RT, B>(Morphism.function(ff.Op.Apply(Obj<RT>.This)).Apply(fa.Op.Apply(Obj<RT>.This))));

    public static Eff<RT, Func<B, C>> Apply<RT, A, B, C>(this Eff<RT, Func<A, B, C>> ff, Eff<RT, A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Eff<RT, Func<B, Func<C, D>>> Apply<RT, A, B, C, D>(this Eff<RT, Func<A, B, C, D>> ff, Eff<RT, A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Eff<RT, Func<B, Func<C, Func<D, E>>>> Apply<RT, A, B, C, D, E>(this Eff<RT, Func<A, B, C, D, E>> ff, Eff<RT, A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Eff<RT, Func<B, Func<C, Func<D, Func<E, F>>>>> Apply<RT, A, B, C, D, E, F>(this Eff<RT, Func<A, B, C, D, E, F>> ff, Eff<RT, A> fa) =>
        ff.Map(LanguageExt.Prelude.curry).Apply(fa);

    public static Eff<RT, A> use<RT, A>(Eff<RT, A> ma) where A : IDisposable =>
        new(Morphism.lambda<RT, A>(Obj.Use(ma.Op.Apply(Obj<RT>.This))));

    public static Eff<RT, A> use<RT, A>(Eff<RT, A> ma, Func<A, Unit> release) =>
        new(Morphism.lambda<RT, A>(Obj.Use(ma.Op.Apply(Obj<RT>.This), Morphism.function(release))));

    public static Eff<RT, A> use<RT, A>(Eff<RT, A> ma, Func<A, Eff<RT, Unit>> release)  =>
        new(Morphism.map<RT, A>(rt => Obj.Use(ma.Op.Apply(rt), Morphism.bind<A, Unit>(x => release(x).Op.Apply(rt)))));

    public static Eff<RT, Unit> release<RT, A>(Eff<RT, A> ma) =>
        new(Morphism.lambda<RT, Unit>(Obj.Release(ma.Op.Apply(Obj<RT>.This))));

    public static Eff<RT, B> Bind<RT, A, B>(this IEnumerable<A> ma, Func<A, Eff<RT, B>> f) =>
        liftEff<RT, A>(ma).Bind(f);

    public static Eff<RT, B> SelectMany<RT, A, B>(this IEnumerable<A> ma, Func<A, Eff<RT, B>> f) =>
        liftEff<RT, A>(ma).Bind(f);

    public static Eff<RT, C> SelectMany<RT, A, B, C>(this IEnumerable<A> ma, Func<A, Eff<RT, B>> bind, Func<A, B, C> project) =>
        liftEff<RT, A>(ma).SelectMany(bind, project);

    public static Eff<RT, B> Bind<RT, A, B>(this IObservable<A> ma, Func<A, Eff<RT, B>> f) =>
        liftEff<RT, A>(ma).Bind(f);

    public static Eff<RT, B> SelectMany<RT, A, B>(this IObservable<A> ma, Func<A, Eff<RT, B>> f) =>
        liftEff<RT, A>(ma).Bind(f);

    public static Eff<RT, C> SelectMany<RT, A, B, C>(this IObservable<A> ma, Func<A, Eff<RT, B>> bind, Func<A, B, C> project) =>
        liftEff<RT, A>(ma).SelectMany(bind, project);
                */

}
