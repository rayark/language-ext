#nullable enable
using System;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace LanguageExt.DSL2;

record FlattenTransducer<A, B>(Transducer<A, Transducer<A, B>> Nested) : Transducer<A, B>
{
    public Func<TState, S, A, TResult<S>> Transform<S>(Func<TState, S, B, TResult<S>> reduce) =>
        (st, s, a) => Nested.Transform<S>((st1, s1, r1) => r1.Transform(reduce)(st1, s1, a))(st, s, a);
    
    public TransducerAsync<A, B> ToAsync() =>
        new FlattenTransducerAsyncSync<A, B>(Nested.ToAsync());  // NOTE: This is expensive
}

record FlattenTransducerUnit<A, B>(Transducer<A, Transducer<Unit, B>> Nested) : Transducer<A, B>
{
    public Func<TState, S, A, TResult<S>> Transform<S>(Func<TState, S, B, TResult<S>> reduce) =>
        (st, s, xa) => Nested.Transform<S>((st1, s1, r1) => r1.Transform(reduce)(st1, s1, default))(st, s, xa);
    
    public TransducerAsync<A, B> ToAsync() =>
        new FlattenTransducerAsyncSyncUnit<A, B>(Nested.ToAsync());  // NOTE: This is expensive
}

record FlattenTransducerAsync<A, B>(TransducerAsync<A, TransducerAsync<A, B>> Nested) : TransducerAsync<A, B>
{
    public Func<TState, S, A, ValueTask<TResult<S>>> TransformAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduce) =>
        (st, s, xa) => Nested.TransformAsync<S>(
            (st1, s1, r1) => r1.TransformAsync(reduce)(st1, s1, xa))(st, s, xa);
}

record FlattenTransducerAsyncUnit<A, B>(TransducerAsync<A, TransducerAsync<Unit, B>> Nested) : TransducerAsync<A, B>
{
    public Func<TState, S, A, ValueTask<TResult<S>>> TransformAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduce) =>
        (st, s, a) => Nested.TransformAsync<S>(
            (st1, s1, r1) => r1.TransformAsync(reduce)(st1, s1, default))(st, s, a);
}

record FlattenTransducerAsyncSync<A, B>(TransducerAsync<A, Transducer<A, B>> Nested) : TransducerAsync<A, B>
{
    // NOTE: This is expensive due to the ToAsync
    
    public Func<TState, S, A, ValueTask<TResult<S>>> TransformAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduce) =>
        (st, s, a) => Nested.TransformAsync<S>(
            (st1, s1, r1) => r1.ToAsync().TransformAsync(reduce)(st1, s1, a))(st, s, a);
}

record FlattenTransducerAsyncSyncUnit<A, B>(TransducerAsync<A, Transducer<Unit, B>> Nested) : TransducerAsync<A, B>
{
    // NOTE: This is expensive due to the ToAsync
    
    public Func<TState, S, A, ValueTask<TResult<S>>> TransformAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduce) =>
        (st, s, a) => Nested.TransformAsync<S>(
            (st1, s1, r1) => r1.ToAsync().TransformAsync(reduce)(st1, s1, default))(st, s, a);
}

record SumFlattenTransducer<X, Y, A, B>(SumTransducer<X, Y, A, SumTransducer<X, Y, A, B>> Nested) : SumTransducer<X, Y, A, B>
{
    public Func<TState, S, Sum<X, A>, TResult<S>> BiTransform<S>(
        Func<TState, S, Y, TResult<S>> Left,
        Func<TState, S, B, TResult<S>> Right) =>
        (st, s, xa) => Nested.BiTransform(
            Right: (st1, s1, r1) => r1.BiTransform(Left, Right)(st1, s1, xa),
            Left: Left)(st, s, xa);

    public Func<TState, S, A, TResult<S>> TransformRight<S>(Func<TState, S, B, TResult<S>> reduceRight) => 
        SumTransducerDefault<X, Y, A, B>.TransformRight(this, reduceRight);

    public Func<TState, S, X, TResult<S>> TransformLeft<S>(Func<TState, S, Y, TResult<S>> reduceLeft) => 
        SumTransducerDefault<X, Y, A, B>.TransformLeft(this, reduceLeft);
    
