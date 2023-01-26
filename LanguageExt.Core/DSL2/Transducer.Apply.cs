#nullable enable
using System;
using System.Threading.Tasks;

namespace LanguageExt.DSL2;

record ApplyTransducer1<E, A, B>(Transducer<E, Transducer<A, B>> FF, Transducer<E, A> FA) :
    Transducer<E, B>
{
    public Func<TState, S, E, TResult<S>> Transform<S>(Func<TState, S, B, TResult<S>> reduce) =>
        (st, s, e) =>
            FF.Transform<S>((st1, s1, f) =>
                FA.Transform<S>((st2, s2, a) =>
                    f.Transform(reduce)(st2, s2, a)
                )(st1, s1, e))(st, s, e);

    public TransducerAsync<E, B> ToAsync() =>
        new ApplyTransducerAsyncSync1<E, A, B>(FF.ToAsync(), FA.ToAsync());
}

record ApplyTransducer2<E, A, B>(Transducer<E, Func<A, B>> FF, Transducer<E, A> FA) :
    Transducer<E, B>
{
    public Func<TState, S, E, TResult<S>> Transform<S>(Func<TState, S, B, TResult<S>> reduce) =>
        (st, s, e) =>
            FF.Transform<S>((st1, s1, f) =>
                FA.Transform<S>((st2, s2, a) =>
                    reduce(st2, s2, f(a))
                )(st1, s1, e))(st, s, e);

    public TransducerAsync<E, B> ToAsync() =>
        new ApplyTransducerAsync2<E, A, B>(FF.ToAsync(), FA.ToAsync());
}

record ApplyTransducerAsync1<E, A, B>(TransducerAsync<E, TransducerAsync<A, B>> FF, TransducerAsync<E, A> FA) :
    TransducerAsync<E, B>
{
    public Func<TState, S, E, ValueTask<TResult<S>>> TransformAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduce) =>
        (st, s, e) =>
            FF.TransformAsync<S>((st1, s1, f) =>
                FA.TransformAsync<S>((st2, s2, a) =>
                    f.TransformAsync(reduce)(st2, s2, a)
                )(st1, s1, e))(st, s, e);
}

record ApplyTransducerAsyncSync1<E, A, B>(TransducerAsync<E, Transducer<A, B>> FF, TransducerAsync<E, A> FA) :
    TransducerAsync<E, B>
{
    public Func<TState, S, E, ValueTask<TResult<S>>> TransformAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduce) =>
        (st, s, e) =>
            FF.TransformAsync<S>((st1, s1, f) =>
                FA.TransformAsync<S>((st2, s2, a) =>
                    f.ToAsync().TransformAsync(reduce)(st2, s2, a)
                )(st1, s1, e))(st, s, e);
}

record ApplyTransducerAsync2<E, A, B>(TransducerAsync<E, Func<A, B>> FF, TransducerAsync<E, A> FA) :
    TransducerAsync<E, B>
{
    public Func<TState, S, E, ValueTask<TResult<S>>> TransformAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduce) =>
        (st, s, e) =>
            FF.TransformAsync<S>((st1, s1, f) =>
                FA.TransformAsync<S>((st2, s2, a) =>
                    reduce(st2, s2, f(a)))(st1, s1, e))(st, s, e);
}

record ApplySumTransducer1<E, X, A, B>(SumTransducer<X, X, E, Transducer<A, B>> FF, SumTransducer<X, X, E, A> FA) :
    SumTransducer<X, X, E, B>
{
    public Func<TState, S, Sum<X, E>, TResult<S>> BiTransform<S>(
        Func<TState, S, X, TResult<S>> Left,
        Func<TState, S, B, TResult<S>> Right) =>
        (st, s, xe) =>
            xe switch
            {
                SumRight<X, E> e =>
                    FF.BiTransform(
                        Left: Left,
                        Right: (st1, s1, f) => FA.BiTransform(
                            Left: Left,
                            Right: (st2, s2, a) => f.Transform(Right)(st2, s2, a)
                        )(st1, s1, e))(st, s, e),

                SumLeft<X, E> x =>
                    Left(st, s, x.Value),

                _ => TResult.Complete(s)
            };

    public Func<TState, S, E, TResult<S>> TransformRight<S>(Func<TState, S, B, TResult<S>> reduceRight) => 
        SumTransducerDefault<X, X, E, B>.TransformRight(this, reduceRight);

    public Func<TState, S, X, TResult<S>> TransformLeft<S>(Func<TState, S, X, TResult<S>> reduceLeft) => 
        SumTransducerDefault<X, X, E, B>.TransformLeft(this, reduceLeft);

    public SumTransducerAsync<X, X, E, B> ToSumAsync() =>
        new ApplySumTransducerAsyncSync1<E, X, A, B>(FF.ToSumAsync(), FA.ToSumAsync());
}

