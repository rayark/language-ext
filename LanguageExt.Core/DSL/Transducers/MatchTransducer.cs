#nullable enable

using System;

namespace LanguageExt.DSL.Transducers;

internal record MatchTransducer<E, X, A, B>(
    Transducer<E, CoProduct<X, A>> M,
    Transducer<X, B> Left,
    Transducer<A, B> Right) : 
    Transducer<E, B>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, B, TResult<S>> reduce) =>
        (s, v) => M.Transform<S>((s1, p) => p switch
        {
            CoProductRight<X, A> r => Right.Transform(reduce)(s1, r.Value),
            CoProductLeft<X, A> l => Left.Transform(reduce)(s1, l.Value),
            CoProductFail<X, A> f => TResult.Fail<S>(f.Value),
            _ => throw new NotSupportedException()
        })(s, v);
}

internal record MatchTransducer2<E, X, A, B>(
    Transducer<E, CoProduct<X, A>> M,
    Func<X, B> Left,
    Func<A, B> Right) : 
    Transducer<E, B>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, B, TResult<S>> reduce) =>
        (s, v) => M.Transform<S>((s1, p) => p switch
        {
            CoProductRight<X, A> r => reduce(s1, Right(r.Value)),
            CoProductLeft<X, A> l => reduce(s1, Left(l.Value)),
            CoProductFail<X, A> f => TResult.Fail<S>(f.Value),
            _ => throw new NotSupportedException()
        })(s, v);
}
