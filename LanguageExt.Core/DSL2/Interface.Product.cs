#nullable enable
using System;
using System.Threading.Tasks;

namespace LanguageExt.DSL2;

public interface ProductTransducer<X, out Y, A, out B>
{
    Func<TState, S, (X, A), TResult<S>> BiTransform<S>(
        Func<TState, S, Y, TResult<S>> First, 
        Func<TState, S, B, TResult<S>> Second);
 
    Func<TState, S, (X, A), TResult<S>> TransformFirst<S>(
        Func<TState, S, Y, TResult<S>> reduceFirst);
 
    Func<TState, S, (X, A), TResult<S>> TransformSecond<S>(
        Func<TState, S, B, TResult<S>> reduceSecond);
    
    ProductTransducerAsync<X, Y, A, B> ToProductAsync();
}

public interface ProductTransducerAsync<X, out Y, A, out B>
{
    Func<TState, S, (X, A), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> First, 
        Func<TState, S, B, ValueTask<TResult<S>>> Second);
 
    Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformFirstAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> reduceFirst);
 
    Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformSecondAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduceSecond);
}

public static class ProductTransducerDefault<X, Y, A, B>
{
    public static Func<TState, S, (X, A), TResult<S>> TransformFirst<S>(
        ProductTransducer<X, Y, A, B> self,
        Func<TState, S, Y, TResult<S>> reduceFirst) =>
        (st, s, pair) =>
            self.BiTransform(reduceFirst, static (_, s1, _) => TResult.Complete(s1))(st, s, pair);
 
    public static Func<TState, S, (X, A), TResult<S>> TransformSecond<S>(
        ProductTransducer<X, Y, A, B> self,
        Func<TState, S, B, TResult<S>> reduceSecond) =>
        (st, s, pair) =>
            self.BiTransform(static (_, s1, _) => TResult.Complete(s1), reduceSecond)(st, s, pair);
}

public static class ProductTransducerAsyncDefault<X, Y, A, B>
{
    public static Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformFirstAsync<S>(
        ProductTransducerAsync<X, Y, A, B> self,
        Func<TState, S, Y, ValueTask<TResult<S>>> reduceFirst) =>
        (st, s, pair) =>
            self.BiTransformAsync(
                reduceFirst, 
                static (_, s1, _) => new ValueTask<TResult<S>>(TResult.Complete(s1)))(st, s, pair);
 
    public static Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformSecondAsync<S>(
        ProductTransducerAsync<X, Y, A, B> self,
        Func<TState, S, B, ValueTask<TResult<S>>> reduceSecond) =>
        (st, s, pair) =>
            self.BiTransformAsync(
                static (_, s1, _) => new ValueTask<TResult<S>>(TResult.Complete(s1)), 
                reduceSecond)(st, s, pair);
}
