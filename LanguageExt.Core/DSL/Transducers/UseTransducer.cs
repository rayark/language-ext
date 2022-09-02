#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record UseTransducer<A> : Transducer<A, A> where A : IDisposable
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, A, TResult<S>> reduce) =>
        (state, value) =>
        {
            state.Use(value, value);
            return reduce(state, value);
        };
}

internal sealed record UseTransducer2<A>(Action<A> Dispose) : Transducer<A, A>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, A, TResult<S>> reduce) =>
        (state, value) =>
        {
            state.Use(value, new Disposer(value, Dispose));
            return reduce(state, value);
        };

    public record Disposer(A value, Action<A> dispose) : IDisposable
    {
        public void Dispose() =>
            dispose(value);
    }
}

internal sealed record ReleaseTransducer<A> : Transducer<A, Unit>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, Unit, TResult<S>> reduce) =>
        (state, value) =>
        {
            state.Release(value);
            return reduce(state, default);
        };
}
