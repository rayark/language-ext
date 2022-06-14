using System;
using System.Linq;
using System.Text;

namespace LanguageExt.SourceGen.Parser;

internal abstract record Result<A>(State State)
{
    public abstract A Value { get; }
    public abstract bool IsEmpty { get; }
    public abstract bool IsSuccess { get; }
    public bool IsFail => !IsSuccess;
    public abstract Result<B> Cast<B>();
    public abstract Result<B> Select<B>(Func<A, B> f);
    public Result<B> Map<B>(Func<A, B> f) => Select(f);
    public Result<A> Next() => this with { State = State.Next() };
}

internal record SuccessResult<A>(State State, A Return) : Result<A>(State)
{
    public override A Value =>
        Return;
 
    public override bool IsEmpty => 
        false;

    public override bool IsSuccess => 
        true;

    public override Result<B> Cast<B>() =>
        throw new InvalidOperationException("Can't cast a value success, only a empty-success or a failure");
 
    public override Result<B> Select<B>(Func<A, B> f) =>
        new SuccessResult<B>(State, f(Return));
}

internal record EmptyResult<A>(State State) : Result<A>(State)
{
    public override A Value =>
        throw new InvalidOperationException("No value");
 
    public override bool IsEmpty => 
        true;

    public override bool IsSuccess => 
        true;

    public override Result<B> Cast<B>() =>
        new EmptyResult<B>(State);
    
    public override Result<B> Select<B>(Func<A, B> f) =>
        new EmptyResult<B>(State);
}

internal record FailResult<A>(State State, Error Error) : Result<A>(State)
{
    public override A Value =>
        throw new InvalidOperationException("No value");
 
    public override bool IsEmpty => 
        true;

    public override bool IsSuccess => 
        false;

    public override Result<B> Cast<B>() =>
        new FailResult<B>(State, Error);

    public override Result<B> Select<B>(Func<A, B> f) => 
        new FailResult<B>(State, Error);
}
