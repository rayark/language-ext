#nullable enable
using System;
using System.Threading.Tasks;

namespace LanguageExt.DSL2;

record SelectManyTransducer1<E, A, B, C>(
    Transducer<E, A> MA, 
    Transducer<A, Transducer<E, B>> Bind,
    Transducer<A, Transducer<B, C>> Project) :
    Transducer<E, C>
{
    public Func<TState, S, E, TResult<S>> Transform<S>(Func<TState, S, C, TResult<S>> reduce) =>
        (st, s, e) =>
            MA.Transform<S>((st1, s1, a) =>
                Bind.Transform<S>((st2, s2, teb) =>
                    teb.Transform<S>((st3, s3, b) =>
                        Project.Transform<S>((st4, s4, tbc) =>
                            tbc.Transform(reduce)(st4, s4, b))(st3, s3, a))(st2, s2, e))(st1, s1, a))(st, s, e);

    public TransducerAsync<E, C> ToAsync() => 
        new SelectManyTransducerAsyncSync1<E, A, B, C>(MA.ToAsync(), Bind.ToAsync(), Project.ToAsync());
}

record SelectManyTransducerAsyncSync1<E, A, B, C>(
    TransducerAsync<E, A> MA, 
    TransducerAsync<A, Transducer<E, B>> Bind,
    TransducerAsync<A, Transducer<B, C>> Project) :
    TransducerAsync<E, C>
{
    public Func<TState, S, E, ValueTask<TResult<S>>> TransformAsync<S>(Func<TState, S, C, ValueTask<TResult<S>>> reduce) =>
        (st, s, e) =>
            MA.TransformAsync<S>((st1, s1, a) =>
                Bind.TransformAsync<S>((st2, s2, teb) =>
                    teb.ToAsync().TransformAsync<S>((st3, s3, b) =>
                        Project.TransformAsync<S>((st4, s4, tbc) =>
                            tbc.ToAsync().TransformAsync(reduce)(st4, s4, b))(st3, s3, a))(st2, s2, e))(st1, s1, a))(st, s, e);
}

record SelectManyTransducerAsync1<E, A, B, C>(
    TransducerAsync<E, A> MA, 
    TransducerAsync<A, TransducerAsync<E, B>> Bind,
    TransducerAsync<A, TransducerAsync<B, C>> Project) :
    TransducerAsync<E, C>
{
    public Func<TState, S, E, ValueTask<TResult<S>>> TransformAsync<S>(Func<TState, S, C, ValueTask<TResult<S>>> reduce) =>
        (st, s, e) =>
            MA.TransformAsync<S>((st1, s1, a) =>
                Bind.TransformAsync<S>((st2, s2, teb) =>
                    teb.TransformAsync<S>((st3, s3, b) =>
                        Project.TransformAsync<S>((st4, s4, tbc) =>
                            tbc.TransformAsync(reduce)(st4, s4, b))(st3, s3, a))(st2, s2, e))(st1, s1, a))(st, s, e);
}

record SelectManyTransducer2<E, A, B, C>(
    Transducer<E, A> MA, 
    Transducer<A, Transducer<E, B>> Bind,
    Func<A, B, C> Project) :
    Transducer<E, C>
{
    public Func<TState, S, E, TResult<S>> Transform<S>(Func<TState, S, C, TResult<S>> reduce) =>
        (st, s, e) =>
            MA.Transform<S>((st1, s1, a) =>
                Bind.Transform<S>((st2, s2, teb) =>
                    teb.Transform<S>((st3, s3, b) =>
                        reduce(st3, s3, Project(a, b)))(st2, s2, e))(st1, s1, a))(st, s, e);

    public TransducerAsync<E, C> ToAsync() =>
        new SelectManyTransducerAsyncSync2<E, A, B, C>(MA.ToAsync(), Bind.ToAsync(), Project);
}

record SelectManyTransducerAsyncSync2<E, A, B, C>(
    TransducerAsync<E, A> MA, 
    TransducerAsync<A, Transducer<E, B>> Bind,
    Func<A, B, C> Project) :
    TransducerAsync<E, C>
{
    public Func<TState, S, E, ValueTask<TResult<S>>>
        TransformAsync<S>(Func<TState, S, C, ValueTask<TResult<S>>> reduce) =>
        (st, s, e) =>
            MA.TransformAsync<S>((st1, s1, a) =>
                Bind.TransformAsync<S>((st2, s2, teb) =>
                    teb.ToAsync().TransformAsync<S>((st3, s3, b) =>
                        reduce(st3, s3, Project(a, b)))(st2, s2, e))(st1, s1, a))(st, s, e);
}

