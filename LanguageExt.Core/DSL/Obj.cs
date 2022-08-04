#nullable enable
using System;
using System.Collections.Concurrent;
using LanguageExt.Common;

namespace LanguageExt.Core.DSL;

public static class Obj
{
    public static readonly Obj<Unit> Unit = Pure(Prelude.unit);
    
    public static Obj<A> Pure<A>(A value) =>
        new PureObj<A>(value);
    
    public static Obj<A> Many<A>(Seq<Obj<A>> values) =>
        new ManyObj<A>(values);
    
    public static Obj<A> Fail<A>(Error value) =>
        new LeftObj<Error, A>(value);
    
    public static Obj<R> Left<L, R>(L value) =>
        new LeftObj<L, R>(value);
    
    public static Obj<B> Apply<A, B>(Morphism<A, B> morphism, Obj<A> value) =>
        new ApplyObj<A, B>(morphism, value);

    public static Obj<A> Flatten<A>(this Obj<Obj<A>> value) =>
        new FlattenObj<A>(value);

    public static Obj<A> Choice<A>(params Obj<A>[] values) =>
        new ChoiceObj<A>(values.ToSeq());

    public static Obj<A> Switch<X, A>(Obj<X> subject, params (Morphism<X, bool> Match, Morphism<X, A> Body)[] cases) =>
        new SwitchObj<X, A>(subject, cases.ToSeq());

    public static Obj<A> If<A>(Obj<bool> predicate, Obj<A> then, Obj<A> @else) =>
        Switch(predicate,
            (IdentityMorphism<bool>.Default, Morphism.Const<bool, A>(then)),
            (NotMorphism.Default, Morphism.Const<bool, A>(@else)));

    public static Obj<Unit> Guard(Obj<bool> predicate, Obj<Error> onFalse) =>
        If(predicate, Unit, Morphism.Bind<Error, Unit>(Fail<Unit>).Apply(onFalse));

    public static Obj<Unit> When(Obj<bool> predicate, Obj<Unit> onTrue) =>
        If(predicate, onTrue, Unit);
}

// ---------------------------------------------------------------------------------------------------------------------
// Interpreter state 

internal record State<RT>(ConcurrentDictionary<object, IDisposable> Disps, RT Runtime, object This)
{
    public static State<RT> Create(RT runtime) => new (new(), runtime, Prim<Unit>.None);

    public State<NRT> LocalRuntime<NRT>(Func<RT, NRT> f) =>
        new (Disps, f(Runtime), This);

    public State<RT> SetThis<NEW_THIS>(Prim<NEW_THIS> @this) =>
        new (Disps, Runtime, @this);
}

// ---------------------------------------------------------------------------------------------------------------------
// Primitive Objects 

public static class Prim
{
    public static Prim<A> Pure<A>(A value) =>
        new PurePrim<A>(value);

    public static Prim<A> Many<A>(Seq<Prim<A>> value) =>
        value.IsEmpty
            ? Prim<A>.None
            : value.Tail.IsEmpty
                ? value.Head
                : new ManyPrim<A>(value);

    public static Prim<A> Fail<A>(Error value) =>
        new LeftPrim<Error,A>(value);

    public static Prim<A> Left<X, A>(X value) =>
        new LeftPrim<X, A>(value);

    public static Prim<A> Flatten<A>(this Prim<Prim<A>> mma) =>
        mma.Bind(Prelude.identity);
}

public abstract record Prim<A> : Obj<A>
{
    public static readonly Prim<A> None = new ManyPrim<A>(Seq<Prim<A>>.Empty);

    public abstract Prim<B> Bind<B>(Func<A, Prim<B>> f);
    public abstract Prim<B> Map<B>(Func<A, B> f);
    public abstract bool IsNone { get; }
    public abstract bool IsMany { get; }
    public abstract bool IsSucc { get; }
    public abstract bool IsFail { get; }
}

public sealed record PurePrim<A>(A Value) : Prim<A>
{
    public override Prim<B> Bind<B>(Func<A, Prim<B>> f) =>
        f(Value);

    public override Prim<B> Map<B>(Func<A, B> f) =>
        new PurePrim<B>(f(Value));
 
    internal override Prim<A> Interpret<RT>(State<RT> state) => 
        this;

