#nullable enable
using System;
using System.Threading.Tasks;

namespace LanguageExt.DSL2;

record BindTransducer1<E, A, B>(
    Transducer<E, A> MA, 
    Transducer<A, Transducer<E, B>> Bind) :
    Transducer<E, B>
{
    public Func<TState, S, E, TResult<S>> Transform<S>(Func<TState, S, B, TResult<S>> reduce) =>
        (st, s, e) =>
            MA.Transform<S>((st1, s1, a) =>
                Bind.Transform<S>(
                    (st2, s2, teb) =>
                        teb.Transform(reduce)(st2, s2, e))(st1, s1, a))(st, s, e);

    public TransducerAsync<E, B> ToAsync() => 
        new BindTransducerAsyncSync1<E, A, B>(MA.ToAsync(), Bind.ToAsync());
}

record BindTransducerAsyncSync1<E, A, B>(
    TransducerAsync<E, A> MA, 
    TransducerAsync<A, Transducer<E, B>> Bind) :
    TransducerAsync<E, B>
{
    public Func<TState, S, E, ValueTask<TResult<S>>> TransformAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduce) =>
        (st, s, e) =>
            MA.TransformAsync<S>((st1, s1, a) =>
                Bind.TransformAsync<S>((st2, s2, teb) =>
                    teb.ToAsync().TransformAsync(reduce)(st2, s2, e))(st1, s1, a))(st, s, e);
}

record BindTransducerAsync1<E, A, B>(
    TransducerAsync<E, A> MA, 
    TransducerAsync<A, TransducerAsync<E, B>> Bind) :
    TransducerAsync<E, B>
{
    public Func<TState, S, E, ValueTask<TResult<S>>> TransformAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduce) =>
        (st, s, e) =>
            MA.TransformAsync<S>((st1, s1, a) =>
                Bind.TransformAsync<S>((st2, s2, teb) =>
                    teb.TransformAsync(reduce)(st2, s2, e))(st1, s1, a))(st, s, e);
}

record BindTransducer2<E, A, B>(
    Transducer<E, A> MA, 
    Func<A, Transducer<E, B>> Bind) :
    Transducer<E, B>
{
    public Func<TState, S, E, TResult<S>> Transform<S>(Func<TState, S, B, TResult<S>> reduce) =>
        (st, s, e) =>
            MA.Transform<S>((st1, s1, a) => Bind(a).Transform(reduce)(st1, s1, e))(st, s, e);

    public TransducerAsync<E, B> ToAsync() =>
        new BindTransducerAsync2<E, A, B>(MA.ToAsync(), a => Bind(a).ToAsync());
}

record BindTransducerAsync2<E, A, B>(
    TransducerAsync<E, A> MA, 
    Func<A, TransducerAsync<E, B>> Bind) :
    TransducerAsync<E, B>
{
    public Func<TState, S, E, ValueTask<TResult<S>>>
        TransformAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduce) =>
        (st, s, e) =>
            MA.TransformAsync<S>((st1, s1, a) =>
                Bind(a).TransformAsync(reduce)(st1, s1, e))(st, s, e);
}

record BindSumTransducer1<X, E, A, B>(
    SumTransducer<X, X, E, A> MA, 
    SumTransducer<X, X, A, SumTransducer<X, X, E, B>> Bind) :
    SumTransducer<X, X, E, B>
{
    public Func<TState, S, Sum<X, E>, TResult<S>> BiTransform<S>(Func<TState, S, X, TResult<S>> Left,
        Func<TState, S, B, TResult<S>> Right) =>
        (st, s, e) =>
            MA.BiTransform(Right: (st1, s1, a) =>
                        Bind.BiTransform(
                            Right: (st2, s2, teb) =>
                                teb.BiTransform(Left, Right)(st2, s2, e),
                            Left: Left)(st1, s1, Sum<X, A>.Right(a)),
                    Left: Left)
                (st, s, e);

    public Func<TState, S, E, TResult<S>> TransformRight<S>(Func<TState, S, B, TResult<S>> reduceRight) =>
        SumTransducerDefault<X, X, E, B>.TransformRight(this, reduceRight);

    public Func<TState, S, X, TResult<S>> TransformLeft<S>(Func<TState, S, X, TResult<S>> reduceLeft) => 
        SumTransducerDefault<X, X, E, B>.TransformLeft(this, reduceLeft);

    public SumTransducerAsync<X, X, E, B> ToSumAsync() => 
        new BindSumTransducerAsyncSync1<X, E, A, B>(MA.ToSumAsync(), Bind.ToSumAsync());
}