record SelectManyTransducerAsync2<E, A, B, C>(
    TransducerAsync<E, A> MA, 
    TransducerAsync<A, TransducerAsync<E, B>> Bind,
    Func<A, B, C> Project) :
    TransducerAsync<E, C>
{
    public Func<TState, S, E, ValueTask<TResult<S>>>
        TransformAsync<S>(Func<TState, S, C, ValueTask<TResult<S>>> reduce) =>
        (st, s, e) =>
            MA.TransformAsync<S>((st1, s1, a) =>
                Bind.TransformAsync<S>((st2, s2, teb) =>
                    teb.TransformAsync<S>((st3, s3, b) =>
                        reduce(st3, s3, Project(a, b)))(st2, s2, e))(st1, s1, a))(st, s, e);
}

record SelectManyTransducer3<E, A, B, C>(
    Transducer<E, A> MA, 
    Func<A, Transducer<E, B>> Bind,
    Func<A, B, C> Project) :
    Transducer<E, C>
{
    public Func<TState, S, E, TResult<S>> Transform<S>(Func<TState, S, C, TResult<S>> reduce) =>
        (st, s, e) =>
            MA.Transform<S>((st1, s1, a) =>
                Bind(a).Transform<S>((st2, s2, b) =>
                    reduce(st2, s2, Project(a, b)))(st1, s1, e))(st, s, e);

    public TransducerAsync<E, C> ToAsync() =>
        new SelectManyTransducerAsync3<E, A, B, C>(MA.ToAsync(), a => Bind(a).ToAsync(), Project);
}

record SelectManyTransducerAsync3<E, A, B, C>(
    TransducerAsync<E, A> MA, 
    Func<A, TransducerAsync<E, B>> Bind,
    Func<A, B, C> Project) :
    TransducerAsync<E, C>
{
    public Func<TState, S, E, ValueTask<TResult<S>>>
        TransformAsync<S>(Func<TState, S, C, ValueTask<TResult<S>>> reduce) =>
        (st, s, e) =>
            MA.TransformAsync<S>((st1, s1, a) =>
                Bind(a).TransformAsync<S>((st2, s2, b) =>
                    reduce(st2, s2, Project(a, b)))(st1, s1, e))(st, s, e);
}

record SelectManySumTransducer1<X, E, A, B, C>(
    SumTransducer<X, X, E, A> MA, 
    SumTransducer<X, X, A, SumTransducer<X, X, E, B>> Bind,
    SumTransducer<X, X, A, SumTransducer<X, X, B, C>> Project) :
    SumTransducer<X, X, E, C>
{
    public Func<TState, S, Sum<X, E>, TResult<S>> BiTransform<S>(Func<TState, S, X, TResult<S>> Left,
        Func<TState, S, C, TResult<S>> Right) =>
        (st, s, e) =>
            MA.BiTransform(Right: (st1, s1, a) =>
                        Bind.BiTransform(
                            Right: (st2, s2, teb) =>
                                teb.BiTransform(
                                    Right: (st3, s3, b) =>
                                        Project.BiTransform(
                                                Right: (st4, s4, tbc) =>
                                                    tbc.BiTransform(Left, Right)(st4, s4, Sum<X, B>.Right(b)),
                                                Left: Left)
                                            (st3, s3, Sum<X, A>.Right(a)),
                                    Left: Left)(st2, s2, e),
                            Left: Left)(st1, s1, Sum<X, A>.Right(a)),
                    Left: Left)
                (st, s, e);

    public Func<TState, S, E, TResult<S>> TransformRight<S>(Func<TState, S, C, TResult<S>> reduceRight) =>
        SumTransducerDefault<X, X, E, C>.TransformRight(this, reduceRight);

    public Func<TState, S, X, TResult<S>> TransformLeft<S>(Func<TState, S, X, TResult<S>> reduceLeft) => 
        SumTransducerDefault<X, X, E, C>.TransformLeft(this, reduceLeft);

    public SumTransducerAsync<X, X, E, C> ToSumAsync() => 
        new SelectManySumTransducerAsyncSync1<X, E, A, B, C>(MA.ToSumAsync(), Bind.ToSumAsync(), Project.ToSumAsync());
}

