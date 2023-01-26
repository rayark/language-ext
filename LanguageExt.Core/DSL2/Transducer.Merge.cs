#nullable enable
using System;
using System.Threading.Tasks;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace LanguageExt.DSL2;

record ProductMergeTransducer<X, Y, A, B>(ProductTransducer<X, Y, A, B> Transducer) : Transducer<(X, A), (Y, B)>
{
    public Func<TState, S, (X, A), TResult<S>> Transform<S>(Func<TState, S, (Y, B), TResult<S>> reduce) =>
        (st, s, xa) =>
        {
            Y? fst = default;
            return Transducer.BiTransform<S>(
                First: (st1, s1, y) =>
                {
                    fst = y;
                    return TResult.Continue(s1);
                },
                Second: (st1, s1, b) =>
                {
                    #nullable disable
                    return reduce(st1, s1, (fst, b));
                    #nullable enable
                })(st, s, xa);
        };

    public TransducerAsync<(X, A), (Y, B)> ToAsync() => 
        new ProductMergeTransducerAsync<X, Y, A, B>(Transducer.ToProductAsync());
}

record ProductMergeTransducerAsync<X, Y, A, B>(ProductTransducerAsync<X, Y, A, B> Product) : TransducerAsync<(X, A), (Y, B)>
{
    public Func<TState, S, (X, A), ValueTask<TResult<S>>> TransformAsync<S>(Func<TState, S, (Y, B), ValueTask<TResult<S>>> reduce) =>
        (st, s, xa) =>
        {
            Y? fst = default;
            return Product.BiTransformAsync<S>(
                First: (_, s1, y) =>
                {
                    fst = y;
                    return new ValueTask<TResult<S>>(TResult.Continue(s1));
                },
                Second: (st1, s1, b) =>
                {
                    #nullable disable
                    return reduce(st1, s1, (fst, b));
                    #nullable enable
                })(st, s, xa);
        };
}

record SumMergeTransducer<X, Y, A, B>(SumTransducer<X, Y, A, B> Sum) : Transducer<Sum<X, A>, Sum<Y, B>>
{
    public Func<TState, S, Sum<X, A>, TResult<S>> Transform<S>(Func<TState, S, Sum<Y, B>, TResult<S>> reduce) =>
        (st, s, xa) => Sum.BiTransform<S>(
            Left: (st1, s1, y) => reduce(st1, s1, Sum<Y, B>.Left(y)),
            Right: (st1, s1, b) => reduce(st1, s1, Sum<Y, B>.Right(b)))(st, s, xa);

    public TransducerAsync<Sum<X, A>, Sum<Y, B>> ToAsync() => 
        new SumMergeTransducerAsync<X, Y, A, B>(Sum.ToSumAsync());
}

record SumMergeTransducerAsync<X, Y, A, B>(SumTransducerAsync<X, Y, A, B> Sum) : TransducerAsync<Sum<X, A>, Sum<Y, B>>
{
    public Func<TState, S, Sum<X, A>, ValueTask<TResult<S>>> TransformAsync<S>(Func<TState, S, Sum<Y, B>, ValueTask<TResult<S>>> reduce) =>
        (st, s, xa) => Sum.BiTransformAsync<S>(
            Left: (st1, s1, y) => reduce(st1, s1, Sum<Y, B>.Left(y)),
            Right: (st1, s1, b) => reduce(st1, s1, Sum<Y, B>.Right(b)))(st, s, xa);
}
