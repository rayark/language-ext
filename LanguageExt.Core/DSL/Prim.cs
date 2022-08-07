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
    
    public static Prim<A> Flatten<RT, A>(this Prim<Prim<A>> mma, State<RT> state) =>
        mma.Bind(state, LanguageExt.Prelude.identity);
}

public abstract record Prim<A> : Obj<A>, IDisposable
{
    public static readonly Prim<A> None = new ManyPrim<A>(Seq<Prim<A>>.Empty);

    public override Prim<A> Interpret<RT>(State<RT> state) =>
        this;

    public abstract Prim<B> Bind<RT, B>(State<RT> state, Func<A, Obj<B>> f);
    public abstract Prim<B> Map<B>(Func<A, B> f);
    public abstract Prim<A> Append(Prim<A> rhs);

    public abstract Prim<A> Head { get; }
    public abstract Prim<A> Last { get; }
    public abstract Prim<A> Tail { get; }
    public abstract Prim<A> Skip(int amount);
    public abstract Prim<A> Take(int amount);
    public abstract bool ForAll<RT>(State<RT> state, Func<A, bool> f);
    public abstract bool Exists<RT>(State<RT> state, Func<A, bool> f);

    /// <summary>
    /// Dispose
    /// </summary>
    public abstract void Dispose();
}

public sealed record PurePrim<A>(A Value) : Prim<A>
{
    public override Prim<B> Bind<RT, B>(State<RT> state, Func<A, Obj<B>> f) =>
        f(Value).Interpret(state);

    public override Prim<B> Map<B>(Func<A, B> f) =>
        new PurePrim<B>(f(Value));

    public override bool ForAll<RT>(State<RT> state, Func<A, bool> f) => 
        f(Value);
    
    public override bool Exists<RT>(State<RT> state, Func<A, bool> f) => 
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
    public override Prim<B> Bind<RT, B>(State<RT> state, Func<A, Obj<B>> f) =>
        Prim.Many(Items.Map(x => x.Bind(state, f)));

    public override Prim<B> Map<B>(Func<A, B> f) =>
        Prim.Many(Items.Map(x => x.Map(f)));
 
    public override Prim<A> Append(Prim<A> rhs) =>
        rhs switch
        {
            _ when Items.IsEmpty => rhs,
            PurePrim<A> p        => Prim.Many(Items.Add(Prim.Pure(p.Value))),
            ManyPrim<A> p        => Prim.Many(Items + p.Items),
            _                    => throw new InvalidOperationException("Result shouldn't be extended")
        };

    public override bool ForAll<RT>(State<RT> state, Func<A, bool> f) => 
        Items.ForAll(x => x.ForAll(state, f));
    
    public override bool Exists<RT>(State<RT> state, Func<A, bool> f) => 
        Items.Exists(x => x.Exists(state, f));
 
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
