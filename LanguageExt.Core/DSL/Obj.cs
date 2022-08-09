#nullable enable
using System;
using System.Collections.Generic;
using LanguageExt.Common;

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

    public static Obj<CoProduct<X, A>> attempt<X, A>(Obj<A> @try, Func<Exception, X> @catch) =>
        new TryObj<X, A>(@try, @catch);

    public static Obj<B> ApplyT<A, B>(this Obj<Morphism<A, B>> mm, Obj<A> x) =>
        mm.Bind(Morphism.bind<Morphism<A, B>, B>(m => m.Apply(x)));

    public static Obj<Morphism<A, C>> ApplyT<A, B, C>(this Obj<Morphism<A, B>> mx, Morphism<B, C> my) =>
        mx.Bind(Morphism.function<Morphism<A, B>, Morphism<A, C>>(m => m.Compose(my)));

    public static Obj<Morphism<A, C>> ApplyT<A, B, C>(this Obj<Morphism<A, B>> mx, Obj<Morphism<B, C>> my) =>
        mx.Bind(Morphism.bind<Morphism<A, B>, Morphism<A, C>>(x =>
            Morphism.function<Morphism<B, C>, Morphism<A, C>>(x.Compose).Apply(my)));
    
    public static Prim<B> InvokeT<RT, A, B>(this Obj<Morphism<A, B>> mm, State<RT> state, Prim<A> x) =>
        mm.Bind(Morphism.bind<Morphism<A, B>, B>(m => m.Invoke(state, x))).Interpret(state);

    public static Obj<A> ToObj<A>(this Obj<CoProduct<Error, A>> Object) =>
        new ToObj<A>(Object);
}

public abstract record Obj<A>
{
    public abstract Prim<A> Interpret<RT>(State<RT> state);

    public virtual Obj<B> Bind<B>(Morphism<A, B> f) =>
        new ApplyObj<A, B>(f, this);
    
    public static readonly Obj<A> This = 
        new ThisObj<A>();

    public virtual Obj<A> Head => Bind(Morphism<A>.head);
    public virtual Obj<A> Last => Bind(Morphism<A>.last);
    public virtual Obj<A> Tail => Bind(Morphism<A>.tail);
    public virtual Obj<A> Skip(int amount) => Bind(Morphism.skip<A>(amount));
    public virtual Obj<A> Take(int amount) => Bind(Morphism.take<A>(amount));
}

internal sealed record PureObj<A>(A Value) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        new PurePrim<A>(Value);
 
    public override string ToString() => 
        $"Pure({Value})";
}

internal sealed record FailObj<A>(Error Error) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        new FailPrim<A>(Error);
 
    public override string ToString() => 
        $"Fail({Error})";
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
        Value.Interpret(state);
        return Prim.Unit;
    }
    
    public override string ToString() => 
        $"{Value}.ToUnit";
}

/*
internal  sealed record ObservableCollectObj<A>(IObservable<Obj<A>> Items) : Obj<A>
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
    
    public override Obj<A> Head => 
        new ObservableCollectObj<A>(Items.FirstAsync());

    public override Obj<A> Last => 
        new ObservableCollectObj<A>(Items.LastAsync());

    public override Obj<A> Tail => 
        new ObservableCollectObj<A>(Items.Skip(1));

    public override Obj<A> Skip(int amount) => 
        new ObservableCollectObj<A>(Items.Skip(amount));

    public override Obj<A> Take(int amount) => 
        new ObservableCollectObj<A>(Items.Take(amount));

    public override string ToString() => 
        $"Obj.Observable<{typeof(A).Name}>";

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

internal sealed record ObservableConsumeObj<A>(IObservable<Obj<A>> Items) : Obj<A>
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
        new ObservableConsumeObj<B>(Items.Select(f.Apply));
    
    public override Obj<A> Head => 
        new ObservableConsumeObj<A>(Items.FirstAsync());

    public override Obj<A> Last => 
        new ObservableConsumeObj<A>(Items.LastAsync());

    public override Obj<A> Tail => 
        new ObservableConsumeObj<A>(Items.Skip(1));

    public override Obj<A> Skip(int amount) => 
        new ObservableConsumeObj<A>(Items.Skip(amount));

    public override Obj<A> Take(int amount) => 
        new ObservableConsumeObj<A>(Items.Take(amount));
    
    public override string ToString() => 
        $"Obj.Observable<{typeof(A).Name}>";
            
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
*/

internal sealed record TryObj<X, A>(Obj<A> Value, Func<Exception, X> Catch) : Obj<CoProduct<X, A>>
{
    public override Prim<CoProduct<X, A>> Interpret<RT>(State<RT> state)
    {
        try
        {
            return Value.Interpret(state).Map(CoProduct.Right<X, A>);
        }
        catch (Exception e)
        {
            return Prim.Pure(CoProduct.Left<X, A>(Catch(e)));
        }
    }
    
    public override string ToString() => 
        $"{Value}.ToUnit";
}

internal sealed record ToObj<A>(Obj<CoProduct<Error, A>> Object) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        Object.Interpret(state).Bind(Go);

    static Prim<A> Go(CoProduct<Error, A> value) =>
        value switch
        {
            PurePrim<CoProduct<Error, A>> c =>
                c.Value switch
                {
                    CoProductRight<Error, A> r => Prim.Pure<A>(r.Value),
                    CoProductLeft<Error, A> l => Prim.Fail<A>(l.Value),
                    CoProductFail<Error, A> f => Prim.Fail<A>(f.Value),
                    _ => throw new NotSupportedException()
                },

            ManyPrim<CoProduct<Error, A>> cs =>
                cs.Bind(Go),

            FailPrim<CoProduct<Error, A>> f => Prim.Fail<A>(f.Value),

            _ => throw new NotSupportedException()
        };
}
