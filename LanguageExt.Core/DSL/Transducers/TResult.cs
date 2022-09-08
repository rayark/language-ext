using System;
using LanguageExt.Common;

namespace LanguageExt.DSL.Transducers;

public abstract record TResult<A>
{
    public abstract bool Complete { get; }
    public abstract bool Continue { get; }
    public abstract bool Faulted { get; }
    public abstract A ValueUnsafe { get; }
    public abstract Error ErrorUnsafe { get; }
    public abstract B Match<B>(Func<A, B> Complete, Func<A, B> Continue, Func<Error, B> Fail);
    public abstract TResult<B> Map<B>(Func<A, B> f);
}

public record TResultComplete<A>(A Value) : TResult<A>
{
    public override bool Complete => true;
    public override bool Continue => false;
    public override bool Faulted => false;
    public override A ValueUnsafe => Value;
    public override Error ErrorUnsafe => Errors.Bottom; 

    public override B Match<B>(Func<A, B> Complete, Func<A, B> Continue, Func<Error, B> Fail) =>
        Complete(Value);

    public override TResult<B> Map<B>(Func<A, B> f) =>
        new TResultComplete<B>(f(Value));
}

public sealed record TResultContinue<A>(A Value) : TResult<A>
{
    public override bool Complete => false;
    public override bool Continue => true;
    public override bool Faulted => false;
    public override A ValueUnsafe => Value;
    public override Error ErrorUnsafe => Errors.Bottom; 

    public override B Match<B>(Func<A, B> Complete, Func<A, B> Continue, Func<Error, B> Fail) =>
        Continue(Value);

    public override TResult<B> Map<B>(Func<A, B> f) =>
        new TResultContinue<B>(f(Value));
}

public sealed record TResultFail<A>(Error Error) : TResult<A>
{
    public override bool Complete => true;
    public override bool Continue => false;
    public override bool Faulted => true;
    public override A ValueUnsafe => Error.Throw<A>();
    public override Error ErrorUnsafe => Error; 

    public override B Match<B>(Func<A, B> Complete, Func<A, B> Continue, Func<Error, B> Fail) =>
        Fail(Error);

    public override TResult<B> Map<B>(Func<A, B> f) =>
        new TResultFail<B>(Error);
}

public sealed record TResultAlt<X, A>(X Alt) : TResult<A>
{
    public override bool Complete => true;
    public override bool Continue => false;
    public override bool Faulted => true;
    public override A ValueUnsafe => throw new InvalidOperationException();
    public override Error ErrorUnsafe => throw new InvalidOperationException(); 

    public override B Match<B>(Func<A, B> Complete, Func<A, B> Continue, Func<Error, B> Fail) =>
        throw new InvalidOperationException();

    public override TResult<B> Map<B>(Func<A, B> f) =>
        new TResultAlt<X, B>(Alt);
}

public static class TResult
{
    public static TResult<A> Complete<A>(A value) =>
        new TResultComplete<A>(value);
    
    public static TResult<A> Continue<A>(A value) =>
        new TResultContinue<A>(value);
    
    public static TResult<A> Fail<A>(Error value) =>
        new TResultFail<A>(value);
    
    public static TResult<A> Alt<X, A>(X value) =>
        new TResultAlt<X, A>(value);
}
