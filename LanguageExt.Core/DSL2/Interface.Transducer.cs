#nullable enable
using System;
using System.Threading.Tasks;

namespace LanguageExt.DSL2;

public interface Transducer<in A, out B>
{
    Func<TState, S, A, TResult<S>> Transform<S>(Func<TState, S, B, TResult<S>> reduce);
    TransducerAsync<A, B> ToAsync();
}

public interface TransducerAsync<in A, out B>
{
    Func<TState, S, A, ValueTask<TResult<S>>> TransformAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduce);
}
