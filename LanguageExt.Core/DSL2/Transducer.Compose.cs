#nullable enable
using System;
using System.Threading.Tasks;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace LanguageExt.DSL2;

record ComposeTransducer<A, B, C>(Transducer<A, B> One, Transducer<B, C> Two)
    : Transducer<A, C>
{
    public Func<TState, S, A, TResult<S>> Transform<S>(Func<TState, S, C, TResult<S>> reduce) =>
        One.Transform(Two.Transform(reduce));

    public TransducerAsync<A, C> ToAsync() =>
        new ComposeTransducerAsync<A, B, C>(One.ToAsync(), Two.ToAsync());
}

record ComposeTransducerAsync<A, B, C>(TransducerAsync<A, B> One, TransducerAsync<B, C> Two)
    : TransducerAsync<A, C>
{
    public Func<TState, S, A, ValueTask<TResult<S>>> TransformAsync<S>(Func<TState, S, C, ValueTask<TResult<S>>> reduce) =>
        One.TransformAsync(Two.TransformAsync(reduce));
}

record ProductComposeTransducer<X, Y, Z, A, B, C>(
        ProductTransducer<X, Y, A, B> One,
        ProductTransducer<Y, Z, B, C> Two)
    : ProductTransducer<X, Z, A, C>
{
    public Func<TState, S, (X, A), TResult<S>> BiTransform<S>(
        Func<TState, S, Z, TResult<S>> First,
        Func<TState, S, C, TResult<S>> Second) =>
        (st, s, xa) =>
        {
            Y? ry = default;
            return One.BiTransform<S>(
                First: (_, s1, y) =>
                {
                    ry = y;
                    return TResult.Continue(s1);
                },
                Second: (st1, s1, b) =>
                    #nullable disable
                    Two.BiTransform(First, Second)(st1, s1, (ry, b)))(st, s, xa);
                    #nullable enable
        };

    public Func<TState, S, (X, A), TResult<S>> TransformFirst<S>(Func<TState, S, Z, TResult<S>> reduceFirst) => 
        ProductTransducerDefault<X, Z, A, C>.TransformFirst(this, reduceFirst);

    public Func<TState, S, (X, A), TResult<S>> TransformSecond<S>(Func<TState, S, C, TResult<S>> reduceSecond) => 
        ProductTransducerDefault<X, Z, A, C>.TransformSecond(this, reduceSecond);

    public ProductTransducerAsync<X, Z, A, C> ToProductAsync() =>
        new ProductComposeTransducerAsync<X, Y, Z, A, B, C>(One.ToProductAsync(), Two.ToProductAsync());
}

record ProductComposeTransducerAsync<X, Y, Z, A, B, C>(
        ProductTransducerAsync<X, Y, A, B> One,
        ProductTransducerAsync<Y, Z, B, C> Two)
    : ProductTransducerAsync<X, Z, A, C>
{
    public Func<TState, S, (X, A), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, Z, ValueTask<TResult<S>>> First,
        Func<TState, S, C, ValueTask<TResult<S>>> Second) =>
        (st, s, xa) =>
        {
            Y? ry = default;
            return One.BiTransformAsync<S>(
                First: (_, s1, y) =>
                {
                    ry = y;
                    return new ValueTask<TResult<S>>(TResult.Continue(s1));
                },
                Second: (st1, s1, b) =>
#nullable disable
                    Two.BiTransformAsync<S>(First, Second)(st1, s1, (ry, b)))(st, s, xa);
#nullable enable
        };

    public Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformFirstAsync<S>(Func<TState, S, Z, ValueTask<TResult<S>>> reduceFirst) => 
        ProductTransducerAsyncDefault<X, Z, A, C>.TransformFirstAsync(this, reduceFirst);

    public Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformSecondAsync<S>(Func<TState, S, C, ValueTask<TResult<S>>> reduceSecond) => 
        ProductTransducerAsyncDefault<X, Z, A, C>.TransformSecondAsync(this, reduceSecond);
}

record SumComposeTransducer<X, Y, Z, A, B, C>(
        SumTransducer<X, Y, A, B> One,
        SumTransducer<Y, Z, B, C> Two)
    : SumTransducer<X, Z, A, C>
{
    public Func<TState, S, Sum<X, A>, TResult<S>> BiTransform<S>(
        Func<TState, S, Z, TResult<S>> Left,
        Func<TState, S, C, TResult<S>> Right) =>
        (st, s, xa) => One.BiTransform<S>(
            Right: (st1, s1, b) =>
                Two.BiTransform(Left, Right)(st1, s1, Sum<Y, B>.Right(b)),
            Left: (st1, s1, y) =>
                Two.BiTransform(Left, Right)(st1, s1, Sum<Y, B>.Left(y)))(st, s, xa);

    public Func<TState, S, A, TResult<S>> TransformRight<S>(Func<TState, S, C, TResult<S>> reduceRight) =>
        SumTransducerDefault<X, Z, A, C>.TransformRight(this, reduceRight);

    public Func<TState, S, X, TResult<S>> TransformLeft<S>(Func<TState, S, Z, TResult<S>> reduceLeft) => 
        SumTransducerDefault<X, Z, A, C>.TransformLeft(this, reduceLeft);

    public SumTransducerAsync<X, Z, A, C> ToSumAsync() =>
        new SumComposeTransducerAsync<X, Y, Z, A, B, C>(One.ToSumAsync(), Two.ToSumAsync());
}

record SumComposeTransducerAsync<X, Y, Z, A, B, C>(
        SumTransducerAsync<X, Y, A, B> One,
        SumTransducerAsync<Y, Z, B, C> Two)
    : SumTransducerAsync<X, Z, A, C>
{
    public Func<TState, S, Sum<X, A>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, Z, ValueTask<TResult<S>>> Left,
        Func<TState, S, C, ValueTask<TResult<S>>> Right) =>
        (st, s, xa) => One.BiTransformAsync<S>(
            Right: (st1, s1, b) =>
                Two.BiTransformAsync(Left, Right)(st1, s1, Sum<Y, B>.Right(b)),
            Left: (st1, s1, y) =>
                Two.BiTransformAsync(Left, Right)(st1, s1, Sum<Y, B>.Left(y)))(st, s, xa);

    public Func<TState, S, A, ValueTask<TResult<S>>> TransformRightAsync<S>(
        Func<TState, S, C, ValueTask<TResult<S>>> reduceRight) =>
        SumTransducerAsyncDefault<X, Z, A, C>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(
        Func<TState, S, Z, ValueTask<TResult<S>>> reduceLeft) =>
        SumTransducerAsyncDefault<X, Z, A, C>.TransformLeftAsync(this, reduceLeft);
}