record SelectManySumTransducerAsyncSync1<X, E, A, B, C>(
    SumTransducerAsync<X, X, E, A> MA, 
    SumTransducerAsync<X, X, A, SumTransducer<X, X, E, B>> Bind,
    SumTransducerAsync<X, X, A, SumTransducer<X, X, B, C>> Project) :
    SumTransducerAsync<X, X, E, C>
{
    public Func<TState, S, Sum<X, E>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> Left,
        Func<TState, S, C, ValueTask<TResult<S>>> Right) =>
        (st, s, e) =>
            MA.BiTransformAsync(Right: (st1, s1, a) =>
                        Bind.BiTransformAsync(
                            Right: (st2, s2, teb) =>
                                teb.ToSumAsync().BiTransformAsync(
                                    Right: (st3, s3, b) =>
                                        Project.BiTransformAsync(
                                                Right: (st4, s4, tbc) =>
                                                    tbc.ToSumAsync().BiTransformAsync(Left, Right)
                                                        (st4, s4, Sum<X, B>.Right(b)),
                                                Left: Left)
                                            (st3, s3, Sum<X, A>.Right(a)),
                                    Left: Left)(st2, s2, e),
                            Left: Left)(st1, s1, Sum<X, A>.Right(a)),
                    Left: Left)
                (st, s, e);

    public Func<TState, S, E, ValueTask<TResult<S>>> TransformRightAsync<S>(Func<TState, S, C, ValueTask<TResult<S>>> reduceRight) =>
        SumTransducerAsyncDefault<X, X, E, C>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(Func<TState, S, X, ValueTask<TResult<S>>> reduceLeft) => 
        SumTransducerAsyncDefault<X, X, E, C>.TransformLeftAsync(this, reduceLeft);
}

record SelectManySumTransducerAsync1<X, E, A, B, C>(
    SumTransducerAsync<X, X, E, A> MA, 
    SumTransducerAsync<X, X, A, SumTransducerAsync<X, X, E, B>> Bind,
    SumTransducerAsync<X, X, A, SumTransducerAsync<X, X, B, C>> Project) :
    SumTransducerAsync<X, X, E, C>
{
    public Func<TState, S, Sum<X, E>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> Left,
        Func<TState, S, C, ValueTask<TResult<S>>> Right) =>
        (st, s, e) =>
            MA.BiTransformAsync(Right: (st1, s1, a) =>
                        Bind.BiTransformAsync(
                            Right: (st2, s2, teb) =>
                                teb.BiTransformAsync(
                                    Right: (st3, s3, b) =>
                                        Project.BiTransformAsync(
                                                Right: (st4, s4, tbc) =>
                                                    tbc.BiTransformAsync(Left, Right)(st4, s4, Sum<X, B>.Right(b)),
                                                Left: Left)
                                            (st3, s3, Sum<X, A>.Right(a)),
                                    Left: Left)(st2, s2, e),
                            Left: Left)(st1, s1, Sum<X, A>.Right(a)),
                    Left: Left)
                (st, s, e);

    public Func<TState, S, E, ValueTask<TResult<S>>> TransformRightAsync<S>(Func<TState, S, C, ValueTask<TResult<S>>> reduceRight) =>
        SumTransducerAsyncDefault<X, X, E, C>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(Func<TState, S, X, ValueTask<TResult<S>>> reduceLeft) => 
        SumTransducerAsyncDefault<X, X, E, C>.TransformLeftAsync(this, reduceLeft);
}

