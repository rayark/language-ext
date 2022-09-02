#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record KleisliTransducer<E, X, A, B>(Transducer<E, CoProduct<X, A>> First, Transducer<A, CoProduct<X, B>> Second) : Transducer<E, CoProduct<X, B>>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, CoProduct<X, B>, TResult<S>> reduce) =>
        (state, env) =>
        {
            var red = First.Transform<S>((s, xa) =>
                xa switch
                {
                    CoProductRight<X, A> r => Second.Transform(reduce)(s, r.Value),
                    CoProductLeft<X, A> l => reduce(s, CoProduct.Left<X, B>(l.Value)),
                    CoProductFail<X, A> f => reduce(s, CoProduct.Fail<X, B>(f.Value)),
                    _ => throw new NotSupportedException()
                });

            return red(state, env);
        };
}

internal sealed record KleisliTransducer2<E, X, A, B>(Transducer<E, CoProduct<X, A>> First, Transducer<A, Transducer<E, CoProduct<X, B>>> Second) : Transducer<E, CoProduct<X, B>>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, CoProduct<X, B>, TResult<S>> reduce) =>
        (state, env) =>
        {
            var red = First.Transform<S>((s, xa) =>
                xa switch
                {
                    CoProductRight<X, A> r => Second.Transform<S>((s1, mb) => mb.Transform(reduce)(s1, env))(s, r.Value),
                    CoProductLeft<X, A> l => reduce(s, CoProduct.Left<X, B>(l.Value)),
                    CoProductFail<X, A> f => reduce(s, CoProduct.Fail<X, B>(f.Value)),
                    _ => throw new NotSupportedException()
                });

            return red(state, env);
        };
}

internal sealed record KleisliProjectTransducer2<E, X, A, B, C>(
    Transducer<E, CoProduct<X, A>> First, 
    Transducer<A, Transducer<E, CoProduct<X, B>>> Second,
    Func<A, B, C> Project) : 
    Transducer<E, CoProduct<X, B>>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, CoProduct<X, B>, TResult<S>> reduce) =>
        (state, env) =>
        {
            var red = First.Transform<S>((s, xa) =>
                xa switch
                {
                    CoProductRight<X, A> r => Second.Transform<S>((s1, mb) => mb.Transform(reduce)(s1, env))(s, r.Value),
                    CoProductLeft<X, A> l => reduce(s, CoProduct.Left<X, B>(l.Value)),
                    CoProductFail<X, A> f => reduce(s, CoProduct.Fail<X, B>(f.Value)),
                    _ => throw new NotSupportedException()
                });

            return red(state, env);
        };
}

internal sealed record BiKleisliTransducer<E, X, Y, A, B>(Transducer<E, CoProduct<X, A>> First, BiTransducer<X, Y, A, B> Second) : Transducer<E, CoProduct<Y, B>>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, CoProduct<Y, B>, TResult<S>> reduce) =>
        (state, env) =>
        {
            var red = First.Transform<S>((s, xa) =>
                xa switch
                {
                    CoProductRight<X, A> r => Second.RightTransducer.Transform<S>((s1, b) => reduce(s1, CoProduct.Right<Y, B>(b)))(s, r.Value),
                    CoProductLeft<X, A> l => Second.LeftTransducer.Transform<S>((s1, y) => reduce(s1, CoProduct.Left<Y, B>(y)))(s, l.Value),
                    CoProductFail<X, A> f => reduce(s, CoProduct.Fail<Y, B>(f.Value)),
                    _ => throw new NotSupportedException()
                });

            return red(state, env);
        };
}
