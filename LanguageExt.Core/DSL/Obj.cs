#nullable enable
using System;

namespace LanguageExt.DSL;

/*
public static class ObjAny
{
    public static DSL<MError, Error>.Obj<A> Fail<A>(Error value) =>
        new DSL<MError, Error>.LeftObj<A>(value);
    
    public static DSL<MErr, E>.Obj<A> Flatten<MErr, E, A>(this DSL<MErr, E>.Obj<DSL<MErr, E>.Obj<A>> value)
        where MErr : struct, Semigroup<E>, Convertable<Exception, E> =>
        new DSL<MErr, E>.FlattenObj<A>(value);
}
*/

public static class Obj
{
    public static readonly Obj<Unit> Unit = Pure(LanguageExt.Prelude.unit);
 
    public static Obj<A> Pure<A>(A value) =>
        new PureObj<A>(value);

    public static Obj<A> Many<A>(Seq<Obj<A>> values) =>
        new ManyObj<A>(values);

    public static Obj<B> Apply<A, B>(Morphism<A, B> morphism, Obj<A> value) =>
        new ApplyObj<A, B>(morphism, value);

    public static Obj<CoProduct<X, A>> Choice<X, A>(params Obj<CoProduct<X, A>>[] values) =>
        new ChoiceObj<X, A>(values.ToSeq());

    public static Obj<A> Switch<X, A>(Obj<X> subject,
        params (Morphism<X, bool> Match, Morphism<X, A> Body)[] cases) =>
        new SwitchObj<X, A>(subject, cases.ToSeq());

    public static Obj<A> If<A>(Obj<bool> predicate, Obj<A> then, Obj<A> @else) =>
        Switch(predicate,
            (IdentityMorphism<bool>.Default, Morphism.constant<bool, A>(then)),
            (NotMorphism.Default, Morphism.constant<bool, A>(@else)));

    public static Obj<CoProduct<E, Unit>> Guard<E>(Obj<bool> predicate, Obj<E> onFalse) =>
        If(predicate, Unit, Morphism.bind<E, Unit>(Left<Unit>).Apply(onFalse));

    public static Obj<Unit> When(Obj<bool> predicate, Obj<Unit> onTrue) =>
        If(predicate, onTrue, Unit);

    public static Obj<A> Use<A>(Obj<A> value) where A : IDisposable =>
        Morphism.use<A>().Apply(value);

    public static Obj<A> Use<A>(Obj<A> value, Morphism<A, Unit> release) =>
        Morphism.use(release).Apply(value);
    
    public static Obj<Unit> Release<A>(Obj<A> value) =>
        Morphism<A>.release.Apply(value);
    
    public static Obj<A> Flatten<A>(this Obj<Obj<A>> value) =>
        new FlattenObj<A>(value);
}

public abstract record Obj<A>
{
    public abstract Prim<A> Interpret<RT>(State<RT> state);
    public static readonly Obj<A> This = new ThisObj<A>();
}

internal sealed record PureObj<A>(A Value) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        new PurePrim<A>(Value);

    public override string ToString() => 
        $"Pure({Value})";
}

internal sealed record ManyObj<A>(Seq<Obj<A>> Values) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        Prim.Many(Values.Map(x => x.Interpret(state)));

    public override string ToString() => 
        $"Many{Values}";
}

internal sealed record ThisObj<A> : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        state.This as Prim<A> ?? Prim<A>.None;

    public override string ToString() => 
        "This";
}

internal sealed record ApplyObj<X, A>(Morphism<X, A> Morphism, Obj<X> Argument) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        Morphism.Invoke(state, Argument.Interpret(state));

    public override string ToString() => 
        $"Apply {Morphism}({Argument})";
}

internal sealed record FlattenObj<A>(Obj<Obj<A>> Value) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        Value.Interpret(state).Map(o => o.Interpret(state)).Flatten();

    public override string ToString() => 
        $"Flatten";
}

internal sealed record ChoiceObj<X, A>(Seq<Obj<CoProduct<X, A>>> Values) : Obj<CoProduct<X, A>>
{
    public override Prim<CoProduct<X, A>> Interpret<RT>(State<RT> state)
    {
        foreach (var obj in Values)
        {
            var r = obj.Interpret(state);
            if (r.ForAll(x => x.IsRight)) return r;
        }
        return Prim<CoProduct<X, A>>.None;
    }

    public override string ToString() => 
        $"{string.Join(" | ", Values.Map(x => $"{x}"))}";
}

internal sealed record SwitchObj<X, A>(Obj<X> Subject, Seq<(Morphism<X, bool> Match, Morphism<X, A> Body)> Cases) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state)
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
    public override string ToString() => 
        $"Switch";
}

internal sealed record HeadObj<A>(Obj<A> Value) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        Value.Interpret(state).Head;
    
    public override string ToString() => 
        $"{Value}.Head";
}

internal sealed record TailObj<A>(Obj<A> Value) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        Value.Interpret(state).Tail;
    
    public override string ToString() => 
        $"{Value}.Tail";
}

internal sealed record LastObj<A>(Obj<A> Value) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        Value.Interpret(state).Last;
    
    public override string ToString() => 
        $"{Value}.Last";
}

internal sealed record SkipObj<A>(Obj<A> Value, int amount) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        Value.Interpret(state).Skip(amount);
    
    public override string ToString() => 
        $"{Value}.Skip({amount})";
}

internal sealed record TakeObj<A>(Obj<A> Value, int amount) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        Value.Interpret(state).Take(amount);
    
    public override string ToString() => 
        $"{Value}.Take({amount})";
}

internal sealed record ToUnitObj<A>(Obj<A> Value) : Obj<Unit>
{
    public override Prim<Unit> Interpret<RT>(State<RT> state)
    {
        var result = Value.Interpret(state).ToUnit();

        switch (result)
        {
            case ManyPrim<A> m: 
                m.Items.Strict();  // Force side-effects to happen
                break;
            
            case ObservablePrim<A> o:
                o.ToResult();  // Force observable to run
                break;
        }
        
        return Prim.Unit;
    }
    
    public override string ToString() => 
        $"{Value}.ToUnit";
}