record SelectManySumTransducer2<X, E, A, B, C>(
    SumTransducer<X, X, E, A> MA, 
    SumTransducer<X, X, A, SumTransducer<X, X, E, B>> Bind,
    Func<A, B, C> Project) :
    SumTransducer<X, X, E, C>
{
    public Func<TState, S, Sum<X, E>, TResult<S>> BiTransform<S>(Func<TState, S, X, TResult<S>> Left,
        Func<TState, S, C, TResult<S>> Right) =>
        (st, s, e) =>
            MA.BiTransform(Right: (st1, s1, a) =>
                        Bind.BiTransform(
                            Right: (st2, s2, teb) =>
                                teb.BiTransform(
                                    Right: (st3, s3, b) => Right(st3, s3, Project(a, b)),
                                    Left: Left)(st2, s2, e),
                            Left: Left)(st1, s1, Sum<X, A>.Right(a)),
                    Left: Left)
                (st, s, e);

    public Func<TState, S, E, TResult<S>> TransformRight<S>(Func<TState, S, C, TResult<S>> reduceRight) =>
        SumTransducerDefault<X, X, E, C>.TransformRight(this, reduceRight);

    public Func<TState, S, X, TResult<S>> TransformLeft<S>(Func<TState, S, X, TResult<S>> reduceLeft) => 
        SumTransducerDefault<X, X, E, C>.TransformLeft(this, reduceLeft);

    public SumTransducerAsync<X, X, E, C> ToSumAsync() => 
        new SelectManySumTransducerAsyncSync2<X, E, A, B, C>(MA.ToSumAsync(), Bind.ToSumAsync(), Project);
}

record SelectManySumTransducerAsyncSync2<X, E, A, B, C>(
    SumTransducerAsync<X, X, E, A> MA, 
    SumTransducerAsync<X, X, A, SumTransducer<X, X, E, B>> Bind,
    Func<A, B, C> Project) :
    SumTransducerAsync<X, X, E, C>
{
    public Func<TState, S, Sum<X, E>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> Left,
        Func<TState, S, C, ValueTask<TResult<S>>> Right) =>
        (st, s, e) =>
            MA.BiTransformAsync(Right: (st1, s1, a) =>
                        Bind.BiTransformAsync(
                            Right: (st2, s2, teb) =>
                                teb.ToSumAsync().BiTransformAsync(
                                    Right: (st3, s3, b) => Right(st3, s3, Project(a, b)),
                                    Left: Left)(st2, s2, e),
                            Left: Left)(st1, s1, Sum<X, A>.Right(a)),
                    Left: Left)
                (st, s, e);

    public Func<TState, S, E, ValueTask<TResult<S>>> TransformRightAsync<S>(Func<TState, S, C, ValueTask<TResult<S>>> reduceRight) =>
        SumTransducerAsyncDefault<X, X, E, C>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(Func<TState, S, X, ValueTask<TResult<S>>> reduceLeft) => 
        SumTransducerAsyncDefault<X, X, E, C>.TransformLeftAsync(this, reduceLeft);
}

record SelectManySumTransducerAsync2<X, E, A, B, C>(
    SumTransducerAsync<X, X, E, A> MA, 
    SumTransducerAsync<X, X, A, SumTransducerAsync<X, X, E, B>> Bind,
    Func<A, B, C> Project) :
    SumTransducerAsync<X, X, E, C>
{
    public Func<TState, S, Sum<X, E>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> Left,
        Func<TState, S, C, ValueTask<TResult<S>>> Right) =>
        (st, s, e) =>
            MA.BiTransformAsync(Right: (st1, s1, a) =>
                        Bind.BiTransformAsync(
                            Right: (st2, s2, teb) =>
                                teb.BiTransformAsync(
                                    Right: (st3, s3, b) => Right(st3, s3, Project(a, b)),
                                    Left: Left)(st2, s2, e),
                            Left: Left)(st1, s1, Sum<X, A>.Right(a)),
                    Left: Left)
                (st, s, e);

    public Func<TState, S, E, ValueTask<TResult<S>>> TransformRightAsync<S>(Func<TState, S, C, ValueTask<TResult<S>>> reduceRight) =>
        SumTransducerAsyncDefault<X, X, E, C>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(Func<TState, S, X, ValueTask<TResult<S>>> reduceLeft) => 
        SumTransducerAsyncDefault<X, X, E, C>.TransformLeftAsync(this, reduceLeft);
}

