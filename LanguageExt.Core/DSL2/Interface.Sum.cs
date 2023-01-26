#nullable enable
using System;
using System.Threading.Tasks;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace LanguageExt.DSL2;

public interface SumTransducer<X, out Y, A, out B> //: Transducer<A, B>
{
    Func<TState, S, Sum<X, A>, TResult<S>> BiTransform<S>(
        Func<TState, S, Y, TResult<S>> Left, 
        Func<TState, S, B, TResult<S>> Right);

    Func<TState, S, A, TResult<S>> TransformRight<S>(Func<TState, S, B, TResult<S>> reduceRight);
    Func<TState, S, X, TResult<S>> TransformLeft<S>(Func<TState, S, Y, TResult<S>> reduceLeft);
    
    SumTransducerAsync<X, Y, A, B> ToSumAsync();    
}

public interface SumTransducerAsync<X, out Y, A, out B> //: TransducerAsync<A, B>
{
    Func<TState, S, Sum<X, A>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> Left, 
        Func<TState, S, B, ValueTask<TResult<S>>> Right);

    Func<TState, S, A, ValueTask<TResult<S>>> TransformRightAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduceRight);

    Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> reduceLeft);
}

public static class SumTransducerDefault<X, Y, A, B>
{
    public static Func<TState, S, X, TResult<S>> TransformLeft<S>(
        SumTransducer<X, Y, A, B> self,
        Func<TState, S, Y, TResult<S>> reduceLeft) =>
        (st, s, x) =>
            self.BiTransform(reduceLeft, static (_, s1, _) => TResult.Complete(s1))
                (st, s, SumRight<X, A>.Left(x));

    public static Func<TState, S, A, TResult<S>> TransformRight<S>(
        SumTransducer<X, Y, A, B> self,
        Func<TState, S, B, TResult<S>> reduceRight) =>
        (st, s, a) =>
            self.BiTransform(static (_, s1, _) => TResult.Complete(s1), reduceRight)
                (st, s, SumRight<X, A>.Right(a));
}

public static class SumTransducerAsyncDefault<X, Y, A, B>
{
    public static Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(
        SumTransducerAsync<X, Y, A, B> self,
        Func<TState, S, Y, ValueTask<TResult<S>>> reduceLeft) =>
        (st, s, x) =>
            self.BiTransformAsync(reduceLeft, static (_, s1, _) => new ValueTask<TResult<S>>(TResult.Complete(s1)))
                (st, s, SumRight<X, A>.Left(x));

    public static Func<TState, S, A, ValueTask<TResult<S>>> TransformRightAsync<S>(
        SumTransducerAsync<X, Y, A, B> self,
        Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) =>
        (st, s, a) =>
            self.BiTransformAsync(static (_, s1, _) => new ValueTask<TResult<S>>(TResult.Complete(s1)), reduceRight)
                (st, s, SumRight<X, A>.Right(a));
}
