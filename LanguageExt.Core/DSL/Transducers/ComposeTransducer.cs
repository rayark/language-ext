#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record ComposeTransducer<A, B, C>(
    Transducer<A, B> First, 
    Transducer<B, C> Second) : Transducer<A, C>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, C, TResult<S>> reducer) =>
        First.Transform(Second.Transform(reducer));
}

internal sealed record ComposeTransducer<A, B, C, D>(
    Transducer<A, B> First, 
    Transducer<B, C> Second,
    Transducer<C, D> Third
    ) : Transducer<A, D>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, D, TResult<S>> reducer) =>
        First.Transform(Second.Transform(Third.Transform(reducer)));
}

internal sealed record ComposeTransducer<A, B, C, D, E>(
    Transducer<A, B> First, 
    Transducer<B, C> Second,
    Transducer<C, D> Third,
    Transducer<D, E> Fourth
) : Transducer<A, E>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, E, TResult<S>> reducer) =>
        First.Transform(Second.Transform(Third.Transform(Fourth.Transform(reducer))));
}

internal sealed record ComposeTransducer<A, B, C, D, E, F>(
    Transducer<A, B> First, 
    Transducer<B, C> Second,
    Transducer<C, D> Third,
    Transducer<D, E> Fourth,
    Transducer<E, F> Fifth
) : Transducer<A, F>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, F, TResult<S>> reducer) =>
        First.Transform(Second.Transform(Third.Transform(Fourth.Transform(Fifth.Transform(reducer)))));
}

internal sealed record ComposeTransducer<A, B, C, D, E, F, G>(
    Transducer<A, B> First, 
    Transducer<B, C> Second,
    Transducer<C, D> Third,
    Transducer<D, E> Fourth,
    Transducer<E, F> Fifth,
    Transducer<F, G> Sixth
) : Transducer<A, G>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, G, TResult<S>> reducer) =>
        First.Transform(Second.Transform(Third.Transform(Fourth.Transform(Fifth.Transform(Sixth.Transform(reducer))))));
}