    public SumTransducerAsync<X, Y, A, B> ToSumAsync() =>
        new SumFlattenTransducerAsyncSync<X, Y, A, B>(Nested.ToSumAsync());  // NOTE: This is expensive
}

record SumFlattenTransducerUnit<X, Y, A, B>(SumTransducer<X, Y, A, SumTransducer<X, Y, Unit, B>> Nested) : SumTransducer<X, Y, A, B>
{
    public Func<TState, S, Sum<X, A>, TResult<S>> BiTransform<S>(
        Func<TState, S, Y, TResult<S>> Left,
        Func<TState, S, B, TResult<S>> Right) =>
        (st, s, xa) => Nested.BiTransform(
            Right: (st1, s1, r1) => r1.BiTransform(Left, Right)(st1, s1, xa.Map(static _ => unit)),
            Left: Left)(st, s, xa);

    public Func<TState, S, A, TResult<S>> TransformRight<S>(Func<TState, S, B, TResult<S>> reduceRight) => 
        SumTransducerDefault<X, Y, A, B>.TransformRight(this, reduceRight);

    public Func<TState, S, X, TResult<S>> TransformLeft<S>(Func<TState, S, Y, TResult<S>> reduceLeft) => 
        SumTransducerDefault<X, Y, A, B>.TransformLeft(this, reduceLeft);
    
    public SumTransducerAsync<X, Y, A, B> ToSumAsync() =>
        new SumFlattenTransducerAsyncSyncUnit<X, Y, A, B>(Nested.ToSumAsync());  // NOTE: This is expensive
}

record SumFlattenTransducerAsync<X, Y, A, B>(SumTransducerAsync<X, Y, A, SumTransducerAsync<X, Y, A, B>> Nested) : SumTransducerAsync<X, Y, A, B>
{
    public Func<TState, S, Sum<X, A>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> Left,
        Func<TState, S, B, ValueTask<TResult<S>>> Right) =>
        (st, s, xa) => Nested.BiTransformAsync(
            Right: (st1, s1, r1) => r1.BiTransformAsync(Left, Right)(st1, s1, xa),
            Left: Left)(st, s, xa);

    public Func<TState, S, A, ValueTask<TResult<S>>> TransformRightAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) => 
        SumTransducerAsyncDefault<X, Y, A, B>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(Func<TState, S, Y, ValueTask<TResult<S>>> reduceLeft) => 
        SumTransducerAsyncDefault<X, Y, A, B>.TransformLeftAsync(this, reduceLeft);
}

record SumFlattenTransducerAsyncUnit<X, Y, A, B>(SumTransducerAsync<X, Y, A, SumTransducerAsync<X, Y, Unit, B>> Nested) : SumTransducerAsync<X, Y, A, B>
{
    public Func<TState, S, Sum<X, A>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> Left,
        Func<TState, S, B, ValueTask<TResult<S>>> Right) =>
        (st, s, xa) => Nested.BiTransformAsync(
            Right: (st1, s1, r1) => r1.BiTransformAsync(Left, Right)(st1, s1, xa.Map(static _ => unit)),
            Left: Left)(st, s, xa);

    public Func<TState, S, A, ValueTask<TResult<S>>> TransformRightAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) => 
        SumTransducerAsyncDefault<X, Y, A, B>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(Func<TState, S, Y, ValueTask<TResult<S>>> reduceLeft) => 
        SumTransducerAsyncDefault<X, Y, A, B>.TransformLeftAsync(this, reduceLeft);
}

record SumFlattenTransducerAsyncSync<X, Y, A, B>(SumTransducerAsync<X, Y, A, SumTransducer<X, Y, A, B>> Nested) : SumTransducerAsync<X, Y, A, B>
{
    // NOTE: This is expensive due to the ToSumAsync
    
