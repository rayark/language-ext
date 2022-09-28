#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

#if !NET_STANDARD
internal sealed record BindTransducer<F, A> : Transducer<K<F, A>, A>
{
    public static readonly Transducer<K<F, A>, A> Default = new BindTransducer<F, A>();
    
    public Func<TState<S>, K<F, A>, TResult<S>> Transform<S>(Func<TState<S>, A, TResult<S>> reduce) =>
        (state, value) => value.Transform(reduce)(state, default);
}
#endif

internal sealed record BindMapTransducer<E, X, A, B, C>(
    Transducer<E, CoProduct<X, A>> First,
    Func<A, Transducer<Unit, B>> Second,
    Func<A, B, C> Third) :
    Transducer<E, CoProduct<X, C>>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, CoProduct<X, C>, TResult<S>> reduce) =>
        (s1, rt) =>
            Transducer.compose(First, Transducer.mapRightValue<X, A, A>(static x => x)).Transform<S>(
                (s2, a) =>
                    Second(a).Transform<S>((s3, b) => reduce(s3, CoProduct.Right<X, C>(Third(a, b))))(s2, default))(s1, rt);
}
