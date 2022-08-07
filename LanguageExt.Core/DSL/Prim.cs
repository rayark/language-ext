#nullable enable

using System;
using System.Reactive.Linq;
using System.Threading;
using LanguageExt.TypeClasses;

namespace LanguageExt.DSL;

/*
public static class PrimAny
{
    public static DSL<MError, Error>.Prim<A> Fail<A>(Error value) =>
        new DSL<MError, Error>.LeftPrim<A>(value);

}
*/

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

    public static Prim<A> Observable<A>(IObservable<Prim<A>> ma) =>
        new ObservablePrim<A>(ma);
    
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
    public abstract Unit Iter(Func<A, Unit> f);

    public abstract Prim<A> Head { get; }
    public abstract Prim<A> Last { get; }
    public abstract Prim<A> Tail { get; }
    public abstract Prim<A> Skip(int amount);
    public abstract Prim<A> Take(int amount);
    public abstract bool ForAll(Func<A, bool> f);
    public abstract bool Exists(Func<A, bool> f);

    public abstract bool IsNone { get; }
    public abstract bool IsMany { get; }
    public abstract bool IsSucc { get; }

    /// <summary>
    /// Force evaluation and collect nothing
    /// </summary>
    public abstract Result<Unit> ToUnit();
    
    /// <summary>
    /// Force evaluation and collect the values
    /// </summary>
    public abstract Result<A> ToResult();

    /// <summary>
    /// Dispose
    /// </summary>
    public abstract void Dispose();
}

internal sealed record PurePrim<A>(A Value) : Prim<A>
{
    public override Prim<B> Bind<B>(Func<A, Prim<B>> f) =>
        f(Value);

    public override Prim<B> Map<B>(Func<A, B> f) =>
        new PurePrim<B>(f(Value));

    public override Unit Iter(Func<A, Unit> f)
    {
        f(Value);
        return default;
    }

    public override bool ForAll(Func<A, bool> f) => 
        f(Value);
    
    public override bool Exists(Func<A, bool> f) => 
        f(Value);
 
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
    /// Make a concrete result
    /// </summary>
    public override Result<A> ToResult() =>
        Result.Pure(Value);

    /// <summary>
    /// Force evaluation and collect nothing
    /// </summary>
    public override Result<Unit> ToUnit() =>
        Result.Pure<Unit>(default);

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

internal sealed record ManyPrim<A>(Seq<Prim<A>> Items) : Prim<A>
{
    public override Prim<B> Bind<B>(Func<A, Prim<B>> f) =>
        Prim.Many(Items.Map(x => x.Bind(f)));

    public override Prim<B> Map<B>(Func<A, B> f) =>
        Prim.Many(Items.Map(x => x.Map(f)));

    public override Unit Iter(Func<A, Unit> f)
    {
        Items.Iter(x => x.Iter(f));
        return default;
    }

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

    public override bool IsNone => Items.ForAll(x => x.IsNone);
    public override bool IsMany => Items.Exists(x => x.IsSucc);
    public override bool IsSucc => Items.ForAll(x => x.IsSucc);

    /// <summary>
    /// Force evaluation and collect nothing
    /// </summary>
    public override Result<Unit> ToUnit() =>
        Items.Map(static x => x.ToUnit()).Concat();

    /// <summary>
    /// Make a concrete result
    /// </summary>
    public override Result<A> ToResult() =>
        Items.Map(static x => x.ToResult()).Concat();

    /// <summary>
    /// Dispose
    /// </summary>
    public override void Dispose() =>
        Items.Iter(x => x.Dispose());

    public override string ToString() => 
        $"{Items}";
}

public sealed record ObservablePrim<A>(IObservable<Prim<A>> Items) : Prim<A>
{
    public override Prim<B> Bind<B>(Func<A, Prim<B>> f) =>
        new ObservablePrim<B>(Items.Select(px => px.Bind(f)));

    public override Prim<B> Map<B>(Func<A, B> f) =>
        new ObservablePrim<B>(Items.Select(px => px.Map(f)));

    public override Unit Iter(Func<A, Unit> f) =>
        default;
    
    public override Prim<A> Head =>
        new ObservablePrim<A>(Items.Take(1));

    public override Prim<A> Tail =>
        new ObservablePrim<A>(Items.Skip(1));

    public override Prim<A> Skip(int amount) =>
        new ObservablePrim<A>(Items.Skip(amount));

    public override Prim<A> Take(int amount) =>
        new ObservablePrim<A>(Items.Take(amount));

    public override Prim<A> Last =>
        new ObservablePrim<A>(Items.TakeLast(1));

    public override bool ForAll(Func<A, bool> f) => 
        Items.ForAll(x => x.ForAll(f));
    
    public override bool Exists(Func<A, bool> f) => 
        Items.Exists(x => x.Exists(f));
 
    public override bool IsNone => false;
    public override bool IsMany => true;
    public override bool IsSucc => true;

    /// <summary>
    /// Evaluate without collecting
    /// </summary>
    public override Result<Unit> ToUnit()
    {
        using var consumer = new Consumer();
        using var sub = Items.Subscribe(consumer);
        consumer.Wait.WaitOne();
        return consumer.Value;
    }        

    /// <summary>
    /// Make a concrete result
    /// </summary>
    public override Result<A> ToResult()
    {
        using var collect = new Collector();
        using var sub = Items.Subscribe(collect);
        collect.Wait.WaitOne();
        return collect.Value;
    }
    
    /// <summary>
    /// Dispose
    /// </summary>
    public override void Dispose()
    {
        // Nothing to dispose
    }

    public override string ToString() => 
        $"Prim.Observable<{typeof(A).Name}>";

    class Collector : IObserver<Prim<A>>, IDisposable
    {
        public readonly AutoResetEvent Wait = new(false);
        public Result<A> Value = Result<A>.None;

        public void OnNext(Prim<A> value)
        {
            Value = Value.Append(value.ToResult());
            if (Value.IsFail) Wait.Set();
        }            

        public void OnCompleted() =>
            Wait.Set();

        public void OnError(Exception error)
        {
            Value = Value.Append(new ResultFail<A>(default(MErr).Convert(error)));
            Wait.Set();
        }

        public void Dispose() =>
            Wait.Dispose();
    }
            
    class Consumer : IObserver<Prim<A>>, IDisposable
    {
        public readonly AutoResetEvent Wait = new(false);
        public Result<Unit> Value = Result.Pure<Unit>(default);

        public void OnNext(Prim<A> value)
        {
            var u = value.ToUnit();
            if (u.IsFail)
            {
                Value = u;
                Wait.Set();
            }
        }

        public void OnCompleted() =>
            Wait.Set();

        public void OnError(Exception error)
        {
            Value = Result.Fail<Unit>(default(MErr).Convert(error));
            Wait.Set();
        }

        public void Dispose() =>
            Wait.Dispose();
    }
}
