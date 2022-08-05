#nullable enable

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using LanguageExt.ClassInstances;
using LanguageExt.Common;
using LanguageExt.TypeClasses;

namespace LanguageExt.Core.DSL;

public static class PrimAny
{
    public static DSL<MError, Error>.Prim<A> Fail<A>(Error value) =>
        new DSL<MError, Error>.LeftPrim<A>(value);

    public static DSL<MErr, E>.Prim<A> Flatten<MErr, E, A>(
        this DSL<MErr, E>.Prim<DSL<MErr, E>.Prim<A>> mma)
        where MErr : struct, Semigroup<E>, Convertable<Exception, E> =>
        mma.Bind(Prelude.identity);
}

public static class Result
{
    public static Result<E, A> Fail<E, A>(E Value) =>
        new ResultFail<E, A>(Value);
    
    public static Result<E, A> Pure<E, A>(A Value) =>
        new ResultPure<E, A>(Value);

    public static Result<E, A> Many<E, A>(Seq<A> Value) =>
        Value.IsEmpty
            ? new ResultMany<E, A>(Value)
            : Value.Tail.IsEmpty
                ? new ResultPure<E, A>(Value.Head)
                : new ResultMany<E, A>(Value);

    public static Result<E, A> Concat<SemigroupE, E, A>(this Seq<Result<E, A>> xs)
        where SemigroupE : struct, Semigroup<E> =>
        xs.Fold(Result<E, A>.None, static (s, x) => s.Append<SemigroupE>(x));
}

public abstract record Result<E, A>
{
    public static readonly Result<E, A> None = new ResultMany<E, A>(Seq<A>.Empty);
    public abstract Result<E, A> Append<SemigroupE>(Result<E, A> rhs) where SemigroupE : struct, Semigroup<E>;
    public abstract bool IsFail { get; }
}

public record ResultFail<E, A>(E Value) : Result<E, A>
{
    public override bool IsFail => 
        true;
 
    public override Result<E, A> Append<SemigroupE>(Result<E, A> rhs) =>
        rhs is ResultFail<E, A> f
            ? Result.Fail<E, A>(default(SemigroupE).Append(Value, f.Value))
            : this;
}

public record ResultPure<E, A>(A Value) : Result<E, A>
{
    public override bool IsFail => 
        false;
 
    public override Result<E, A> Append<SemigroupE>(Result<E, A> rhs) =>
        rhs switch
        {
            ResultFail<E, A> f                     => f,
            ResultPure<E, A> p                     => Result.Many<E, A>(Prelude.Seq(Value, p.Value)),
            ResultMany<E, A> {Value.IsEmpty: true} => this,
            ResultMany<E, A> p                     => Result.Many<E, A>(Value.Cons(p.Value)),
            _                                      => throw new InvalidOperationException("Result shouldn't be extended")
        };
}

public record ResultMany<E, A>(Seq<A> Value) : Result<E, A>
{
    public override bool IsFail => 
        false;
 
    public override Result<E, A> Append<SemigroupE>(Result<E, A> rhs) =>
        rhs switch
        {
            _ when Value.IsEmpty => rhs,
            ResultFail<E, A> f   => f,
            ResultPure<E, A> p   => Result.Many<E, A>(Value.Add(p.Value)),
            ResultMany<E, A> p   => Result.Many<E, A>(Value + p.Value),
            _                    => throw new InvalidOperationException("Result shouldn't be extended")
        };
}

