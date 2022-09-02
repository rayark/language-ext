#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record ComposeTransducer<A, B, C>(Transducer<A, B> First, Transducer<B, C> Second) : Transducer<A, C>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, C, TResult<S>> reducer) =>
        First.Transform(Second.Transform(reducer));
}
