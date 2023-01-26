#nullable enable
using System;
using System.Threading.Tasks;

namespace LanguageExt.DSL2;

record ActionTransducer1<E, A, B>(Transducer<E, A> FA, Transducer<E, B> FB) :
    Transducer<E, B>
{
    public Func<TState, S, E, TResult<S>> Transform<S>(Func<TState, S, B, TResult<S>> reduce) =>
        (st, s, e) =>
            FA.Transform<S>((st1, s1, _) =>
                FB.Transform(reduce)(st1, s1, e))(st, s, e);

    public TransducerAsync<E, B> ToAsync() =>
        new ActionTransducerAsync1<E, A, B>(FA.ToAsync(), FB.ToAsync());
}

record ActionTransducerAsync1<E, A, B>(TransducerAsync<E, A> FA, TransducerAsync<E, B> FB) :
    TransducerAsync<E, B>
{
    public Func<TState, S, E, ValueTask<TResult<S>>> TransformAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduce) =>
        (st, s, e) =>
            FA.TransformAsync<S>((st1, s1, _) =>
                FB.TransformAsync(reduce)(st1, s1, e))(st, s, e);
}

record ActionSumTransducer1<E, X, A, B>(SumTransducer<X, X, E, A> FA, SumTransducer<X, X, E, B> FB) :
    SumTransducer<X, X, E, B>
{
    public Func<TState, S, Sum<X, E>, TResult<S>> BiTransform<S>(
        Func<TState, S, X, TResult<S>> Left,
        Func<TState, S, B, TResult<S>> Right) =>
        (st, s, e) =>
            FA.BiTransform(
                Left: Left,
                Right: (st1, s1, _) => FB.BiTransform(Left, Right)(st1, s1, e))(st, s, e);

    public Func<TState, S, E, TResult<S>> TransformRight<S>(Func<TState, S, B, TResult<S>> reduceRight) => 
        SumTransducerDefault<X, X, E, B>.TransformRight(this, reduceRight);

    public Func<TState, S, X, TResult<S>> TransformLeft<S>(Func<TState, S, X, TResult<S>> reduceLeft) => 
        SumTransducerDefault<X, X, E, B>.TransformLeft(this, reduceLeft);

    public SumTransducerAsync<X, X, E, B> ToSumAsync() =>
        new ActionSumTransducerAsync1<E, X, A, B>(FA.ToSumAsync(), FB.ToSumAsync());
}

record ActionSumTransducerAsync1<E, X, A, B>(SumTransducerAsync<X, X, E, A> FA, SumTransducerAsync<X, X, E, B> FB) :
    SumTransducerAsync<X, X, E, B>
{
    public Func<TState, S, Sum<X, E>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> Left,
        Func<TState, S, B, ValueTask<TResult<S>>> Right) =>
        (st, s, e) =>
            FA.BiTransformAsync(
                Left: Left,
                Right: (st1, s1, _) => FB.BiTransformAsync(Left, Right)(st1, s1, e))(st, s, e);

    public Func<TState, S, E, ValueTask<TResult<S>>> TransformRightAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) => 
        SumTransducerAsyncDefault<X, X, E, B>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(Func<TState, S, X, ValueTask<TResult<S>>> reduceLeft) => 
        SumTransducerAsyncDefault<X, X, E, B>.TransformLeftAsync(this, reduceLeft);
}

record ActionProductTransducer1<E, X, A, B>(
    ProductTransducer<X, X, E, A> FA, 
    ProductTransducer<X, X, E, B> FB) :
    ProductTransducer<X, X, E, B>
{
    public Func<TState, S, (X, E), TResult<S>> BiTransform<S>(
        Func<TState, S, X, TResult<S>> First,
        Func<TState, S, B, TResult<S>> Second) =>
        (st, s, xe) =>
            FA.BiTransform(
                First: First,
                Second: (st1, s1, _) => FB.BiTransform(First, Second)(st1, s1, xe))(st, s, xe);

    public Func<TState, S, (X, E), TResult<S>> TransformSecond<S>(Func<TState, S, B, TResult<S>> reduceRight) => 
        ProductTransducerDefault<X, X, E, B>.TransformSecond(this, reduceRight);

    public Func<TState, S, (X, E), TResult<S>> TransformFirst<S>(Func<TState, S, X, TResult<S>> reduceLeft) => 
        ProductTransducerDefault<X, X, E, B>.TransformFirst(this, reduceLeft);

    public ProductTransducerAsync<X, X, E, B> ToProductAsync() =>
        new ActionProductTransducerAsync1<E, X, A, B>(FA.ToProductAsync(), FB.ToProductAsync());
}

record ActionProductTransducerAsync1<E, X, A, B>(
    ProductTransducerAsync<X, X, E, A> FA, 
    ProductTransducerAsync<X, X, E, B> FB) :
    ProductTransducerAsync<X, X, E, B>
{
    public Func<TState, S, (X, E), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> First,
        Func<TState, S, B, ValueTask<TResult<S>>> Second) =>
        (st, s, xe) =>
            FA.BiTransformAsync(
                First: First,
                Second: (st1, s1, _) => FB.BiTransformAsync(First, Second)(st1, s1, xe))(st, s, xe);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformSecondAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) => 
        ProductTransducerAsyncDefault<X, X, E, B>.TransformSecondAsync(this, reduceRight);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformFirstAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> reduceLeft) => 
        ProductTransducerAsyncDefault<X, X, E, B>.TransformFirstAsync(this, reduceLeft);
}