    /// <summary>
    /// True if all values are None
    /// </summary>
    public override bool IsNone => false;
    
    /// <summary>
    /// True if any values are Pure
    /// </summary>
    public override bool IsMany => false;
    
    /// <summary>
    /// True if all values are Pure
    /// </summary>
    public override bool IsSucc => true;
    
    /// <summary>
    /// True if any values are Fail
    /// </summary>
    public override bool IsFail => false;
}
    
public sealed record ManyPrim<A>(Seq<Prim<A>> Values) : Prim<A>
{
    public override Prim<B> Bind<B>(Func<A, Prim<B>> f) =>
        Prim.Many(Values.Map(x => x.Bind(f)));

    public override Prim<B> Map<B>(Func<A, B> f) =>
        Prim.Many(Values.Map(x => x.Map(f)));
 
    internal override Prim<A> Interpret<RT>(State<RT> state) => 
        this;

    public override bool IsNone => Values.ForAll(x => x.IsNone);
    public override bool IsMany => Values.Exists(x => x.IsSucc);
    public override bool IsSucc => Values.ForAll(x => x.IsSucc);
    public override bool IsFail => Values.Exists(x => x.IsFail);
}

public sealed record LeftPrim<X, A>(X Value) : Prim<A>
{
    public override Prim<B> Bind<B>(Func<A, Prim<B>> f) =>
        new LeftPrim<X, B>(Value);

    public override Prim<B> Map<B>(Func<A, B> f) =>
        new LeftPrim<X, B>(Value);

    internal override Prim<A> Interpret<RT>(State<RT> state) => 
        this;

    public override bool IsNone => false;
    public override bool IsMany => false;
    public override bool IsSucc => false;
    public override bool IsFail => true;
}

// ---------------------------------------------------------------------------------------------------------------------
// Objects 

public abstract record Obj<A>
{
    internal abstract Prim<A> Interpret<RT>(State<RT> state);

    public static readonly Obj<A> This = new ThisObj<A>();
}

sealed record PureObj<A>(A Value) : Obj<A>
{
    internal override Prim<A> Interpret<RT>(State<RT> state) =>
        new PurePrim<A>(Value);
}
    
sealed record LeftObj<X, A>(X Value) : Obj<A>
{
    internal override Prim<A> Interpret<RT>(State<RT> state) =>
        new LeftPrim<X, A>(Value);
}

sealed record ManyObj<A>(Seq<Obj<A>> Values) : Obj<A>
{
    internal override Prim<A> Interpret<RT>(State<RT> state) =>
        new ManyPrim<A>(Values.Map(x => x.Interpret(state)));
}

sealed record ThisObj<A> : Obj<A>
{
    internal override Prim<A> Interpret<RT>(State<RT> state) => 
        (Prim<A>)state.This;
}

sealed record ApplyObj<X, A>(Morphism<X, A> Morphism, Obj<X> Argument) : Obj<A>
{
    internal override Prim<A> Interpret<RT>(State<RT> state) =>
        Morphism.Invoke(state, Argument.Interpret(state));
}

sealed record FlattenObj<A>(Obj<Obj<A>> Value) : Obj<A>
{
    internal override Prim<A> Interpret<RT>(State<RT> state) =>
        Value.Interpret(state).Map(o => o.Interpret(state)).Flatten();
}

sealed record ChoiceObj<A>(Seq<Obj<A>> Values) : Obj<A>
{
    internal override Prim<A> Interpret<RT>(State<RT> state)
    {
        foreach (var obj in Values)
        {
            var r = obj.Interpret(state);
            if (r.IsSucc) return r;
        }
        return Prim<A>.None;
    }
}

sealed record SwitchObj<X, A>(Obj<X> Subject, Seq<(Morphism<X, bool> Match, Morphism<X, A> Body)> Cases) : Obj<A>
{
    internal override Prim<A> Interpret<RT>(State<RT> state)
    {
        var px = Subject.Interpret(state);
        foreach (var c in Cases)
        {
            var pr = c.Match.Invoke(state, px);
            var fl = false;
            var rs = pr.Bind(x =>
            {
                if (x)
                {
                    fl = true;
                    return c.Body.Invoke(state, px);
                }
                else
                {
                    return Prim<A>.None;
                }
            });
            if (fl) return rs;
        }

        return Prim<A>.None;
    }
}

