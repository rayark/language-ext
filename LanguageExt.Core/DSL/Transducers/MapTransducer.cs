#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record MapTransducer<A, B>(Func<A, B> Function) : Transducer<A, B>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, B, TResult<S>> reducer) =>
        (state, value) => reducer(state, Function(value));
}
