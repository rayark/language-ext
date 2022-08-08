#nullable enable

using System;

namespace LanguageExt.DSL;

public static class Prim
{
    public static readonly Prim<Unit> Unit = Pure(LanguageExt.Prelude.unit);

    public static Prim<A> Pure<A>(A value) =>
        new PurePrim<A>(value);

    public static Prim<A> Many<A>(Seq<Prim<A>> value) =>
        value.IsEmpty
            ? Prim<A>.None
            : value.Tail.IsEmpty
                ? value.Head
                : new ManyPrim<A>(value);
    
    public static Prim<A> Flatten<A>(this Prim<Prim<A>> mma) =>
        mma.Bind(LanguageExt.Prelude.identity);
}

public abstract record Prim<A> : Obj<A>, IDisposable
{
    public static readonly Prim<A> None = new ManyPrim<A>(Seq<Prim<A>>.Empty);

    public override Prim<A> Interpret<RT>(State<RT> state) =>
        this;

    public abstract Prim<B> Bind<B>(Func<A, Prim<B>> f);
    public abstract Prim<B> Map<B>(Func<A, B> f);
    public abstract Prim<S> Fold<S>(Prim<S> state, Func<S, A, S> f);
    public abstract Prim<A> Append(Prim<A> rhs);

    public new abstract Prim<A> Head { get; }
    public new abstract Prim<A> Last { get; }
    public new abstract Prim<A> Tail { get; }
    public new abstract Prim<A> Skip(int amount);
    public new abstract Prim<A> Take(int amount);
    public abstract bool ForAll(Func<A, bool> f);
    public abstract bool Exists(Func<A, bool> f);

    /// <summary>
    /// Dispose
    /// </summary>
    public abstract void Dispose();
}

public sealed record PurePrim<A>(A Value) : Prim<A>
{
    public override Prim<B> Bind<B>(Func<A, Prim<B>> f) =>
        f(Value);

    public override Prim<B> Map<B>(Func<A, B> f) =>
        new PurePrim<B>(f(Value));

    public override Prim<S> Fold<S>(Prim<S> state, Func<S, A, S> f) =>
        state.Map(s => f(s, Value));
    
    public override bool ForAll(Func<A, bool> f) => 
        f(Value);
    
    public override bool Exists(Func<A, bool> f) => 
        f(Value);
 
    public override Prim<A> Append(Prim<A> rhs) =>
        rhs switch
        {
            PurePrim<A> p                     => Prim.Many(LanguageExt.Prelude.Seq(Prim.Pure(Value), Prim.Pure(p.Value))),
            ManyPrim<A> {Items.IsEmpty: true} => this,
            ManyPrim<A> p                     => Prim.Many(Prim.Pure(Value).Cons(p.Items)),
            _                                 => throw new InvalidOperationException("Prim shouldn't be extended")
        };
 
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
    /// Dispose
    /// </summary>
    public override void Dispose()
    {
        if(Value is IDisposable d) d.Dispose();
    }
    
    public override string ToString() => 
        $"{Value}";
}

public sealed record ManyPrim<A>(Seq<Prim<A>> Items) : Prim<A>
{
    public override Prim<B> Bind<B>(Func<A, Prim<B>> f) =>
        Prim.Many(Items.Map(x => x.Bind(f)));

    public override Prim<B> Map<B>(Func<A, B> f) =>
        Prim.Many(Items.Map(x => x.Map(f)));

    public override Prim<S> Fold<S>(Prim<S> state, Func<S, A, S> f) =>
        state.Bind(s => Items.Fold(Prim.Pure(s), (s1, px) => px.Fold(s1, f)));

    public override Prim<A> Append(Prim<A> rhs) =>
        rhs switch
        {
            _ when Items.IsEmpty => rhs,
            PurePrim<A> p        => Prim.Many(Items.Add(Prim.Pure(p.Value))),
            ManyPrim<A> p        => Prim.Many(Items + p.Items),
            _                    => throw new InvalidOperationException("Result shouldn't be extended")
        };

    public override bool ForAll(Func<A, bool> f) => 
        Items.ForAll(x => x.ForAll(f));
    
    public override bool Exists(Func<A, bool> f) => 
        Items.Exists(x => x.Exists(f));
 
    public override Prim<A> Head =>
        Items.IsEmpty
            ? None
            : Items.Head;

    public override Prim<A> Last =>
        Items.IsEmpty
            ? None
            : Items.Last;

    public override Prim<A> Tail =>
        Prim.Many(Items.Tail);

    public override Prim<A> Skip(int amount) =>
        Prim.Many(Items.Skip(amount));

    public override Prim<A> Take(int amount) =>
        Prim.Many(Items.Skip(amount));

    /// <summary>
    /// Dispose
    /// </summary>
    public override void Dispose() =>
        Items.Iter(x => x.Dispose());

    public override string ToString() => 
        $"Many";
}
