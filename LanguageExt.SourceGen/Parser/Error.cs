using System;
using System.Linq;

namespace LanguageExt.SourceGen.Parser;

internal abstract record Error
{
    public static Error Unexpected(string unexpectedValue) =>
        new UnexpectedError(unexpectedValue);

    public static Error Expected(string unexpectedValue, string expectedValue) =>
        new ExpectedError(unexpectedValue, expectedValue);

    public static Error Many(params Error[] errors) =>
        new ManyErrors(Seq.From(errors));

    public static Error Many(Seq<Error> errors) =>
        new ManyErrors(errors);

    public static Error operator +(Error ex, Error ey) =>
        (ex, ey) switch
        {
            (ManyErrors mx, ManyErrors my) => new ManyErrors(mx.Errors + my.Errors),
            (ManyErrors mx, var my)        => new ManyErrors(mx.Errors.Add(my)),
            (var mx, ManyErrors my)        => new ManyErrors(mx.Cons(my.Errors)),
            var (mx, my)                   => new ManyErrors(Seq.From(mx, my))
        };
    
    public abstract string Message { get; }
}

internal record UnexpectedError(string UnexpectedValue) : Error
{
    public override string Message =>
        $"unexpected {UnexpectedValue}";
}

internal record ExpectedError(string UnexpectedValue, string ExpectedValue) : UnexpectedError(UnexpectedValue)
{
    public override string Message =>
        $"unexpected {UnexpectedValue}, expected {ExpectedValue}";
}

internal record ManyErrors(Seq<Error> Errors) : Error
{
    public override string Message =>
        Errors.All(e => e is ExpectedError)
            ? $"expected {FormatExpected(Errors)}"
            : Errors.First().Message;

    static string FormatExpected(Seq<Error> errors) =>
        String.Join(", ", errors.Select(e => (ExpectedError)e).SelectMany(e => e.ExpectedValue));
}