record ApplySumTransducer2<E, X, A, B>(SumTransducer<X, X, E, Func<A, B>> FF, SumTransducer<X, X, E, A> FA) :
    SumTransducer<X, X, E, B>
{
    public Func<TState, S, Sum<X, E>, TResult<S>> BiTransform<S>(
        Func<TState, S, X, TResult<S>> Left,
        Func<TState, S, B, TResult<S>> Right) =>
        (st, s, xe) =>
            xe switch
            {
                SumRight<X, E> e =>
                    FF.BiTransform(
                        Left: Left,
                        Right: (st1, s1, f) => FA.BiTransform(
                            Left: Left,
                            Right: (st2, s2, a) => Right(st2, s2, f(a))
                        )(st1, s1, e))(st, s, e),
                
                SumLeft<X, E> x =>
                    Left(st, s, x.Value),
                
                _ => TResult.Complete(s)
            };

    public Func<TState, S, E, TResult<S>> TransformRight<S>(Func<TState, S, B, TResult<S>> reduceRight) => 
        SumTransducerDefault<X, X, E, B>.TransformRight(this, reduceRight);

    public Func<TState, S, X, TResult<S>> TransformLeft<S>(Func<TState, S, X, TResult<S>> reduceLeft) => 
        SumTransducerDefault<X, X, E, B>.TransformLeft(this, reduceLeft);

    public SumTransducerAsync<X, X, E, B> ToSumAsync() =>
        new ApplySumTransducerAsync2<E, X, A, B>(FF.ToSumAsync(), FA.ToSumAsync());
}

record ApplySumTransducerAsync1<E, X, A, B>(SumTransducerAsync<X, X, E, TransducerAsync<A, B>> FF, SumTransducerAsync<X, X, E, A> FA) :
    SumTransducerAsync<X, X, E, B>
{
    public Func<TState, S, Sum<X, E>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> Left,
        Func<TState, S, B, ValueTask<TResult<S>>> Right) =>
        (st, s, xe) =>
            xe switch
            {
                SumRight<X, E> e =>
                    FF.BiTransformAsync(
                        Left: Left,
                        Right: (st1, s1, f) => FA.BiTransformAsync(
                            Left: Left,
                            Right: (st2, s2, a) => f.TransformAsync(Right)(st2, s2, a)
                        )(st1, s1, e))(st, s, e),
                
                SumLeft<X, E> x =>
                    Left(st, s, x.Value),
                
                _ => new ValueTask<TResult<S>>(TResult.Complete(s))
            };

    public Func<TState, S, E, ValueTask<TResult<S>>> TransformRightAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) => 
        SumTransducerAsyncDefault<X, X, E, B>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(Func<TState, S, X, ValueTask<TResult<S>>> reduceLeft) => 
        SumTransducerAsyncDefault<X, X, E, B>.TransformLeftAsync(this, reduceLeft);
}

record ApplySumTransducerAsyncSync1<E, X, A, B>(SumTransducerAsync<X, X, E, Transducer<A, B>> FF, SumTransducerAsync<X, X, E, A> FA) :
    SumTransducerAsync<X, X, E, B>
{
    public Func<TState, S, Sum<X, E>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> Left,
        Func<TState, S, B, ValueTask<TResult<S>>> Right) =>
        (st, s, xe) =>
            xe switch
            {
                SumRight<X, E> e =>
                    FF.BiTransformAsync(
                        Left: Left,
                        Right: (st1, s1, f) => FA.BiTransformAsync(
                            Left: Left,
                            Right: (st2, s2, a) => f.ToAsync().TransformAsync(Right)(st2, s2, a)
                        )(st1, s1, e))(st, s, e),
                
                SumLeft<X, E> x =>
                    Left(st, s, x.Value),
                
                _ => new ValueTask<TResult<S>>(TResult.Complete(s))
            };

    public Func<TState, S, E, ValueTask<TResult<S>>> TransformRightAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) => 
        SumTransducerAsyncDefault<X, X, E, B>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(Func<TState, S, X, ValueTask<TResult<S>>> reduceLeft) => 
        SumTransducerAsyncDefault<X, X, E, B>.TransformLeftAsync(this, reduceLeft);
}

