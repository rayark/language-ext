namespace LanguageExt.SourceGen.Parser;

internal abstract record Result<A>(State State)
{
    public abstract bool IsSuccess { get; }
    public bool IsFail => !IsSuccess;
}

internal record SuccessResult<A>(State State, A Value) : Result<A>(State)
{
    public override bool IsSuccess => 
        true;
}

internal record EmptyResult<A>(State State) : Result<A>(State)
{
    public override bool IsSuccess => 
        true;
}

internal abstract record FailResult<A>(State State, string Message) : Result<A>(State)
{
    public override bool IsSuccess => 
        false;
}

internal record UnexpectedResult<A>(State State, string Message) : FailResult<A>(State, Message);
