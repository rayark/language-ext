#nullable enable
using System;
using System.Threading.Tasks;

namespace LanguageExt.DSL2;

record ProductExpandTransducer<X, Y, A, B>(Transducer<(X, A), (Y, B)> Product) : ProductTransducer<X, Y, A, B>
{
    public Func<TState, S, (X, A), TResult<S>> BiTransform<S>(
        Func<TState, S, Y, TResult<S>> First,
        Func<TState, S, B, TResult<S>> Second) =>
        (st, s, xa) =>
            Product.Transform<S>((st1, s1, xa1) =>
                First(st1, s1, xa1.Item1) switch
                {
                    TContinue<S> s2 => Second(st1, s2.Value, xa1.Item2),
                    var s3 => s3
                })(st, s, xa);

    public Func<TState, S, (X, A), TResult<S>> TransformFirst<S>(Func<TState, S, Y, TResult<S>> reduceFirst) => 
        ProductTransducerDefault<X, Y, A, B>.TransformFirst(this, reduceFirst);

    public Func<TState, S, (X, A), TResult<S>> TransformSecond<S>(Func<TState, S, B, TResult<S>> reduceSecond) => 
        ProductTransducerDefault<X, Y, A, B>.TransformSecond(this, reduceSecond);

    public ProductTransducerAsync<X, Y, A, B> ToProductAsync() => 
        new ProductExpandTransducerAsync<X, Y, A, B>(Product.ToAsync());
}

record ProductExpandTransducerAsync<X, Y, A, B>(TransducerAsync<(X, A), (Y, B)> Product) : ProductTransducerAsync<X, Y, A, B>
{
    public Func<TState, S, (X, A), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> First,
        Func<TState, S, B, ValueTask<TResult<S>>> Second) =>
        (st, s, xa) =>
            Product.TransformAsync<S>(async (st1, s1, xa1) =>
                await First(st1, s1, xa1.Item1).ConfigureAwait(false) switch
                {
                    TContinue<S> s2 => await Second(st1, s2.Value, xa1.Item2).ConfigureAwait(false),
                    var s3 => s3
                })(st, s, xa);

    public Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformFirstAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> reduceFirst) => 
        ProductTransducerAsyncDefault<X, Y, A, B>.TransformFirstAsync(this, reduceFirst);

    public Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformSecondAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduceSecond) => 
        ProductTransducerAsyncDefault<X, Y, A, B>.TransformSecondAsync(this, reduceSecond);
}

record SumExpandTransducer<X, Y, A, B>(Transducer<Sum<X, A>, Sum<Y, B>> Sum) : SumTransducer<X, Y, A, B>
{
    public Func<TState, S, Sum<X, A>, TResult<S>> BiTransform<S>(
        Func<TState, S, Y, TResult<S>> Left,
        Func<TState, S, B, TResult<S>> Right) =>
        (st, s, xa) => Sum.Transform<S>(
            (st1, s1, yb) => yb switch
            {
                SumRight<Y, B> r => Right(st1, s1, r.Value),
                SumLeft<Y, B> l => Left(st1, s1, l.Value),
                _ => TResult.Complete(s1)
            })(st, s, xa);

    public Func<TState, S, A, TResult<S>> TransformRight<S>(
        Func<TState, S, B, TResult<S>> reduceRight) => 
        SumTransducerDefault<X, Y, A, B>.TransformRight(this, reduceRight);

    public Func<TState, S, X, TResult<S>> TransformLeft<S>(
        Func<TState, S, Y, TResult<S>> reduceLeft) => 
        SumTransducerDefault<X, Y, A, B>.TransformLeft(this, reduceLeft);

    public SumTransducerAsync<X, Y, A, B> ToSumAsync() =>
        new SumExpandTransducerAsync<X, Y, A, B>(Sum.ToAsync());
}

record SumExpandTransducerAsync<X, Y, A, B>(TransducerAsync<Sum<X, A>, Sum<Y, B>> Sum) : SumTransducerAsync<X, Y, A, B>
{
    public Func<TState, S, Sum<X, A>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> Left,
        Func<TState, S, B, ValueTask<TResult<S>>> Right) =>
        (st, s, xa) => Sum.TransformAsync<S>(
            (st1, s1, yb) => yb switch
            {
                SumRight<Y, B> r => Right(st1, s1, r.Value),
                SumLeft<Y, B> l => Left(st1, s1, l.Value),
                _ => new ValueTask<TResult<S>>(TResult.Complete(s1))
            })(st, s, xa);

    public Func<TState, S, A, ValueTask<TResult<S>>> TransformRightAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) =>
        SumTransducerAsyncDefault<X, Y, A, B>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> reduceLeft) =>
        SumTransducerAsyncDefault<X, Y, A, B>.TransformLeftAsync(this, reduceLeft);
}
