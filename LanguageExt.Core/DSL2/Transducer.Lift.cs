#nullable enable
using System;
using System.Threading.Tasks;

namespace LanguageExt.DSL2;

record LiftTransducer<A, B>(Func<A, B> Function) : Transducer<A, B>
{
    public Func<TState, S, A, TResult<S>> Transform<S>(Func<TState, S, B, TResult<S>> reduce) =>
        (st, s, x) => reduce(st, s, Function(x));

    public TransducerAsync<A, B> ToAsync() => 
        new LiftTransducerAsync<A, B>(Function);
}

record LiftTransducerAsync<A, B>(Func<A, B> Function) : TransducerAsync<A, B>
{
    public Func<TState, S, A, ValueTask<TResult<S>>> TransformAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduce) =>
        (st, s, x) => reduce(st, s, Function(x));
}