// ---------------------------------------------------------------------------------------------------------------------
// Morphisms 

public static class Morphism
{
    public static Morphism<A, B> Fun<A, B>(Func<A, B> f) =>
        new FunMorphism<A, B>(f);
    
    public static Morphism<A, B> Fun<A, B>(Obj<Func<A, B>> f) =>
        new ObjFunMorphism<A, B>(f);
    
    public static Morphism<A, B> Bind<A, B>(Func<A, Obj<B>> f) =>
        new BindMorphism<A, B>(f);

    public static Morphism<A, C> Bind<A, B, C>(Morphism<A, B> obj, Func<B, Morphism<A, C>> f) =>
        new BindMorphism2<A, B, C>(obj, f);

    public static Morphism<A, D> Bind<A, B, C, D>(Morphism<A, B> Obj, Func<B, Morphism<A, C>> Bind, Func<B, C, D> Project) =>
        new BindProjectMorphism<A, B, C, D>(Obj, Bind, Project);

    public static Morphism<A, B> Map<A, B>(Func<Obj<A>, Obj<B>> f) =>
        new MapMorphism<A, B>(f);
    
    public static Morphism<A, C> Compose<A, B, C>(Morphism<A, B> f, Morphism<B, C> g) =>
        new ComposeMorphism<A, B, C>(f, g);
    
    public static Morphism<A, B> Const<A, B>(Obj<B> value) =>
        new ConstMorphism<A, B>(value);

    public static Morphism<A, A> Filter<A>(Morphism<A, bool> predicate) =>
        new FilterMorphism<A>(predicate);

    public static Morphism<A, B> Lambda<A, B>(Obj<B> body) =>
        new LambdaMorphism<A, B>(body);
}

public abstract record Morphism<A, B> 
{
    public Obj<B> Apply(Obj<A> value) =>
        new ApplyObj<A, B>(this, value);

    internal abstract Prim<B> Invoke<RT>(State<RT> state, Prim<A> value);
}

sealed record ConstMorphism<A, B>(Obj<B> Value) : Morphism<A, B>
{
    internal override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Value.Interpret(state);
}

sealed record FunMorphism<A, B>(Func<A, B> Value) : Morphism<A, B>
{
    internal override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Map(Value);
}
    
sealed record ObjFunMorphism<A, B>(Obj<Func<A, B>> Value) : Morphism<A, B>
{
    internal override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Value.Interpret(state).Bind(value.Map);
}
    
sealed record BindMorphism<A, B>(Func<A, Obj<B>> Value) : Morphism<A, B>
{
    internal override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Bind(x => Value(x).Interpret(state));
}
    
sealed record BindMorphism2<A, B, C>(Morphism<A, B> Obj, Func<B, Morphism<A, C>> Bind) : Morphism<A, C>
{
    internal override Prim<C> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Obj.Invoke(state, value).Bind(b => Bind(b).Invoke(state, value));
}
    
sealed record BindProjectMorphism<A, B, C, D>(Morphism<A, B> Obj, Func<B, Morphism<A, C>> Bind, Func<B, C, D> Project) : Morphism<A, D>
{
    internal override Prim<D> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Obj.Invoke(state, value).Bind(b => Bind(b).Invoke(state, value).Map(c => Project(b, c)));
}

sealed record MapMorphism<A, B>(Func<Obj<A>, Obj<B>> Value) : Morphism<A, B>
{
    internal override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Value(value).Interpret(state);
}

sealed record LambdaMorphism<A, B>(Obj<B> Body) : Morphism<A, B>
{
    internal override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Body.Interpret(state.SetThis(value));
}
    
sealed record FilterMorphism<A>(Morphism<A, bool> Predicate) : Morphism<A, A>
{
    internal override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Predicate.Invoke(state, value)
                 .Bind(x => x ? value : Prim<A>.None);
}

sealed record ComposeMorphism<A, B, C>(Morphism<A, B> Left, Morphism<B, C> Right) : Morphism<A, C>
{
    internal override Prim<C> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Right.Apply(Left.Invoke(state, value)).Interpret(state);
}
    
sealed record ManyMorphism<A, B>(Seq<Morphism<A, B>> Values) : Morphism<A, B>
{
    internal override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Prim.Many(Values.Map(m => m.Invoke(state, value)));
}

