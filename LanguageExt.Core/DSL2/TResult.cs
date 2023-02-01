#nullable enable
using System;
using System.Threading.Tasks;
using LanguageExt.Common;

namespace LanguageExt.DSL2;

public static class TResult
{
    public static TResult<A> Continue<A>(A value) => new TContinue<A>(value);
    public static TResult<A> Complete<A>(A value) => new TComplete<A>(value);
    public static TResult<A> Cancel<A>() => TCancelled<A>.Default;
    public static TResult<A> None<A>() => TNone<A>.Default;
    public static TResult<A> Fail<A>(Error Error) => new TFail<A>(Error);
}

public abstract record TResult<A>
{
    public abstract bool Success { get; }
    public abstract bool Continue { get; }
    public abstract bool Faulted { get; }
    public virtual A ValueUnsafe => throw new InvalidOperationException("Can't call ValueUnsafe on a TResult that has no value");
    public virtual Error ErrorUnsafe => throw new InvalidOperationException("Can't call ErrorUnsafe on a TResult that succeeded");
    public abstract TResult<B> Map<B>(Func<A, B> f);
    public abstract TResult<B> Bind<B>(Func<A, TResult<B>> f);
    public abstract ValueTask<TResult<B>> BindAsync<B>(Func<A, ValueTask<TResult<B>>> f);
}
public record TContinue<A>(A Value) : TResult<A>
{
    public override bool Success => true;
    public override bool Continue => true;
    public override bool Faulted => false;
    public override A ValueUnsafe => Value;

    public override TResult<B> Map<B>(Func<A, B> f) =>
        TResult.Continue(f(Value));

    public override TResult<B> Bind<B>(Func<A, TResult<B>> f) =>
        f(Value);

    public override ValueTask<TResult<B>> BindAsync<B>(Func<A, ValueTask<TResult<B>>> f) =>
        f(Value);
}
public record TComplete<A>(A Value) : TResult<A>
{
    public override bool Success => true;
    public override bool Continue => false;
    public override bool Faulted => false;
    public override A ValueUnsafe => Value;

    public override TResult<B> Map<B>(Func<A, B> f) =>
        TResult.Complete(f(Value));

    public override TResult<B> Bind<B>(Func<A, TResult<B>> f) =>
        f(Value);

    public override ValueTask<TResult<B>> BindAsync<B>(Func<A, ValueTask<TResult<B>>> f) =>
        f(Value);
}
public record TCancelled<A> : TResult<A>
{
    public static readonly TResult<A> Default = new TCancelled<A>();
    
    public override bool Success => false;
    public override bool Continue => false;
    public override bool Faulted => true;
    public override Error ErrorUnsafe => Errors.Cancelled;

    public override TResult<B> Map<B>(Func<A, B> _) =>
        TCancelled<B>.Default;

    public override TResult<B> Bind<B>(Func<A, TResult<B>> _) =>
        TCancelled<B>.Default;

    public override ValueTask<TResult<B>> BindAsync<B>(Func<A, ValueTask<TResult<B>>> f) =>
        new (TCancelled<B>.Default);
}
public record TNone<A> : TResult<A>
{
    public static readonly TResult<A> Default = new TNone<A>();
    
    public override bool Success => true;
    public override bool Continue => false;
    public override bool Faulted => false;

    public override TResult<B> Map<B>(Func<A, B> _) =>
        TNone<B>.Default;

    public override TResult<B> Bind<B>(Func<A, TResult<B>> _) =>
        TNone<B>.Default;

    public override ValueTask<TResult<B>> BindAsync<B>(Func<A, ValueTask<TResult<B>>> f) =>
        new (TNone<B>.Default);
}
public record TFail<A>(Error Error) : TResult<A>
{
    public override bool Success => false;
    public override bool Continue => false;
    public override bool Faulted => true;
    public override Error ErrorUnsafe => Error;

    public override TResult<B> Map<B>(Func<A, B> _) =>
        TResult.Fail<B>(Error);

    public override TResult<B> Bind<B>(Func<A, TResult<B>> _) =>
        TResult.Fail<B>(Error);

    public override ValueTask<TResult<B>> BindAsync<B>(Func<A, ValueTask<TResult<B>>> f) =>
        new (TResult.Fail<B>(Error));
}
