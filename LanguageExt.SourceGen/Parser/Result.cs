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
    /// Parse success, but not value
    /// </summary>
    /// <param name="state">State</param>
    public static Result<A> Empty<A>(State state) =>
        new EmptyResult<A>(state);

    /// <summary>
    /// Parsing error
    /// </summary>
    /// <param name="state">State</param>
    /// <param name="message">Message</param>
    public static Result<A> Unexpected<A>(State state, string message) =>
        new UnexpectedResult<A>(state, message);
}
