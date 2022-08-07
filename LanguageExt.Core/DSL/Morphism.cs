#nullable enable
using System;
using LanguageExt.TypeClasses;

namespace LanguageExt.DSL;

public static class Morphism
{
    public static Morphism<A, B> function<A, B>(Func<A, B> f) =>
        new FunMorphism<A, B>(f);

    public static Morphism<A, B> function<A, B>(Obj<Func<A, B>> f) =>
        new ObjFunMorphism<A, B>(f);

    public static Morphism<A, B> bind<A, B>(Func<A, Obj<B>> f) =>
        new BindMorphism<A, B>(f);

    public static Morphism<A, C> bind<A, B, C>(Func<A, Obj<B>> bind, Func<A, B, C> project) =>
        new BindProjectMorphism2<A, B, C>(bind, project);

    public static Morphism<A, C> bind<A, B, C>(Morphism<A, B> obj, Func<B, Morphism<A, C>> f) =>
        new BindMorphism2<A, B, C>(obj, f);

    public static Morphism<A, D> bind<A, B, C, D>(Morphism<A, B> Obj, 
        Func<B, Morphism<A, C>> Bind,
        Func<B, C, D> Project) =>
        new BindProjectMorphism<A, B, C, D>(Obj, Bind, Project);

    public static Morphism<A, B> map<A, B>(Func<Obj<A>, Obj<B>> f) =>
        new MapMorphism<A, B>(f);

    public static Morphism<A, C> compose<A, B, C>(Morphism<A, B> f, Morphism<B, C> g) =>
        new ComposeMorphism<A, B, C>(f, g);

    public static Morphism<A, B> constant<A, B>(Obj<B> value) =>
        new ConstMorphism<A, B>(value);

    public static Morphism<A, A> filter<A>(Func<A, bool> predicate) =>
        new FilterMorphism<A>(predicate);

    public static Morphism<A, A> filter<A>(Morphism<A, bool> predicate) =>
        new FilterMorphism2<A>(predicate);

    public static Morphism<A, B> lambda<A, B>(Obj<B> body) =>
        new LambdaMorphism<A, B>(body);

    public static Morphism<A, A> skip<A>(int amount) =>
        new SkipMorphism<A>(amount);

    public static Morphism<A, A> take<A>(int amount) =>
        new TakeMorphism<A>(amount);

    public static Morphism<A, B> many<A, B>(Seq<Morphism<A, B>> ms) =>
        new ManyMorphism<A, B>(ms);
    
    public static Morphism<A, A> use<A>() where A : IDisposable => 
        UseMorphism<A>.Default;

    public static Morphism<A, A> use<A>(Morphism<A, Unit> release) =>
        new UseMorphism2<A>(release);
}

public static class Morphism<A>
{
    public static readonly Morphism<A, Obj<A>> pure = new PureMorphism<A>();
    public static readonly Morphism<A, A> head = new HeadMorphism<A>();
    public static readonly Morphism<A, A> tail = new TailMorphism<A>();
    public static readonly Morphism<A, A> last = new LastMorphism<A>();
    public static readonly Morphism<A, A> identity = new IdentityMorphism<A>();
    public static readonly Morphism<A, Unit> release = ReleaseMorphism<A>.Default;
}

public abstract record Morphism<A, B>
{
    public Obj<B> Apply(Obj<A> value) =>
        new ApplyObj<A, B>(this, value);

    public abstract Prim<B> Invoke<RT>(State<RT> state, Prim<A> value);
}

internal sealed record ConstMorphism<A, B>(Obj<B> Value) : Morphism<A, B>
{
    public override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Value.Interpret(state);
}

internal sealed record FunMorphism<A, B>(Func<A, B> Value) : Morphism<A, B>
{
    public override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Map(Value);
}

internal sealed record ObjFunMorphism<A, B>(Obj<Func<A, B>> Value) : Morphism<A, B>
{
    public override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Value.Interpret(state).Bind(value.Map);
}

internal sealed record BindMorphism<A, B>(Func<A, Obj<B>> Value) : Morphism<A, B>
{
    public override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Bind(x => Value(x).Interpret(state));
}