sealed record IdentityMorphism<A> : Morphism<A, A>
{
    public static readonly Morphism<A, A> Default = new IdentityMorphism<A>();

    internal override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value;
}

sealed record NotMorphism : Morphism<bool, bool>
{
    public static readonly Morphism<bool, bool> Default = new NotMorphism();

    internal override Prim<bool> Invoke<RT>(State<RT> state, Prim<bool> value) =>
        value.Map(static x => !x);
}

public record Eff<RT, A>(Morphism<RT, A> Op) : Morphism<RT, A>
{
    public static Eff<RT, A> operator |(Eff<RT, A> ma, Eff<RT, A> mb) =>
        new(Morphism.Map<RT, A>(rt => Obj.Choice(ma.Op.Apply(rt), mb.Op.Apply(rt))));

    internal override Prim<A> Invoke<RT1>(State<RT1> state, Prim<RT> value) =>
        value.Bind(nrt => Op.Invoke(state.LocalRuntime(_ => nrt), Prim.Pure(nrt)));
}

public static class PreludeExample
{
    public static Eff<RT, A> SuccessEff<RT, A>(A value) =>
        new (Morphism.Const<RT, A>(Obj.Pure(value)));
    
    public static Eff<RT, A> FailEff<RT, A>(Error value) =>
        new (Morphism.Const<RT, A>(Obj.Fail<A>(value)));

    public static Eff<RT, A> EffectMaybe<RT, A>(Func<RT, Fin<A>> f) =>
        new(Morphism.Bind<RT, A>(rt => f(rt).Match(Succ: Obj.Pure, Fail: Obj.Fail<A>)));

    public static Eff<RT, A> Effect<RT, A>(Func<RT, A> f) =>
        new(Morphism.Fun(f));

    public static Prim<A> Run<RT, A>(this Eff<RT, A> ma, RT runtime)
    {
        var state = State<RT>.Create(runtime);
        return ma.Op.Invoke(state, Prim.Pure(runtime));
    }

    public static Eff<RT, B> Map<RT, A, B>(this Eff<RT, A> ma, Func<A, B> f) =>
        new(Morphism.Compose(ma.Op, Morphism.Fun(f)));

    public static Eff<RT, B> Bind<RT, A, B>(this Eff<RT, A> ma, Func<A, Eff<RT, B>> f) =>
        new(Morphism.Bind(ma.Op, f));
    
    public static Eff<RT, B> SelectMany<RT, A, B>(this Eff<RT, A> ma, Func<A, Eff<RT, B>> f) =>
        new(Morphism.Bind(ma.Op, f));
    
    public static Eff<RT, C> SelectMany<RT, A, B, C>(this Eff<RT, A> ma, Func<A, Eff<RT, B>> bind, Func<A, B, C> project) =>
        new(Morphism.Bind(ma.Op, bind, project));

    public static Eff<RT, B> Apply<RT, A, B>(this Eff<RT, Func<A, B>> ff, Eff<RT, A> fa) =>
        new(Morphism.Lambda<RT, B>(Morphism.Fun(ff.Op.Apply(Obj<RT>.This)).Apply(fa.Op.Apply(Obj<RT>.This))));

    public static Eff<RT, Func<B, C>> Apply<RT, A, B, C>(this Eff<RT, Func<A, B, C>> ff, Eff<RT, A> fa) =>
        ff.Map(Prelude.curry).Apply(fa);

    public static Eff<RT, Func<B, Func<C, D>>> Apply<RT, A, B, C, D>(this Eff<RT, Func<A, B, C, D>> ff, Eff<RT, A> fa) =>
        ff.Map(Prelude.curry).Apply(fa);

    public static Eff<RT, Func<B, Func<C, Func<D, E>>>> Apply<RT, A, B, C, D, E>(this Eff<RT, Func<A, B, C, D, E>> ff, Eff<RT, A> fa) =>
        ff.Map(Prelude.curry).Apply(fa);

    public static Eff<RT, Func<B, Func<C, Func<D, Func<E, F>>>>> Apply<RT, A, B, C, D, E, F>(this Eff<RT, Func<A, B, C, D, E, F>> ff, Eff<RT, A> fa) =>
        ff.Map(Prelude.curry).Apply(fa);
}
