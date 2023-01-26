#nullable enable
using System;
using System.Threading.Tasks;

namespace LanguageExt.DSL2;

record FunctorTransducer<A, B>(Func<A, B> Map) : Transducer<A, B>
{
    public Func<TState, S, A, TResult<S>> Transform<S>(
        Func<TState, S, B, TResult<S>> reduce) => (st, s, a) => reduce(st, s, Map(a));

    public TransducerAsync<A, B> ToAsync() =>
        new FunctorTransducerAsync<A, B>(Map);
}

record FunctorTransducerAsync<A, B>(Func<A, B> Map) : TransducerAsync<A, B>
{
    public Func<TState, S, A, ValueTask<TResult<S>>> TransformAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduce) => 
        async (st, s, a) => await reduce(st, s, Map(a)).ConfigureAwait(false);
}

record SumBiFunctorTransducer<X, Y, A, B>(Transducer<X, Y> MapLeft, Transducer<A, B> MapRight) : 
    SumTransducer<X, Y, A, B>
{
    public Func<TState, S, Sum<X, A>, TResult<S>> BiTransform<S>(
        Func<TState, S, Y, TResult<S>> Left,
        Func<TState, S, B, TResult<S>> Right) =>
        (st, s, xa) => xa switch
        {
            SumRight<X, A> r => MapRight.Transform(Right)(st, s, r.Value),
            SumLeft<X, A> l => MapLeft.Transform(Left)(st, s, l.Value),
            _ => TResult.Complete(s)
        };

    public Func<TState, S, A, TResult<S>> TransformRight<S>(Func<TState, S, B, TResult<S>> reduceRight) =>
        SumTransducerDefault<X, Y, A, B>.TransformRight(this, reduceRight);

    public Func<TState, S, X, TResult<S>> TransformLeft<S>(Func<TState, S, Y, TResult<S>> reduceLeft) => 
        SumTransducerDefault<X, Y, A, B>.TransformLeft(this, reduceLeft);

    public SumTransducerAsync<X, Y, A, B> ToSumAsync() =>
        new SumBiFunctorTransducerAsync<X, Y, A, B>(MapLeft.ToAsync(), MapRight.ToAsync());
}

record SumBiFunctorTransducerAsync<X, Y, A, B>(TransducerAsync<X, Y> MapLeft, TransducerAsync<A, B> MapRight) : 
    SumTransducerAsync<X, Y, A, B>
{
    public Func<TState, S, Sum<X, A>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> Left,
        Func<TState, S, B, ValueTask<TResult<S>>> Right) =>
        (st, s, xa) => xa switch
        {
            SumRight<X, A> r => MapRight.TransformAsync(Right)(st, s, r.Value),
            SumLeft<X, A> l => MapLeft.TransformAsync(Left)(st, s, l.Value),
            _ => new ValueTask<TResult<S>>(TResult.Complete(s))
        };

    public Func<TState, S, A, ValueTask<TResult<S>>> TransformRightAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) =>
        SumTransducerAsyncDefault<X, Y, A, B>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> reduceLeft) =>
        SumTransducerAsyncDefault<X, Y, A, B>.TransformLeftAsync(this, reduceLeft);
}

record ProductBiFunctorTransducer<X, Y, A, B>(Transducer<X, Y> MapFirst, Transducer<A, B> MapSecond) : ProductTransducer<X, Y, A, B>
{
    public Func<TState, S, (X, A), TResult<S>> BiTransform<S>(
        Func<TState, S, Y, TResult<S>> First,
        Func<TState, S, B, TResult<S>> Second) =>
        (st, s, pair) =>
        {
            var s1 = MapFirst.Transform(First)(st, s, pair.Item1);
            if (!s1.Continue) return s1;
            return MapSecond.Transform(Second)(st, s1.ValueUnsafe, pair.Item2);
        };

    public Func<TState, S, (X, A), TResult<S>> TransformFirst<S>(Func<TState, S, Y, TResult<S>> reduceFirst) => 
        (st, s, pair) => MapFirst.Transform(reduceFirst)(st, s, pair.Item1);
    
    public Func<TState, S, (X, A), TResult<S>> TransformSecond<S>(Func<TState, S, B, TResult<S>> reduceSecond) => 
        (st, s, pair) => MapSecond.Transform(reduceSecond)(st, s, pair.Item2);

    public ProductTransducerAsync<X, Y, A, B> ToProductAsync() =>
        new ProductBiFunctorTransducerAsync<X, Y, A, B>(MapFirst.ToAsync(), MapSecond.ToAsync());
}

record ProductBiFunctorTransducerAsync<X, Y, A, B>(TransducerAsync<X, Y> MapFirst, TransducerAsync<A, B> MapSecond) : ProductTransducerAsync<X, Y, A, B>
{
    public Func<TState, S, (X, A), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> First,
        Func<TState, S, B, ValueTask<TResult<S>>> Second) =>
        async (st, s, pair) =>
        {
            var s1 = await MapFirst.TransformAsync(First)(st, s, pair.Item1).ConfigureAwait(false);
            if (!s1.Continue) return s1;
            return await MapSecond.TransformAsync(Second)(st, s1.ValueUnsafe, pair.Item2).ConfigureAwait(false);
        };

    public Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformFirstAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> reduceFirst) => 
        (st, s, pair) => MapFirst.TransformAsync(reduceFirst)(st, s, pair.Item1);
    
    public Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformSecondAsync<S>(
        Func<TState, S, B, ValueTask<TResult<S>>> reduceSecond) => 
        (st, s, pair) => MapSecond.TransformAsync(reduceSecond)(st, s, pair.Item2);
}

