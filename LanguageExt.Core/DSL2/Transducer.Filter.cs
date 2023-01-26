#nullable enable
using System;
using System.Threading.Tasks;

namespace LanguageExt.DSL2;

record FilterTransducer<A>(Transducer<A, bool> Predicate)
    : Transducer<A, A>
{
    public Func<TState, S, A, TResult<S>> Transform<S>(Func<TState, S, A, TResult<S>> reduce) =>
        (st, s, x) =>
            Predicate.Transform<S>((st1, s1, tf) => tf
                ? reduce(st1, s1, x)
                : TResult.Continue(s1))(st, s, x);

    public TransducerAsync<A, A> ToAsync() =>
        new FilterTransducerAsync<A>(Predicate.ToAsync());
}

record FilterTransducerAsync<A>(TransducerAsync<A, bool> Predicate)
    : TransducerAsync<A, A>
{
    public Func<TState, S, A, ValueTask<TResult<S>>> TransformAsync<S>(Func<TState, S, A, ValueTask<TResult<S>>> reduce) =>
        (st, s, x) =>
            Predicate.TransformAsync<S>((st1, s1, tf) => tf
                ? reduce(st1, s1, x)
                : new ValueTask<TResult<S>>(TResult.Continue(s1)))(st, s, x);
}