record ApplySumTransducerAsync2<E, X, A, B>(
    SumTransducerAsync<X, X, E, Func<A, B>> FF, 
    SumTransducerAsync<X, X, E, A> FA) :
    SumTransducerAsync<X, X, E, B>
{
    public Func<TState, S, Sum<X, E>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> Left,
        Func<TState, S, B, ValueTask<TResult<S>>> Right) =>
        (st, s, xe) =>
            xe switch
            {
                SumRight<X, E> e =>
                    FF.BiTransformAsync(
                        Left: Left,
                        Right: (st1, s1, f) => FA.BiTransformAsync(
                            Left: Left,
                            Right: (st2, s2, a) => Right(st2, s2, f(a))
                        )(st1, s1, e))(st, s, e),

                SumLeft<X, E> x =>
                    Left(st, s, x.Value),

                _ => new ValueTask<TResult<S>>(TResult.Complete(s))
            };

    public Func<TState, S, E, ValueTask<TResult<S>>> TransformRightAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) => 
        SumTransducerAsyncDefault<X, X, E, B>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> reduceLeft) => 
        SumTransducerAsyncDefault<X, X, E, B>.TransformLeftAsync(this, reduceLeft);
}

record ApplyProductTransducer1<E, X, A, B>(
    ProductTransducer<X, X, E, Transducer<A, B>> FF, 
    ProductTransducer<X, X, E, A> FA) :
    ProductTransducer<X, X, E, B>
{
    public Func<TState, S, (X, E), TResult<S>> BiTransform<S>(
        Func<TState, S, X, TResult<S>> First,
        Func<TState, S, B, TResult<S>> Second) =>
        (st, s, xe) =>
            FF.BiTransform<S>(
                First: (st1, s1, x1) =>
                    FA.BiTransform<S>(
                        First: (st2, s2, x2) => First(st2, s2, x1).Bind(s3 => First(st2, s3, x2)),
                        Second: (st2, s2, _) => First(st2, s2, x1))(st1, s1, xe),
                Second: (st1, s1, tab) =>
                    FA.BiTransform(
                        First: First,
                        Second: (st2, s2, a) =>
                            tab.Transform(Second)(st2, s2, a))(st1, s1, xe))(st, s, xe);

    public Func<TState, S, (X, E), TResult<S>> TransformSecond<S>(Func<TState, S, B, TResult<S>> reduceRight) => 
        ProductTransducerDefault<X, X, E, B>.TransformSecond(this, reduceRight);

    public Func<TState, S, (X, E), TResult<S>> TransformFirst<S>(Func<TState, S, X, TResult<S>> reduceLeft) => 
        ProductTransducerDefault<X, X, E, B>.TransformFirst(this, reduceLeft);

    public ProductTransducerAsync<X, X, E, B> ToProductAsync() =>
        new ApplyProductTransducerAsyncSync1<E, X, A, B>(FF.ToProductAsync(), FA.ToProductAsync());
}


record ApplyProductTransducer2<E, X, A, B>(
    ProductTransducer<X, X, E, Func<A, B>> FF, 
    ProductTransducer<X, X, E, A> FA) :
    ProductTransducer<X, X, E, B>
{
    public Func<TState, S, (X, E), TResult<S>> BiTransform<S>(
        Func<TState, S, X, TResult<S>> First,
        Func<TState, S, B, TResult<S>> Second) =>
        (st, s, xe) =>
            FF.BiTransform<S>(
                First: (st1, s1, x1) =>
                    FA.BiTransform<S>(
                        First: (st2, s2, x2) => First(st2, s2, x1).Bind(s3 => First(st2, s3, x2)),
                        Second: (st2, s2, _) => First(st2, s2, x1))(st1, s1, xe),
                Second: (st1, s1, tab) =>
                    FA.BiTransform(
                        First: First,
                        Second: (st2, s2, a) =>
                            Second(st2, s2, tab(a)))(st1, s1, xe))(st, s, xe);

    public Func<TState, S, (X, E), TResult<S>> TransformSecond<S>(Func<TState, S, B, TResult<S>> reduceRight) => 
        ProductTransducerDefault<X, X, E, B>.TransformSecond(this, reduceRight);

    public Func<TState, S, (X, E), TResult<S>> TransformFirst<S>(Func<TState, S, X, TResult<S>> reduceLeft) => 
        ProductTransducerDefault<X, X, E, B>.TransformFirst(this, reduceLeft);

    public ProductTransducerAsync<X, X, E, B> ToProductAsync() =>
        new ApplyProductTransducerAsync2<E, X, A, B>(FF.ToProductAsync(), FA.ToProductAsync());
}