    public Func<TState, S, Sum<X, A>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> Left,
        Func<TState, S, B, ValueTask<TResult<S>>> Right) =>
        (st, s, xa) => Nested.BiTransformAsync(
            Right: (st1, s1, r1) => r1.ToSumAsync().BiTransformAsync(Left, Right)(st1, s1, xa),
            Left: Left)(st, s, xa);

    public Func<TState, S, A, ValueTask<TResult<S>>> TransformRightAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) => 
        SumTransducerAsyncDefault<X, Y, A, B>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(Func<TState, S, Y, ValueTask<TResult<S>>> reduceLeft) => 
        SumTransducerAsyncDefault<X, Y, A, B>.TransformLeftAsync(this, reduceLeft);
}

record SumFlattenTransducerAsyncSyncUnit<X, Y, A, B>(SumTransducerAsync<X, Y, A, SumTransducer<X, Y, Unit, B>> Nested) : SumTransducerAsync<X, Y, A, B>
{
    // NOTE: This is expensive due to the ToSumAsync
    
    public Func<TState, S, Sum<X, A>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> Left,
        Func<TState, S, B, ValueTask<TResult<S>>> Right) =>
        (st, s, xa) => Nested.BiTransformAsync(
            Right: (st1, s1, r1) => r1.ToSumAsync().BiTransformAsync(Left, Right)(st1, s1, xa.Map(static _ => unit)),
            Left: Left)(st, s, xa);

    public Func<TState, S, A, ValueTask<TResult<S>>> TransformRightAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) => 
        SumTransducerAsyncDefault<X, Y, A, B>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(Func<TState, S, Y, ValueTask<TResult<S>>> reduceLeft) => 
        SumTransducerAsyncDefault<X, Y, A, B>.TransformLeftAsync(this, reduceLeft);
}

record ProductFlattenTransducer<X, Y, A, B>(ProductTransducer<X, Y, A, ProductTransducer<X, Y, A, B>> Nested) 
    : ProductTransducer<X, Y, A, B>
{
    public Func<TState, S, (X, A), TResult<S>> BiTransform<S>(
        Func<TState, S, Y, TResult<S>> First,
        Func<TState, S, B, TResult<S>> Second) =>
        (st, s, xa) => Nested.BiTransform(
            First:  First,
            Second: (st1, s1, tx) => tx.BiTransform(First, Second)(st1, s1, xa))(st, s, xa);

    public Func<TState, S, (X, A), TResult<S>> TransformSecond<S>(Func<TState, S, B, TResult<S>> reduceRight) => 
        ProductTransducerDefault<X, Y, A, B>.TransformSecond(this, reduceRight);

    public Func<TState, S, (X, A), TResult<S>> TransformFirst<S>(Func<TState, S, Y, TResult<S>> reduceLeft) => 
        ProductTransducerDefault<X, Y, A, B>.TransformFirst(this, reduceLeft);

    public ProductTransducerAsync<X, Y, A, B> ToProductAsync() =>
        new ProductFlattenTransducerAsyncSync<X, Y, A, B>(Nested.ToProductAsync());  // NOTE: This is expensive
}

record ProductFlattenTransducerUnit<X, Y, A, B>(ProductTransducer<X, Y, A, ProductTransducer<X, Y, Unit, B>> Nested) 
    : ProductTransducer<X, Y, A, B>
{
    public Func<TState, S, (X, A), TResult<S>> BiTransform<S>(
        Func<TState, S, Y, TResult<S>> First,
        Func<TState, S, B, TResult<S>> Second) =>
        (st, s, xa) => Nested.BiTransform(
            First:  First,
            Second: (st1, s1, tx) => tx.BiTransform(First, Second)(st1, s1, (xa.Item1, unit)))(st, s, xa);

    public Func<TState, S, (X, A), TResult<S>> TransformSecond<S>(Func<TState, S, B, TResult<S>> reduceRight) => 
        ProductTransducerDefault<X, Y, A, B>.TransformSecond(this, reduceRight);

    public Func<TState, S, (X, A), TResult<S>> TransformFirst<S>(Func<TState, S, Y, TResult<S>> reduceLeft) => 
        ProductTransducerDefault<X, Y, A, B>.TransformFirst(this, reduceLeft);

    public ProductTransducerAsync<X, Y, A, B> ToProductAsync() =>
        new ProductFlattenTransducerAsyncSyncUnit<X, Y, A, B>(Nested.ToProductAsync());  // NOTE: This is expensive
}