record SelectManySumTransducer3<X, E, A, B, C>(
    SumTransducer<X, X, E, A> MA, 
    Func<A, SumTransducer<X, X, E, B>> Bind,
    Func<A, B, C> Project) :
    SumTransducer<X, X, E, C>
{
    public Func<TState, S, Sum<X, E>, TResult<S>> BiTransform<S>(Func<TState, S, X, TResult<S>> Left,
        Func<TState, S, C, TResult<S>> Right) =>
        (st, s, e) =>
            MA.BiTransform(Right: (st1, s1, a) =>
                        Bind(a).BiTransform(
                            Right: (st3, s3, b) => Right(st3, s3, Project(a, b)),
                            Left: Left)(st1, s1, e),
                    Left: Left)
                (st, s, e);

    public Func<TState, S, E, TResult<S>> TransformRight<S>(Func<TState, S, C, TResult<S>> reduceRight) =>
        SumTransducerDefault<X, X, E, C>.TransformRight(this, reduceRight);

    public Func<TState, S, X, TResult<S>> TransformLeft<S>(Func<TState, S, X, TResult<S>> reduceLeft) => 
        SumTransducerDefault<X, X, E, C>.TransformLeft(this, reduceLeft);

    public SumTransducerAsync<X, X, E, C> ToSumAsync() => 
        new SelectManySumTransducerAsyncSync3<X, E, A, B, C>(MA.ToSumAsync(), Bind, Project);
}

record SelectManySumTransducerAsyncSync3<X, E, A, B, C>(
    SumTransducerAsync<X, X, E, A> MA, 
    Func<A, SumTransducer<X, X, E, B>> Bind,
    Func<A, B, C> Project) :
    SumTransducerAsync<X, X, E, C>
{
    public Func<TState, S, Sum<X, E>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> Left,
        Func<TState, S, C, ValueTask<TResult<S>>> Right) =>
        (st, s, e) =>
            MA.BiTransformAsync(
                    Right: (st1, s1, a) =>
                        Bind(a).ToSumAsync().BiTransformAsync(
                            Right: (st3, s3, b) => Right(st3, s3, Project(a, b)),
                            Left: Left)(st1, s1, e),
                    Left: Left)
                (st, s, e);

    public Func<TState, S, E, ValueTask<TResult<S>>> TransformRightAsync<S>(Func<TState, S, C, ValueTask<TResult<S>>> reduceRight) =>
        SumTransducerAsyncDefault<X, X, E, C>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(Func<TState, S, X, ValueTask<TResult<S>>> reduceLeft) => 
        SumTransducerAsyncDefault<X, X, E, C>.TransformLeftAsync(this, reduceLeft);
}

record SelectManySumTransducerAsync3<X, E, A, B, C>(
    SumTransducerAsync<X, X, E, A> MA, 
    Func<A, SumTransducerAsync<X, X, E, B>> Bind,
    Func<A, B, C> Project) :
    SumTransducerAsync<X, X, E, C>
{
    public Func<TState, S, Sum<X, E>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> Left,
        Func<TState, S, C, ValueTask<TResult<S>>> Right) =>
        (st, s, e) =>
            MA.BiTransformAsync(
                    Right: (st1, s1, a) =>
                        Bind(a).BiTransformAsync(
                            Right: (st3, s3, b) => Right(st3, s3, Project(a, b)),
                            Left: Left)(st1, s1, e),
                    Left: Left)
                (st, s, e);

    public Func<TState, S, E, ValueTask<TResult<S>>> TransformRightAsync<S>(Func<TState, S, C, ValueTask<TResult<S>>> reduceRight) =>
        SumTransducerAsyncDefault<X, X, E, C>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(Func<TState, S, X, ValueTask<TResult<S>>> reduceLeft) => 
        SumTransducerAsyncDefault<X, X, E, C>.TransformLeftAsync(this, reduceLeft);
}

