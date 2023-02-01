#nullable enable
using System;
using System.Threading.Tasks;

namespace LanguageExt.DSL2;

record LiftTransducer1<A, B>(Func<A, B> Function) : Transducer<A, B>
{
    public Func<TState, S, A, TResult<S>> Transform<S>(Func<TState, S, B, TResult<S>> reduce) =>
        (st, s, x) => reduce(st, s, Function(x));

    public TransducerAsync<A, B> ToAsync() => 
        new LiftTransducerAsync1<A, B>(Function);
}

record LiftTransducerAsync1<A, B>(Func<A, ValueTask<B>> Function) : TransducerAsync<A, B>
{
    public Func<TState, S, A, ValueTask<TResult<S>>> TransformAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduce) =>
        async (st, s, x) => 
            await reduce(st, s, await Function(x).ConfigureAwait(false)).ConfigureAwait(false);
}

record LiftTransducer2<A, B>(Func<A, TResult<B>> Function) : Transducer<A, B>
{
    public Func<TState, S, A, TResult<S>> Transform<S>(Func<TState, S, B, TResult<S>> reduce) =>
        (st, s, x) => Function(x).Bind(y => reduce(st, s, y));

    public TransducerAsync<A, B> ToAsync() => 
        new LiftTransducerAsync2<A, B>(x => new ValueTask<TResult<B>>(Function(x)));
}

record LiftTransducerAsync2<A, B>(Func<A, ValueTask<TResult<B>>> Function) : TransducerAsync<A, B>
{
    public Func<TState, S, A, ValueTask<TResult<S>>> TransformAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduce) =>
        async (st, s, x) => await 
            (await Function(x).ConfigureAwait(false))
                .BindAsync(y => reduce(st, s, y)).ConfigureAwait(false);
}