record ProductFlattenTransducerAsync<X, Y, A, B>(ProductTransducerAsync<X, Y, A, ProductTransducerAsync<X, Y, A, B>> Nested) 
    : ProductTransducerAsync<X, Y, A, B>
{
    public Func<TState, S, (X, A), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> First,
        Func<TState, S, B, ValueTask<TResult<S>>> Second) =>
        (st, s, xa) => Nested.BiTransformAsync(
            First:  First,
            Second: (st1, s1, tx) => tx.BiTransformAsync(First, Second)(st1, s1, xa))(st, s, xa);

    public Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformSecondAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) => 
        ProductTransducerAsyncDefault<X, Y, A, B>.TransformSecondAsync(this, reduceRight);

    public Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformFirstAsync<S>(Func<TState, S, Y, ValueTask<TResult<S>>> reduceLeft) => 
        ProductTransducerAsyncDefault<X, Y, A, B>.TransformFirstAsync(this, reduceLeft);
}

record ProductFlattenTransducerAsyncUnit<X, Y, A, B>(ProductTransducerAsync<X, Y, A, ProductTransducerAsync<X, Y, Unit, B>> Nested) 
    : ProductTransducerAsync<X, Y, A, B>
{
    public Func<TState, S, (X, A), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> First,
        Func<TState, S, B, ValueTask<TResult<S>>> Second) =>
        (st, s, xa) => Nested.BiTransformAsync(
            First: First,
            Second: (st1, s1, tx) => tx.BiTransformAsync(First, Second)(st1, s1, (xa.Item1, unit)))(st, s, xa);

    public Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformSecondAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) => 
        ProductTransducerAsyncDefault<X, Y, A, B>.TransformSecondAsync(this, reduceRight);

    public Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformFirstAsync<S>(Func<TState, S, Y, ValueTask<TResult<S>>> reduceLeft) => 
        ProductTransducerAsyncDefault<X, Y, A, B>.TransformFirstAsync(this, reduceLeft);
}

record ProductFlattenTransducerAsyncSync<X, Y, A, B>(ProductTransducerAsync<X, Y, A, ProductTransducer<X, Y, A, B>> Nested) 
    : ProductTransducerAsync<X, Y, A, B>
{
    public Func<TState, S, (X, A), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> First,
        Func<TState, S, B, ValueTask<TResult<S>>> Second) =>
        (st, s, xa) => Nested.BiTransformAsync(
            First:  First,
            Second: (st1, s1, tx) => tx.ToProductAsync().BiTransformAsync(First, Second)(st1, s1, xa))(st, s, xa);

    public Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformSecondAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) => 
        ProductTransducerAsyncDefault<X, Y, A, B>.TransformSecondAsync(this, reduceRight);

    public Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformFirstAsync<S>(Func<TState, S, Y, ValueTask<TResult<S>>> reduceLeft) => 
        ProductTransducerAsyncDefault<X, Y, A, B>.TransformFirstAsync(this, reduceLeft);
}


record ProductFlattenTransducerAsyncSyncUnit<X, Y, A, B>(ProductTransducerAsync<X, Y, A, ProductTransducer<X, Y, Unit, B>> Nested) 
    : ProductTransducerAsync<X, Y, A, B>
{
    public Func<TState, S, (X, A), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> First,
        Func<TState, S, B, ValueTask<TResult<S>>> Second) =>
        (st, s, xa) => Nested.BiTransformAsync(
            First:  First,
            Second: (st1, s1, tx) => tx.ToProductAsync().BiTransformAsync(First, Second)(st1, s1, (xa.Item1, unit)))(st, s, xa);

    public Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformSecondAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) => 
        ProductTransducerAsyncDefault<X, Y, A, B>.TransformSecondAsync(this, reduceRight);

    public Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformFirstAsync<S>(Func<TState, S, Y, ValueTask<TResult<S>>> reduceLeft) => 
        ProductTransducerAsyncDefault<X, Y, A, B>.TransformFirstAsync(this, reduceLeft);
}