record BindSumTransducerAsyncSync1<X, E, A, B>(
    SumTransducerAsync<X, X, E, A> MA, 
    SumTransducerAsync<X, X, A, SumTransducer<X, X, E, B>> Bind) :
    SumTransducerAsync<X, X, E, B>
{
    public Func<TState, S, Sum<X, E>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> Left,
        Func<TState, S, B, ValueTask<TResult<S>>> Right) =>
        (st, s, e) =>
            MA.BiTransformAsync(Right: (st1, s1, a) =>
                        Bind.BiTransformAsync(
                            Right: (st2, s2, teb) =>
                                teb.ToSumAsync().BiTransformAsync(Left, Right)(st2, s2, e),
                            Left: Left)(st1, s1, Sum<X, A>.Right(a)),
                    Left: Left)
                (st, s, e);
    
    public Func<TState, S, E, ValueTask<TResult<S>>> TransformRightAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) =>
        SumTransducerAsyncDefault<X, X, E, B>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(Func<TState, S, X, ValueTask<TResult<S>>> reduceLeft) => 
        SumTransducerAsyncDefault<X, X, E, B>.TransformLeftAsync(this, reduceLeft);
}

record BindSumTransducerAsync1<X, E, A, B>(
    SumTransducerAsync<X, X, E, A> MA, 
    SumTransducerAsync<X, X, A, SumTransducerAsync<X, X, E, B>> Bind) :
    SumTransducerAsync<X, X, E, B>
{
    public Func<TState, S, Sum<X, E>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> Left,
        Func<TState, S, B, ValueTask<TResult<S>>> Right) =>
        (st, s, e) =>
            MA.BiTransformAsync(Right: (st1, s1, a) =>
                        Bind.BiTransformAsync(
                            Right: (st2, s2, teb) =>
                                teb.BiTransformAsync(Left, Right)(st2, s2, e),
                            Left: Left)(st1, s1, Sum<X, A>.Right(a)),
                    Left: Left)
                (st, s, e);

    public Func<TState, S, E, ValueTask<TResult<S>>> TransformRightAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) =>
        SumTransducerAsyncDefault<X, X, E, B>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(Func<TState, S, X, ValueTask<TResult<S>>> reduceLeft) => 
        SumTransducerAsyncDefault<X, X, E, B>.TransformLeftAsync(this, reduceLeft);
}

record BindSumTransducer2<X, E, A, B>(
    SumTransducer<X, X, E, A> MA, 
    Func<A, SumTransducer<X, X, E, B>> Bind) :
    SumTransducer<X, X, E, B>
{
    public Func<TState, S, Sum<X, E>, TResult<S>> BiTransform<S>(Func<TState, S, X, TResult<S>> Left,
        Func<TState, S, B, TResult<S>> Right) =>
        (st, s, e) =>
            MA.BiTransform(Right: (st1, s1, a) =>
                        Bind(a).BiTransform(Left, Right)(st1, s1, e),
                    Left: Left)
                (st, s, e);

    public Func<TState, S, E, TResult<S>> TransformRight<S>(Func<TState, S, B, TResult<S>> reduceRight) =>
        SumTransducerDefault<X, X, E, B>.TransformRight(this, reduceRight);

    public Func<TState, S, X, TResult<S>> TransformLeft<S>(Func<TState, S, X, TResult<S>> reduceLeft) => 
        SumTransducerDefault<X, X, E, B>.TransformLeft(this, reduceLeft);

    public SumTransducerAsync<X, X, E, B> ToSumAsync() => 
        new BindSumTransducerAsyncSync2<X, E, A, B>(MA.ToSumAsync(), Bind);
}