record SelectManyProductTransducer1<X, E, A, B, C>(
    ProductTransducer<X, X, E, A> MA, 
    ProductTransducer<X, X, A, ProductTransducer<X, X, E, B>> Bind,
    ProductTransducer<X, X, A, ProductTransducer<X, X, B, C>> Project) :
    ProductTransducer<X, X, E, C>
{
    public Func<TState, S, (X, E), TResult<S>> BiTransform<S>(Func<TState, S, X, TResult<S>> First,
        Func<TState, S, C, TResult<S>> Second) =>
        (st, s, xe) =>
            MA.BiTransform(
                    First: First,
                    Second: (st1, s1, a) =>
                        Bind.BiTransform(
                            First: First,
                            Second: (st2, s2, teb) =>
                                teb.BiTransform(
                                    First: First,
                                    Second: (st3, s3, b) =>
                                        Project.BiTransform(
                                                First: First,
                                                Second: (st4, s4, tbc) =>
                                                    tbc.BiTransform(First, Second)(st4, s4, (xe.Item1, b)))
                                            (st3, s3, (xe.Item1, a)))(st2, s2, xe))(st1, s1, (xe.Item1, a)))(st, s, xe);

    public Func<TState, S, (X, E), TResult<S>> TransformSecond<S>(Func<TState, S, C, TResult<S>> reduceSecond) =>
        ProductTransducerDefault<X, X, E, C>.TransformSecond(this, reduceSecond);

    public Func<TState, S, (X, E), TResult<S>> TransformFirst<S>(Func<TState, S, X, TResult<S>> reduceFirst) => 
        ProductTransducerDefault<X, X, E, C>.TransformFirst(this, reduceFirst);

    public ProductTransducerAsync<X, X, E, C> ToProductAsync() => 
        new SelectManyProductTransducerAsyncSync1<X, E, A, B, C>(MA.ToProductAsync(), Bind.ToProductAsync(), Project.ToProductAsync());
}

record SelectManyProductTransducerAsyncSync1<X, E, A, B, C>(
    ProductTransducerAsync<X, X, E, A> MA, 
    ProductTransducerAsync<X, X, A, ProductTransducer<X, X, E, B>> Bind,
    ProductTransducerAsync<X, X, A, ProductTransducer<X, X, B, C>> Project) :
    ProductTransducerAsync<X, X, E, C>
{
    public Func<TState, S, (X, E), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> First,
        Func<TState, S, C, ValueTask<TResult<S>>> Second) =>
        (st, s, xe) =>
            MA.BiTransformAsync(
                First: First,
                Second: (st1, s1, a) =>
                    Bind.BiTransformAsync(
                        First: First,
                        Second: (st2, s2, teb) =>
                            teb.ToProductAsync().BiTransformAsync(
                                First: First,
                                Second: (st3, s3, b) =>
                                    Project.BiTransformAsync(
                                            First: First,
                                            Second: (st4, s4, tbc) =>
                                                tbc.ToProductAsync().BiTransformAsync(First, Second)
                                                    (st4, s4, (xe.Item1, b)))
                                        (st3, s3, (xe.Item1, a)))
                                (st2, s2, xe))
                        (st1, s1, (xe.Item1, a)))
                (st, s, xe);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformSecondAsync<S>(
        Func<TState, S, C, ValueTask<TResult<S>>> reduceSecond) =>
        ProductTransducerAsyncDefault<X, X, E, C>.TransformSecondAsync(this, reduceSecond);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformFirstAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> reduceFirst) => 
        ProductTransducerAsyncDefault<X, X, E, C>.TransformFirstAsync(this, reduceFirst);
}

record SelectManyProductTransducerAsync1<X, E, A, B, C>(
    ProductTransducerAsync<X, X, E, A> MA, 
    ProductTransducerAsync<X, X, A, ProductTransducerAsync<X, X, E, B>> Bind,
    ProductTransducerAsync<X, X, A, ProductTransducerAsync<X, X, B, C>> Project) :
    ProductTransducerAsync<X, X, E, C>
{
    public Func<TState, S, (X, E), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> First,
        Func<TState, S, C, ValueTask<TResult<S>>> Second) =>
        (st, s, xe) =>
            MA.BiTransformAsync(
                First: First,
                Second: (st1, s1, a) =>
                    Bind.BiTransformAsync(
                        First: First,
                        Second: (st2, s2, teb) =>
                            teb.BiTransformAsync(
                                First: First,
                                Second: (st3, s3, b) =>
                                    Project.BiTransformAsync(
                                            First: First,
                                            Second: (st4, s4, tbc) =>
                                                tbc.BiTransformAsync(First, Second)(st4, s4, (xe.Item1, b)))
                                        (st3, s3, (xe.Item1, a)))(st2, s2, xe))(st1, s1, (xe.Item1, a)))(st, s, xe);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformSecondAsync<S>(
        Func<TState, S, C, ValueTask<TResult<S>>> reduceSecond) =>
        ProductTransducerAsyncDefault<X, X, E, C>.TransformSecondAsync(this, reduceSecond);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformFirstAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> reduceFirst) => 
        ProductTransducerAsyncDefault<X, X, E, C>.TransformFirstAsync(this, reduceFirst);
}

