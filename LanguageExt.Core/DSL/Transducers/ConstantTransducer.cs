#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record ConstantTransducer<A, B>(B Constant) : Transducer<A, B>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, B, TResult<S>> reducer) =>
        (state, _) => reducer(state, Constant);
}
