#nullable enable
using System;
using System.Threading.Tasks;

namespace LanguageExt.DSL2;

record CurryTransducer2<A, B, C>(Func<A, B, C> F) :
    Transducer<A, Transducer<B, C>>
{
    public Func<TState, S, A, TResult<S>> Transform<S>(Func<TState, S, Transducer<B, C>, TResult<S>> reduce) =>
        (st, s, a) => reduce(st, s, Transducer.map(Prelude.par(F, a)));

    public TransducerAsync<A, Transducer<B, C>> ToAsync() =>
        new CurryTransducerAsyncSync2<A, B, C>(F);
}

record CurryTransducerAsync2<A, B, C>(Func<A, B, C> F) :
    TransducerAsync<A, TransducerAsync<B, C>>
{
    public Func<TState, S, A, ValueTask<TResult<S>>> TransformAsync<S>(
        Func<TState, S, TransducerAsync<B, C>, ValueTask<TResult<S>>> reduce) =>
        (st, s, a) => reduce(st, s, TransducerAsync.map(Prelude.par(F, a)));
}

record CurryTransducerAsyncSync2<A, B, C>(Func<A, B, C> F) :
    TransducerAsync<A, Transducer<B, C>>
{
    public Func<TState, S, A, ValueTask<TResult<S>>> TransformAsync<S>(
        Func<TState, S, Transducer<B, C>, ValueTask<TResult<S>>> reduce) =>
        (st, s, a) => reduce(st, s, Transducer.map(Prelude.par(F, a)));
}

