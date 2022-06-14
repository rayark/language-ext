namespace LanguageExt.SourceGen.Parser;

internal static class Result
{
    /// <summary>
    /// Parse success with value
    /// </summary>
    /// <param name="state">State</param>
    /// <param name="value">Parsed</param>
    public static Result<A> Success<A>(State state, A value) =>
        new SuccessResult<A>(state, value);

    /// <summary>
    /// Fail with error
    /// </summary>
    /// <param name="state">State</param>
    /// <param name="error">Error</param>
    public static Result<A> Fail<A>(State state, Error error) =>
        new FailResult<A>(state, error);

    /// <summary>
    /// Parse success, but not value
    /// </summary>
    /// <param name="state">State</param>
    public static Result<A> Empty<A>(State state) =>
        new EmptyResult<A>(state);

    /// <summary>
    /// Parsing error
    /// </summary>
    /// <param name="state">State</param>
    /// <param name="unexpectedValue">unexpectedValue</param>
    public static Result<A> Unexpected<A>(State state, string unexpectedValue) =>
        new FailResult<A>(state, new UnexpectedError(unexpectedValue));

    /// <summary>
    /// Parsing error
    /// </summary>
    /// <param name="state">State</param>
    /// <param name="unexpectedValue">unexpectedValue</param>
    /// <param name="expectedValue">expectedValue</param>
    public static Result<A> Expected<A>(State state, string unexpectedValue, string expectedValue) =>
        new FailResult<A>(state, new ExpectedError(unexpectedValue, expectedValue));

    /// <summary>
    /// End of stream error
    /// </summary>
    public static Result<A> EOS<A>(State state, string expected) =>
        Expected<A>(state, "end-of-stream", expected);
    
    /// <summary>
    /// End of stream error
    /// </summary>
    public static Result<A> EOS<A>(State state) =>
        Unexpected<A>(state, "end-of-stream");
}
