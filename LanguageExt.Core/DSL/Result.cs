#nullable enable

using System;
using LanguageExt.TypeClasses;

namespace LanguageExt.DSL;

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

    public override string ToString() => 
        $"{Value}";
}

public record ResultPure<E, A>(A Value) : Result<E, A>
{
    public override bool IsFail => 
        false;
 
    public override Result<E, A> Append<SemigroupE>(Result<E, A> rhs) =>
        rhs switch
        {
            ResultFail<E, A> f                     => f,
            ResultPure<E, A> p                     => Result.Many<E, A>(LanguageExt.Prelude.Seq(Value, p.Value)),
            ResultMany<E, A> {Value.IsEmpty: true} => this,
            ResultMany<E, A> p                     => Result.Many<E, A>(Value.Cons(p.Value)),
            _                                      => throw new InvalidOperationException("Result shouldn't be extended")
        };

    public override string ToString() => 
        $"{Value}";
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

    public override string ToString() => 
        $"{Value}";
}