record BindSumTransducerAsyncSync2<X, E, A, B>(
    SumTransducerAsync<X, X, E, A> MA, 
    Func<A, SumTransducer<X, X, E, B>> Bind) :
    SumTransducerAsync<X, X, E, B>
{
    public Func<TState, S, Sum<X, E>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> Left,
        Func<TState, S, B, ValueTask<TResult<S>>> Right) =>
        (st, s, e) =>
            MA.BiTransformAsync(
                    Right: (st1, s1, a) =>
                        Bind(a).ToSumAsync().BiTransformAsync(Left, Right)(st1, s1, e),
                    Left: Left)
                (st, s, e);

    public Func<TState, S, E, ValueTask<TResult<S>>> TransformAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduce) =>
        SumTransducerAsyncDefault<X, X, E, B>.TransformRightAsync(this, reduce);

    public Func<TState, S, E, ValueTask<TResult<S>>> TransformRightAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) =>
        SumTransducerAsyncDefault<X, X, E, B>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(Func<TState, S, X, ValueTask<TResult<S>>> reduceLeft) => 
        SumTransducerAsyncDefault<X, X, E, B>.TransformLeftAsync(this, reduceLeft);
}

record BindSumTransducerAsync2<X, E, A, B>(
    SumTransducerAsync<X, X, E, A> MA, 
    Func<A, SumTransducerAsync<X, X, E, B>> Bind) :
    SumTransducerAsync<X, X, E, B>
{
    public Func<TState, S, Sum<X, E>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> Left,
        Func<TState, S, B, ValueTask<TResult<S>>> Right) =>
        (st, s, e) =>
            MA.BiTransformAsync(
                    Right: (st1, s1, a) =>
                        Bind(a).BiTransformAsync(Left, Right)(st1, s1, e),
                    Left: Left)
                (st, s, e);

    public Func<TState, S, E, ValueTask<TResult<S>>> TransformAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduce) =>
        SumTransducerAsyncDefault<X, X, E, B>.TransformRightAsync(this, reduce);

    public Func<TState, S, E, ValueTask<TResult<S>>> TransformRightAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) =>
        SumTransducerAsyncDefault<X, X, E, B>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(Func<TState, S, X, ValueTask<TResult<S>>> reduceLeft) => 
        SumTransducerAsyncDefault<X, X, E, B>.TransformLeftAsync(this, reduceLeft);
}

record BindProductTransducer1<X, E, A, B>(
    ProductTransducer<X, X, E, A> MA, 
    ProductTransducer<X, X, A, ProductTransducer<X, X, E, B>> Bind) :
    ProductTransducer<X, X, E, B>
{
    public Func<TState, S, (X, E), TResult<S>> BiTransform<S>(Func<TState, S, X, TResult<S>> First,
        Func<TState, S, B, TResult<S>> Second) =>
        (st, s, xe) =>
            MA.BiTransform(
                    First: First,
                    Second: (st1, s1, a) =>
                        Bind.BiTransform(
                            First: First,
                            Second: (st2, s2, teb) =>
                                teb.BiTransform(First, Second)(st2, s2, xe))(st1, s1, (xe.Item1, a)))(st, s, xe);

    public Func<TState, S, (X, E), TResult<S>> TransformSecond<S>(Func<TState, S, B, TResult<S>> reduceSecond) =>
        ProductTransducerDefault<X, X, E, B>.TransformSecond(this, reduceSecond);

    public Func<TState, S, (X, E), TResult<S>> TransformFirst<S>(Func<TState, S, X, TResult<S>> reduceFirst) => 
        ProductTransducerDefault<X, X, E, B>.TransformFirst(this, reduceFirst);

    public ProductTransducerAsync<X, X, E, B> ToProductAsync() => 
        new BindProductTransducerAsyncSync1<X, E, A, B>(MA.ToProductAsync(), Bind.ToProductAsync());
}

record BindProductTransducerAsyncSync1<X, E, A, B>(
    ProductTransducerAsync<X, X, E, A> MA, 
    ProductTransducerAsync<X, X, A, ProductTransducer<X, X, E, B>> Bind) :
    ProductTransducerAsync<X, X, E, B>
{
    public Func<TState, S, (X, E), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> First,
        Func<TState, S, B, ValueTask<TResult<S>>> Second) =>
        (st, s, xe) =>
            MA.BiTransformAsync(
                First: First,
                Second: (st1, s1, a) =>
                    Bind.BiTransformAsync(
                        First: First,
                        Second: (st2, s2, teb) =>
                            teb.ToProductAsync().BiTransformAsync(First, Second)(st2, s2, xe))
                        (st1, s1, (xe.Item1, a)))
                (st, s, xe);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformSecondAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduceSecond) =>
        ProductTransducerAsyncDefault<X, X, E, B>.TransformSecondAsync(this, reduceSecond);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformFirstAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> reduceFirst) => 
        ProductTransducerAsyncDefault<X, X, E, B>.TransformFirstAsync(this, reduceFirst);
}