public static partial class DSL<MErr, E>
    where MErr : struct, Semigroup<E>, Convertable<Exception, E>
{
    public static class Prim
    {
        public static readonly Prim<Unit> Unit = Pure(Prelude.unit);

        public static Prim<A> Pure<A>(A value) =>
            new PurePrim<A>(value);

        public static Prim<A> Many<A>(Seq<Prim<A>> value) =>
            value.IsEmpty
                ? Prim<A>.None
                : value.Tail.IsEmpty
                    ? value.Head
                    : new ManyPrim<A>(value);
        
        public static Prim<A> Left<A>(E value) =>
            new LeftPrim<A>(value);

        public static Prim<A> Observable<A>(IObservable<Prim<A>> ma) =>
            new ObservablePrim<A>(ma);
    }

    public abstract record Prim<A> : Obj<A>, IDisposable
    {
        public static readonly Prim<A> None = new ManyPrim<A>(Seq<Prim<A>>.Empty);

        public abstract Prim<B> Bind<B>(Func<A, Prim<B>> f);
        public abstract Prim<B> Map<B>(Func<A, B> f);
        public abstract Unit Iter(Func<A, Unit> f);
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

        /// <summary>
        /// Force evaluation and collect nothing
        /// </summary>
        public abstract Result<E, Unit> ToUnit();
        
        /// <summary>
        /// Force evaluation and collect the values
        /// </summary>
        public abstract Result<E, A> ToResult();

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

        /// <summary>
        /// Make a concrete result
        /// </summary>
        public override Result<E, A> ToResult() =>
            Result.Pure<E, A>(Value);

        /// <summary>
        /// Force evaluation and collect nothing
        /// </summary>
        public override Result<E, Unit> ToUnit() =>
            Result.Pure<E, Unit>(default);

        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose()
        {
            if(Value is IDisposable d) d.Dispose();
        }
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

        public override Prim<A> Interpret<RT>(State<RT> state) =>
            this;

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
        public override bool IsFail => Items.Exists(x => x.IsFail);

        /// <summary>
        /// Force evaluation and collect nothing
        /// </summary>
        public override Result<E, Unit> ToUnit() =>
            Items.Map(static x => x.ToUnit()).Concat<MErr, E, Unit>();

        /// <summary>
        /// Make a concrete result
        /// </summary>
        public override Result<E, A> ToResult() =>
            Items.Map(static x => x.ToResult()).Concat<MErr, E, A>();

        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose() =>
            Items.Iter(x => x.Dispose());
    }

    internal sealed record LeftPrim<A>(E Value) : Prim<A>
    {
        public override Prim<B> Bind<B>(Func<A, Prim<B>> f) =>
            new LeftPrim<B>(Value);

        public override Prim<B> Map<B>(Func<A, B> f) =>
            new LeftPrim<B>(Value);

        public override Unit Iter(Func<A, Unit> f) =>
            default;

        public override Prim<A> Interpret<RT>(State<RT> state) =>
            this;

        public override Prim<B> Cast<B>() =>
            Prim.Left<B>(Value);

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

        /// <summary>
        /// Force evaluation and collect nothing
        /// </summary>
        public override Result<E, Unit> ToUnit() =>
            Result.Fail<E, Unit>(Value);
        
        /// <summary>
        /// Make a concrete result
        /// </summary>
        public override Result<E, A> ToResult() =>
            Result.Fail<E, A>(Value);

        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose()
        {
            if(Value is IDisposable d) d.Dispose();
        }
    }

    public sealed record ObservablePrim<A>(IObservable<Prim<A>> Items) : Prim<A>
    {
        public override Prim<B> Bind<B>(Func<A, Prim<B>> f) =>
            new ObservablePrim<B>(Items.Select(px => px.Bind(f)));

        public override Prim<B> Map<B>(Func<A, B> f) =>
            new ObservablePrim<B>(Items.Select(px => px.Map(f)));

        public override Unit Iter(Func<A, Unit> f) =>
            default;
        
        public override Prim<A> Interpret<RT>(State<RT> state) =>
            this;

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

        public override bool IsNone => false;
        public override bool IsMany => true;
        public override bool IsSucc => true;
        public override bool IsFail => false;

        /// <summary>
        /// Evaluate without collecting
        /// </summary>
        public override Result<E, Unit> ToUnit()
        {
            using var consumer = new Consumer();
            using var sub = Items.Subscribe(consumer);
            consumer.Wait.WaitOne();
            return consumer.Value;
        }        

        /// <summary>
        /// Make a concrete result
        /// </summary>
        public override Result<E, A> ToResult()
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

        class Collector : IObserver<Prim<A>>, IDisposable
        {
            public readonly AutoResetEvent Wait = new(false);
            public Result<E, A> Value = Result<E, A>.None;

            public void OnNext(Prim<A> value)
            {
                Value = Value.Append<MErr>(value.ToResult());
                if (Value.IsFail) Wait.Set();
            }            

            public void OnCompleted() =>
                Wait.Set();

            public void OnError(Exception error)
            {
                Value = Value.Append<MErr>(new ResultFail<E, A>(default(MErr).Convert(error)));
                Wait.Set();
            }

            public void Dispose() =>
                Wait.Dispose();
        }
                
        class Consumer : IObserver<Prim<A>>, IDisposable
        {
            public readonly AutoResetEvent Wait = new(false);
            public Result<E, Unit> Value = Result.Pure<E, Unit>(default);

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
                Value = Result.Fail<E, Unit>(default(MErr).Convert(error));
                Wait.Set();
            }

            public void Dispose() =>
                Wait.Dispose();
        }
    }
}