record SelectManyProductTransducer2<X, E, A, B, C>(
    ProductTransducer<X, X, E, A> MA, 
    ProductTransducer<X, X, A, ProductTransducer<X, X, E, B>> Bind,
    Func<A, B, C> Project) :
    ProductTransducer<X, X, E, C>
{
    public Func<TState, S, (X, E), TResult<S>> BiTransform<S>(Func<TState, S, X, TResult<S>> First,
        Func<TState, S, C, TResult<S>> Second) =>
        (st, s, xe) =>
            MA.BiTransform(
                    First: First,
                    Second: (st1, s1, a) =>
                        Bind.BiTransform(
                            First: First,
                            Second: (st2, s2, teb) =>
                                teb.BiTransform(
                                    First: First,
                                    Second: (st3, s3, b) =>
                                        Second(st3, s3, Project(a, b)))
                                    (st2, s2, xe))(st1, s1, (xe.Item1, a)))(st, s, xe);

    public Func<TState, S, (X, E), TResult<S>> TransformSecond<S>(Func<TState, S, C, TResult<S>> reduceSecond) =>
        ProductTransducerDefault<X, X, E, C>.TransformSecond(this, reduceSecond);

    public Func<TState, S, (X, E), TResult<S>> TransformFirst<S>(Func<TState, S, X, TResult<S>> reduceFirst) => 
        ProductTransducerDefault<X, X, E, C>.TransformFirst(this, reduceFirst);

    public ProductTransducerAsync<X, X, E, C> ToProductAsync() => 
        new SelectManyProductTransducerAsyncSync2<X, E, A, B, C>(MA.ToProductAsync(), Bind.ToProductAsync(), Project);
}

record SelectManyProductTransducerAsyncSync2<X, E, A, B, C>(
    ProductTransducerAsync<X, X, E, A> MA, 
    ProductTransducerAsync<X, X, A, ProductTransducer<X, X, E, B>> Bind,
    Func<A, B, C> Project) :
    ProductTransducerAsync<X, X, E, C>
{
    public Func<TState, S, (X, E), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> First,
        Func<TState, S, C, ValueTask<TResult<S>>> Second) =>
        (st, s, xe) =>
            MA.BiTransformAsync(
                    First: First,
                    Second: (st1, s1, a) =>
                        Bind.BiTransformAsync(
                                First: First,
                                Second: (st2, s2, teb) =>
                                    teb.ToProductAsync().BiTransformAsync(
                                            First: First,
                                            Second: (st3, s3, b) =>
                                                Second(st3, s3, Project(a, b)))
                                        (st2, s2, xe))
                            (st1, s1, (xe.Item1, a)))
                (st, s, xe);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformSecondAsync<S>(
        Func<TState, S, C, ValueTask<TResult<S>>> reduceSecond) =>
        ProductTransducerAsyncDefault<X, X, E, C>.TransformSecondAsync(this, reduceSecond);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformFirstAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> reduceFirst) => 
        ProductTransducerAsyncDefault<X, X, E, C>.TransformFirstAsync(this, reduceFirst);
}

record SelectManyProductTransducerAsync2<X, E, A, B, C>(
    ProductTransducerAsync<X, X, E, A> MA, 
    ProductTransducerAsync<X, X, A, ProductTransducerAsync<X, X, E, B>> Bind,
    Func<A, B, C> Project) :
    ProductTransducerAsync<X, X, E, C>
{
    public Func<TState, S, (X, E), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> First,
        Func<TState, S, C, ValueTask<TResult<S>>> Second) =>
        (st, s, xe) =>
            MA.BiTransformAsync(
                First: First,
                Second: (st1, s1, a) =>
                    Bind.BiTransformAsync(
                        First: First,
                        Second: (st2, s2, teb) =>
                            teb.BiTransformAsync(
                                    First: First,
                                    Second: (st3, s3, b) => Second(st3, s3, Project(a, b)))
                                (st2, s2, xe))(st1, s1, (xe.Item1, a)))(st, s, xe);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformSecondAsync<S>(
        Func<TState, S, C, ValueTask<TResult<S>>> reduceSecond) =>
        ProductTransducerAsyncDefault<X, X, E, C>.TransformSecondAsync(this, reduceSecond);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformFirstAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> reduceFirst) => 
        ProductTransducerAsyncDefault<X, X, E, C>.TransformFirstAsync(this, reduceFirst);
}

