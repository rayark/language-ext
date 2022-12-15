#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record FlattenTransducer<A, B>(Transducer<A, Transducer<A, B>> Function) : Transducer<A, B>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, B, TResult<S>> reduce) =>
        (state, value) => Function.Transform<S>((s, v) => v.Transform(reduce)(s, value))(state, value);
}

internal sealed record FlattenTransducer2<A, B>(Transducer<A, Transducer<Unit, B>> Function) : Transducer<A, B>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, B, TResult<S>> reduce) =>
        (state, value) => Function.Transform<S>((s, v) => v.Transform(reduce)(s, default))(state, value);
}

internal sealed record FlattenTransducer3<A, B>(Transducer<Unit, Transducer<A, B>> Function) : Transducer<A, B>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, B, TResult<S>> reduce) =>
        (state, value) => Function.Transform<S>((s, v) => v.Transform(reduce)(s, value))(state, default);
}


internal sealed record FlattenTransducer4<A, B>(Func<A, Transducer<A, B>> Function) : Transducer<A, B>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, B, TResult<S>> reduce) =>
        (state, value) => Function(value).Transform(reduce)(state, value);
}