record ApplyProductTransducerAsyncSync1<E, X, A, B>(
    ProductTransducerAsync<X, X, E, Transducer<A, B>> FF, 
    ProductTransducerAsync<X, X, E, A> FA) :
    ProductTransducerAsync<X, X, E, B>
{
    public Func<TState, S, (X, E), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> First,
        Func<TState, S, B, ValueTask<TResult<S>>> Second) =>
        (st, s, xe) =>
            FF.BiTransformAsync<S>(
                First: (st1, s1, x1) =>
                    FA.BiTransformAsync<S>(
                        First: (st2, s2, x2) => First(st2, s2, x1),                 // TODO: Combine x1 with x2
                        Second: (st2, s2, _) => First(st2, s2, x1))(st1, s1, xe),
                Second: (st1, s1, tab) =>
                    FA.BiTransformAsync(
                        First: First,
                        Second: (st2, s2, a) =>
                            tab.ToAsync().TransformAsync(Second)(st2, s2, a))(st1, s1, xe))(st, s, xe);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformSecondAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) => 
        ProductTransducerAsyncDefault<X, X, E, B>.TransformSecondAsync(this, reduceRight);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformFirstAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> reduceLeft) => 
        ProductTransducerAsyncDefault<X, X, E, B>.TransformFirstAsync(this, reduceLeft);
}

record ApplyProductTransducerAsync1<E, X, A, B>(
    ProductTransducerAsync<X, X, E, TransducerAsync<A, B>> FF, 
    ProductTransducerAsync<X, X, E, A> FA) :
    ProductTransducerAsync<X, X, E, B>
{
    public Func<TState, S, (X, E), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> First,
        Func<TState, S, B, ValueTask<TResult<S>>> Second) =>
        (st, s, xe) =>
            FF.BiTransformAsync<S>(
                First: (st1, s1, x1) =>
                    FA.BiTransformAsync<S>(
                        First: (st2, s2, x2) => First(st2, s2, x1),                 // TODO: Combine x1 with x2 
                        Second: (st2, s2, _) => First(st2, s2, x1))(st1, s1, xe),
                Second: (st1, s1, tab) =>
                    FA.BiTransformAsync(
                        First: First,
                        Second: (st2, s2, a) =>
                            tab.TransformAsync(Second)(st2, s2, a))(st1, s1, xe))(st, s, xe);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformSecondAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) => 
        ProductTransducerAsyncDefault<X, X, E, B>.TransformSecondAsync(this, reduceRight);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformFirstAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> reduceLeft) => 
        ProductTransducerAsyncDefault<X, X, E, B>.TransformFirstAsync(this, reduceLeft);
}

record ApplyProductTransducerAsync2<E, X, A, B>(
    ProductTransducerAsync<X, X, E, Func<A, B>> FF, 
    ProductTransducerAsync<X, X, E, A> FA) :
    ProductTransducerAsync<X, X, E, B>
{
    public Func<TState, S, (X, E), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> First,
        Func<TState, S, B, ValueTask<TResult<S>>> Second) =>
        (st, s, xe) =>
            FF.BiTransformAsync<S>(
                First: (st1, s1, x1) =>
                    FA.BiTransformAsync<S>(
                        First: (st2, s2, x2) => First(st2, s2, x1),                 // TODO: Combine x1 with x2 
                        Second: (st2, s2, _) => First(st2, s2, x1))(st1, s1, xe),
                Second: (st1, s1, tab) =>
                    FA.BiTransformAsync(
                        First: First,
                        Second: (st2, s2, a) =>
                            Second(st2, s2, tab(a)))(st1, s1, xe))(st, s, xe);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformSecondAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) => 
        ProductTransducerAsyncDefault<X, X, E, B>.TransformSecondAsync(this, reduceRight);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformFirstAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> reduceLeft) => 
        ProductTransducerAsyncDefault<X, X, E, B>.TransformFirstAsync(this, reduceLeft);
}