record ProductMapFirstTransducer<X, Y, Z, A, B>(ProductTransducer<X, Y, A, B> F, Transducer<Y, Z> MapFirst) : 
    ProductTransducer<X, Z, A, B>
{
    public Func<TState, S, (X, A), TResult<S>> BiTransform<S>(
        Func<TState, S, Z, TResult<S>> First,
        Func<TState, S, B, TResult<S>> Second) =>
        (st, s, pair) =>
            F.BiTransform(
                First: (st1, s1, y) => MapFirst.Transform(First)(st1, s1, y),
                Second: Second)(st, s, pair);

    public Func<TState, S, (X, A), TResult<S>> TransformFirst<S>(Func<TState, S, Z, TResult<S>> reduceFirst) =>
        ProductTransducerDefault<X, Z, A, B>.TransformFirst(this, reduceFirst);
    
    public Func<TState, S, (X, A), TResult<S>> TransformSecond<S>(Func<TState, S, B, TResult<S>> reduceSecond) => 
        ProductTransducerDefault<X, Z, A, B>.TransformSecond(this, reduceSecond);
    
    public ProductTransducerAsync<X, Z, A, B> ToProductAsync() =>
        new ProductMapFirstTransducerAsync<X, Y, Z, A, B>(F.ToProductAsync(), MapFirst.ToAsync());
}

record ProductMapFirstTransducerAsync<X, Y, Z, A, B>(ProductTransducerAsync<X, Y, A, B> F, TransducerAsync<Y, Z> MapFirst) : 
    ProductTransducerAsync<X, Z, A, B>
{
    public Func<TState, S, (X, A), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, Z, ValueTask<TResult<S>>> First,
        Func<TState, S, B, ValueTask<TResult<S>>> Second) =>
        (st, s, pair) =>
            F.BiTransformAsync(
                First: (st1, s1, y) => MapFirst.TransformAsync(First)(st1, s1, y),
                Second: Second)(st, s, pair);

    public Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformFirstAsync<S>(Func<TState, S, Z, ValueTask<TResult<S>>> reduceFirst) =>
        ProductTransducerAsyncDefault<X, Z, A, B>.TransformFirstAsync(this, reduceFirst);
    
    public Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformSecondAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduceSecond) => 
        ProductTransducerAsyncDefault<X, Z, A, B>.TransformSecondAsync(this, reduceSecond);
}

record ProductMapSecondTransducer<X, Y, A, B, C>(ProductTransducer<X, Y, A, B> F, Transducer<B, C> MapSecond) : 
    ProductTransducer<X, Y, A, C>
{
    public Func<TState, S, (X, A), TResult<S>> BiTransform<S>(
        Func<TState, S, Y, TResult<S>> First,
        Func<TState, S, C, TResult<S>> Second) =>
        (st, s, pair) =>
            F.BiTransform(
                First: First,
                Second: (st1, s1, y) => MapSecond.Transform(Second)(st1, s1, y))(st, s, pair);

    public Func<TState, S, (X, A), TResult<S>> TransformFirst<S>(Func<TState, S, Y, TResult<S>> reduceFirst) =>
        ProductTransducerDefault<X, Y, A, C>.TransformFirst(this, reduceFirst);
    
    public Func<TState, S, (X, A), TResult<S>> TransformSecond<S>(Func<TState, S, C, TResult<S>> reduceSecond) => 
        ProductTransducerDefault<X, Y, A, C>.TransformSecond(this, reduceSecond);
    
    public ProductTransducerAsync<X, Y, A, C> ToProductAsync() =>
        new ProductMapSecondTransducerAsync<X, Y, A, B, C>(F.ToProductAsync(), MapSecond.ToAsync());
}

record ProductMapSecondTransducerAsync<X, Y, A, B, C>(ProductTransducerAsync<X, Y, A, B> F, TransducerAsync<B, C> MapSecond) : 
    ProductTransducerAsync<X, Y, A, C>
{
    public Func<TState, S, (X, A), ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> First,
        Func<TState, S, C, ValueTask<TResult<S>>> Second) =>
        (st, s, pair) =>
            F.BiTransformAsync(
                First: First,
                Second: (st1, s1, y) => MapSecond.TransformAsync(Second)(st1, s1, y))(st, s, pair);

    public Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformFirstAsync<S>(Func<TState, S, Y, ValueTask<TResult<S>>> reduceFirst) =>
        ProductTransducerAsyncDefault<X, Y, A, C>.TransformFirstAsync(this, reduceFirst);
    
    public Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformSecondAsync<S>(Func<TState, S, C, ValueTask<TResult<S>>> reduceSecond) => 
        ProductTransducerAsyncDefault<X, Y, A, C>.TransformSecondAsync(this, reduceSecond);
}