record SelectManyProductTransducer3<X, E, A, B, C>(
    ProductTransducer<X, X, E, A> MA, 
    Func<A, ProductTransducer<X, X, E, B>> Bind,
    Func<A, B, C> Project) :
    ProductTransducer<X, X, E, C>
{
    public Func<TState, S, (X, E), TResult<S>> BiTransform<S>(Func<TState, S, X, TResult<S>> First,
        Func<TState, S, C, TResult<S>> Second) =>
        (st, s, xe) =>
            MA.BiTransform(
                First: First,
                Second: (st1, s1, a) =>
                    Bind(a).BiTransform(
                            First: First,
                            Second: (st3, s3, b) => Second(st3, s3, Project(a, b)))
                        (st1, s1, xe))(st, s, xe);

    public Func<TState, S, (X, E), TResult<S>> TransformSecond<S>(Func<TState, S, C, TResult<S>> reduceSecond) =>
        ProductTransducerDefault<X, X, E, C>.TransformSecond(this, reduceSecond);

    public Func<TState, S, (X, E), TResult<S>> TransformFirst<S>(Func<TState, S, X, TResult<S>> reduceFirst) => 
        ProductTransducerDefault<X, X, E, C>.TransformFirst(this, reduceFirst);

    public ProductTransducerAsync<X, X, E, C> ToProductAsync() => 
        new SelectManyProductTransducerAsyncSync3<X, E, A, B, C>(MA.ToProductAsync(), Bind, Project);
}

record SelectManyProductTransducerAsyncSync3<X, E, A, B, C>(
    ProductTransducerAsync<X, X, E, A> MA, 
    Func<A, ProductTransducer<X, X, E, B>> Bind,
    Func<A, B, C> Project) :
    ProductTransducerAsync<X, X, E, C>
{
    public Func<TState, S, (X, E), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> First,
        Func<TState, S, C, ValueTask<TResult<S>>> Second) =>
        (st, s, xe) =>
            MA.BiTransformAsync(
                First: First,
                Second: (st1, s1, a) =>
                    Bind(a).ToProductAsync().BiTransformAsync(
                            First: First,
                            Second: (st3, s3, b) => Second(st3, s3, Project(a, b)))
                        (st1, s1, xe))(st, s, xe);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformSecondAsync<S>(
        Func<TState, S, C, ValueTask<TResult<S>>> reduceSecond) =>
        ProductTransducerAsyncDefault<X, X, E, C>.TransformSecondAsync(this, reduceSecond);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformFirstAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> reduceFirst) => 
        ProductTransducerAsyncDefault<X, X, E, C>.TransformFirstAsync(this, reduceFirst);
}

record SelectManyProductTransducerAsync3<X, E, A, B, C>(
    ProductTransducerAsync<X, X, E, A> MA, 
    Func<A, ProductTransducerAsync<X, X, E, B>> Bind,
    Func<A, B, C> Project) :
    ProductTransducerAsync<X, X, E, C>
{
    public Func<TState, S, (X, E), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> First,
        Func<TState, S, C, ValueTask<TResult<S>>> Second) =>
        (st, s, xe) =>
            MA.BiTransformAsync(
                First: First,
                Second: (st1, s1, a) =>
                    Bind(a).BiTransformAsync(
                        First: First,
                        Second: (st3, s3, b) => 
                            Second(st3, s3, Project(a, b)))(st1, s1, xe))(st, s, xe);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformSecondAsync<S>(
        Func<TState, S, C, ValueTask<TResult<S>>> reduceSecond) =>
        ProductTransducerAsyncDefault<X, X, E, C>.TransformSecondAsync(this, reduceSecond);

    public Func<TState, S, (X, E), ValueTask<TResult<S>>> TransformFirstAsync<S>(
        Func<TState, S, X, ValueTask<TResult<S>>> reduceFirst) => 
        ProductTransducerAsyncDefault<X, X, E, C>.TransformFirstAsync(this, reduceFirst);
}
