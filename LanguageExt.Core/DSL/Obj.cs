#nullable enable
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using LanguageExt.TypeClasses;

namespace LanguageExt.DSL;

public static class Obj
{
    public static readonly Obj<Unit> Unit = Pure(LanguageExt.Prelude.unit);
 
    public static Obj<A> Pure<A>(A value) =>
        new PureObj<A>(value);

    public static Obj<A> Many<A>(IEnumerable<Obj<A>> values) =>
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
        If(predicate,
            Pure(CoProduct.Right<E, Unit>(default)),
            Morphism.bind<E, CoProduct<E, Unit>>(e => Pure(CoProduct.Left<E, Unit>(e))).Apply(onFalse));

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

    public static Obj<A> Collect<FaultA, A>(IObservable<Obj<A>> ma) =>
        new ObservableCollectObj<A>(ma);

    public static Obj<A> Consume<FaultA, A>(IObservable<Obj<A>> ma) =>
        new ObservableConsumeObj<A>(ma);
}

public abstract record Obj<A>
{
    public abstract Prim<A> Interpret<RT>(State<RT> state);

    public virtual Obj<B> Bind<B>(Morphism<A, B> f) =>
        new ApplyObj<A, B>(f, this);
    
    public static readonly Obj<A> This = 
        new ThisObj<A>();
}

internal sealed record PureObj<A>(A Value) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        new PurePrim<A>(Value);
 
    public override string ToString() => 
        $"Pure({Value})";
}

internal sealed record ManyObj<A>(IEnumerable<Obj<A>> Values) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        Prim.Many(Values.Map(x => x.Interpret(state)).ToSeq());

    public override Obj<B> Bind<B>(Morphism<A, B> f) =>
        new ManyObj<B>(Values.Map(f.Apply));
 
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

    public override Obj<B> Bind<B>(Morphism<A, B> f) =>
        new ApplyObj<X, B>(DSL.Morphism.compose(Morphism, f), Argument);

    public override string ToString() =>
        $"Apply {Morphism}({Argument})";
}

internal sealed record FlattenObj<A>(Obj<Obj<A>> Value) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        Value.Interpret(state).Map(o => o.Interpret(state)).Flatten(state);

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
            if (r.ForAll(state, x => x.IsRight)) return r;
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
            var rs = pr.Bind(state, x =>
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
        Value.Interpret(state);
        return Prim.Unit;
    }
    
    public override string ToString() => 
        $"{Value}.ToUnit";
}

public sealed record ObservableCollectObj<A>(IObservable<Obj<A>> Items) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state)
    {
        using var collect = new Collector<RT>(state);
        using var sub = Items.Subscribe(collect);
        collect.Wait.WaitOne();
        collect.Error?.Rethrow<Unit>();
        return collect.Value;
    }

    public override Obj<B> Bind<B>(Morphism<A, B> f) =>
        new ObservableCollectObj<B>(Items.Select(f.Apply));

    public override string ToString() => 
        $"Prim.Observable<{typeof(A).Name}>";

    class Collector<RT> : IObserver<Obj<A>>, IDisposable
    {
        public readonly State<RT> State;
        public readonly AutoResetEvent Wait = new(false);
        public Exception? Error;
        public Prim<A> Value = Prim<A>.None;

        public Collector(State<RT> state) =>
            State = state;

        public void OnNext(Obj<A> value)
        {
            var prim = value.Interpret(State);
            Value = Value.Append(prim);
        }            

        public void OnCompleted() =>
            Wait.Set();

        public void OnError(Exception error)
        {
            Error = error;
            Wait.Set();
        }

        public void Dispose() =>
            Wait.Dispose();
    }
}

public sealed record ObservableConsumeObj<A>(IObservable<Obj<A>> Items) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state)
    {
        using var consume = new Consumer<RT>(state);
        using var sub = Items.Subscribe(consume);
        consume.Wait.WaitOne();
        consume.Error?.Rethrow<Unit>();
        return consume.Value;
    }

    public override Obj<B> Bind<B>(Morphism<A, B> f) =>
        new ObservableCollectObj<B>(Items.Select(f.Apply));

    public override string ToString() => 
        $"Prim.Observable<{typeof(A).Name}>";
            
    class Consumer<RT> : IObserver<Obj<A>>, IDisposable
    {
        public readonly State<RT> State;
        public readonly AutoResetEvent Wait = new(false);
        public Prim<A> Value = Prim<A>.None;
        public Exception? Error;

        public Consumer(State<RT> state) =>
            State = state;

        public void OnNext(Obj<A> value)
        {
            var prim = value.Interpret(State);
            Value = prim;
        }

        public void OnCompleted() =>
            Wait.Set();

        public void OnError(Exception error)
        {
            Error = error;
            Wait.Set();
        }

        public void Dispose() =>
            Wait.Dispose();
    }
}
