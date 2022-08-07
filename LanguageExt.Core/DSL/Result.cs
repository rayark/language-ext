#nullable enable

using System;

namespace LanguageExt.DSL;

public static class Result
{
    public static Result<A> Pure<A>(A Value) =>
        new ResultPure<A>(Value);

    public static Result<A> Many<A>(Seq<A> Value) =>
        Value.IsEmpty
            ? new ResultMany<A>(Value)
            : Value.Tail.IsEmpty
                ? new ResultPure<A>(Value.Head)
                : new ResultMany<A>(Value);

    public static Result<A> Concat<A>(this Seq<Result<A>> xs) => 
        xs.Fold(Result<A>.None, static (s, x) => s.Append(x));
}

public abstract record Result<A>
{
    public static readonly Result<A> None = new ResultMany<A>(Seq<A>.Empty);
    public abstract Result<A> Append(Result<A> rhs);
    public abstract bool IsFail { get; }
}

public record ResultPure<A>(A Value) : Result<A>
{
    public override bool IsFail => 
        false;
 
    public override Result<A> Append(Result<A> rhs) =>
        rhs switch
        {
            ResultPure<A> p                     => Result.Many(LanguageExt.Prelude.Seq(Value, p.Value)),
            ResultMany<A> {Value.IsEmpty: true} => this,
            ResultMany<A> p                     => Result.Many(Value.Cons(p.Value)),
            _                                   => throw new InvalidOperationException("Result shouldn't be extended")
        };

    public override string ToString() => 
        $"{Value}";
}

public record ResultMany<A>(Seq<A> Value) : Result<A>
{
    public override bool IsFail => 
        false;
 
    public override Result<A> Append(Result<A> rhs) =>
        rhs switch
        {
            _ when Value.IsEmpty => rhs,
            ResultPure<A> p   => Result.Many(Value.Add(p.Value)),
            ResultMany<A> p   => Result.Many(Value + p.Value),
            _                    => throw new InvalidOperationException("Result shouldn't be extended")
        };

    public override string ToString() => 
        $"{Value}";
}
