#nullable enable

using System;
using LanguageExt.Common;

namespace LanguageExt.Core.DSL;

public static class Prim
{
    public static readonly Prim<Unit> Unit = Prim.Pure(Prelude.unit);

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
    public virtual Prim<B> Cast<B>() => throw new InvalidCastException();

    public abstract Prim<A> Head { get; }
    public abstract Prim<A> Last { get; }
    public abstract Prim<A> Tail { get; }
    public abstract Prim<A> Skip(int amount);
    public abstract Prim<A> Take(int amount);

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
 
    public override Prim<A> Interpret<RT>(State<RT> state) => 
        this;

    public override Prim<A> Head =>
        this;

    public override Prim<A> Last =>
        this;

    public override Prim<A> Tail =>
        None;

    public override Prim<A> Skip(int amount) =>
        amount == 0 ? this : None;
    
    public override Prim<A> Take(int amount) =>
        amount == 0 ? this : None;

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

    public override Prim<A> Interpret<RT>(State<RT> state) =>
        this;

    public override Prim<A> Head =>
        Values.IsEmpty
            ? None
            : Values.Head;

    public override Prim<A> Last =>
        Values.IsEmpty
            ? None
            : Values.Last;

    public override Prim<A> Tail =>
        Prim.Many(Values.Tail);

    public override Prim<A> Skip(int amount) =>
        Prim.Many(Values.Skip(amount));
    
    public override Prim<A> Take(int amount) =>
        Prim.Many(Values.Skip(amount));

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

    public override Prim<A> Interpret<RT>(State<RT> state) => 
        this;

    public override Prim<B> Cast<B>() =>
        Prim.Left<X, B>(Value);
    
    public override Prim<A> Head =>
        this;

    public override Prim<A> Last =>
        this;

    public override Prim<A> Tail =>
        this;

    public override Prim<A> Skip(int amount) =>
        this;
    
    public override Prim<A> Take(int amount) =>
        this;

    public override bool IsNone => false;
    public override bool IsMany => false;
    public override bool IsSucc => false;
    public override bool IsFail => true;
}