internal sealed record BindMorphism2<A, B, C>(Morphism<A, B> Obj, Func<B, Morphism<A, C>> Bind) : Morphism<A, C>
{
    public override Prim<C> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Obj.Invoke(state, value).Bind(b => Bind(b).Invoke(state, value));
}

internal sealed record BindProjectMorphism<A, B, C, D>(
    Morphism<A, B> Obj, 
    Func<B, Morphism<A, C>> Bind, 
    Func<B, C, D> Project) 
    : Morphism<A, D>
{
    public override Prim<D> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Obj.Invoke(state, value).Bind(b => Bind(b).Invoke(state, value).Map(c => Project(b, c)));
}

internal sealed record BindProjectMorphism2<A, B, C>(
    Func<A, Obj<B>> bind,
    Func<A, B, C> project)
    : Morphism<A, C>
{
    public override Prim<C> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Bind(a => bind(a).Interpret(state).Map(b => project(a, b)));
}
    
internal sealed record MapMorphism<A, B>(Func<Obj<A>, Obj<B>> Value) : Morphism<A, B>
{
    public override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Value(value).Interpret(state);
}

internal sealed record LambdaMorphism<A, B>(Obj<B> Body) : Morphism<A, B>
{
    public override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Body.Interpret(state.SetThis(value));
}

internal sealed record FilterMorphism<A>(Func<A, bool> Predicate) : Morphism<A, A>
{
    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Bind(v => Predicate(v) ? value : Prim<A>.None);
}

internal sealed record FilterMorphism2<A>(Morphism<A, bool> Predicate) : Morphism<A, A>
{
    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Predicate.Invoke(state, value)
            .Bind(x => x ? value : Prim<A>.None);
}

internal sealed record ComposeMorphism<A, B, C>(Morphism<A, B> Left, Morphism<B, C> Right) : Morphism<A, C>
{
    public override Prim<C> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Right.Invoke(state, Left.Invoke(state, value));
}

internal sealed record ManyMorphism<A, B>(Seq<Morphism<A, B>> Values) : Morphism<A, B>
{
    public override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Prim.Many(Values.Map(m => m.Invoke(state, value)));
}

internal sealed record IdentityMorphism<A> : Morphism<A, A>
{
    public static readonly Morphism<A, A> Default = new IdentityMorphism<A>();

    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value;
}

internal sealed record NotMorphism : Morphism<bool, bool>
{
    public static readonly Morphism<bool, bool> Default = new NotMorphism();

    public override Prim<bool> Invoke<RT>(State<RT> state, Prim<bool> value) =>
        value.Map(static x => !x);
}

internal sealed record SkipMorphism<A>(int Amount) : Morphism<A, A>
{
    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Skip(Amount);
}

internal sealed record TakeMorphism<A>(int Amount) : Morphism<A, A>
{
    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Take(Amount);
}

internal sealed record HeadMorphism<A> : Morphism<A, A>
{
    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Head;
}

internal sealed record TailMorphism<A> : Morphism<A, A>
{
    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Tail;
}

internal sealed record LastMorphism<A> : Morphism<A, A>
{
    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Tail;
}

internal sealed record PureMorphism<A> : Morphism<A, Obj<A>>
{
    public override Prim<Obj<A>> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Prim.Pure<Obj<A>>(value);
}

internal sealed record UseMorphism<A> : Morphism<A, A>
    where A : IDisposable
{
    public static Morphism<A, A> Default = new UseMorphism<A>();
    
    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value)
    {
        state.Use(value, value);
        return value;
    }
}

internal sealed record UseMorphism2<A>(Morphism<A, Unit> Release) : Morphism<A, A>
{
    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value)
    {
        state.Use(value, new Acq<RT>(state, value, Release));
        return value;
    }
    record Acq<RT>(State<RT> State, Prim<A> Value, Morphism<A, Unit> Release) : IDisposable
    {
        public void Dispose() =>
            Release.Apply(Value).Interpret(State);
    }

}

internal sealed record ReleaseMorphism<A> : Morphism<A, Unit>
{
    public static Morphism<A, Unit> Default = new ReleaseMorphism<A>();
    
    public override Prim<Unit> Invoke<RT>(State<RT> state, Prim<A> value)
    {
        state.Release(value);
        return Prim.Unit;
    }
}