record BindProductTransducerAsync1<X, E, A, B>(
    ProductTransducerAsync<X, X, E, A> MA, 
    ProductTransducerAsync<X, X, A, ProductTransducerAsync<X, X, E, B>> Bind) :
    ProductTransducerAsync<X, X, E, B>
{
    public Func<TState, S, (X, E), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> First,
        Func<TState, S, B, ValueTask<TResult<S>>> Second) =>
        (st, s, xe) =>
            MA.BiTransformAsync(
                First: First,
                Second: (st1, s1, a) =>
                    Bind.BiTransformAsync(
                        First: First,
                        Second: (st2, s2, teb) =>
                            teb.BiTransformAsync(First, Second)(st2, s2, xe))(st1, s1, (xe.Item1, a)))(st, s, xe);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformSecondAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduceSecond) =>
        ProductTransducerAsyncDefault<X, X, E, B>.TransformSecondAsync(this, reduceSecond);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformFirstAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> reduceFirst) => 
        ProductTransducerAsyncDefault<X, X, E, B>.TransformFirstAsync(this, reduceFirst);
}

record BindProductTransducer2<X, E, A, B>(
    ProductTransducer<X, X, E, A> MA, 
    Func<A, ProductTransducer<X, X, E, B>> Bind) :
    ProductTransducer<X, X, E, B>
{
    public Func<TState, S, (X, E), TResult<S>> BiTransform<S>(Func<TState, S, X, TResult<S>> First,
        Func<TState, S, B, TResult<S>> Second) =>
        (st, s, xe) =>
            MA.BiTransform(
                First: First,
                Second: (st1, s1, a) =>
                    Bind(a).BiTransform(First, Second)
                        (st1, s1, xe))(st, s, xe);

    public Func<TState, S, (X, E), TResult<S>> TransformSecond<S>(Func<TState, S, B, TResult<S>> reduceSecond) =>
        ProductTransducerDefault<X, X, E, B>.TransformSecond(this, reduceSecond);

    public Func<TState, S, (X, E), TResult<S>> TransformFirst<S>(Func<TState, S, X, TResult<S>> reduceFirst) => 
        ProductTransducerDefault<X, X, E, B>.TransformFirst(this, reduceFirst);

    public ProductTransducerAsync<X, X, E, B> ToProductAsync() => 
        new BindProductTransducerAsyncSync2<X, E, A, B>(MA.ToProductAsync(), Bind);
}

record BindProductTransducerAsyncSync2<X, E, A, B>(
    ProductTransducerAsync<X, X, E, A> MA, 
    Func<A, ProductTransducer<X, X, E, B>> Bind) :
    ProductTransducerAsync<X, X, E, B>
{
    public Func<TState, S, (X, E), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> First,
        Func<TState, S, B, ValueTask<TResult<S>>> Second) =>
        (st, s, xe) =>
            MA.BiTransformAsync(
                First: First,
                Second: (st1, s1, a) =>
                    Bind(a).ToProductAsync().BiTransformAsync(First, Second)
                        (st1, s1, xe))(st, s, xe);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformSecondAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduceSecond) =>
        ProductTransducerAsyncDefault<X, X, E, B>.TransformSecondAsync(this, reduceSecond);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformFirstAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> reduceFirst) => 
        ProductTransducerAsyncDefault<X, X, E, B>.TransformFirstAsync(this, reduceFirst);
}

record BindProductTransducerAsync2<X, E, A, B>(
    ProductTransducerAsync<X, X, E, A> MA, 
    Func<A, ProductTransducerAsync<X, X, E, B>> Bind) :
    ProductTransducerAsync<X, X, E, B>
{
    public Func<TState, S, (X, E), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> First,
        Func<TState, S, B, ValueTask<TResult<S>>> Second) =>
        (st, s, xe) =>
            MA.BiTransformAsync(
                First: First,
                Second: (st1, s1, a) =>
                    Bind(a).BiTransformAsync(First, Second)(st1, s1, xe))(st, s, xe);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformSecondAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduceSecond) =>
        ProductTransducerAsyncDefault<X, X, E, B>.TransformSecondAsync(this, reduceSecond);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformFirstAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> reduceFirst) => 
        ProductTransducerAsyncDefault<X, X, E, B>.TransformFirstAsync(this, reduceFirst);
}
