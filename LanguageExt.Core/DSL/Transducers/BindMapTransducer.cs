#nullable enable
using System;
using LanguageExt.Common;

namespace LanguageExt.DSL.Transducers;

internal sealed record BindMapTransducer<RT, E, A, B, C>(
    Transducer<RT, CoProduct<E, A>> First,
    Func<A, Transducer<Unit, B>> Second,
    Func<A, B, C> Third) :
    Transducer<RT, CoProduct<E, C>>
{
    public Func<TState<S>, RT, TResult<S>> Transform<S>(Func<TState<S>, CoProduct<E, C>, TResult<S>> reduce) =>
        (s1, rt) =>
            Transducer.compose(First, Transducer.mapRightValue<E, A, A>(static x => x)).Transform<S>(
                (s2, a) =>
                    Second(a).Transform<S>((s3, b) => reduce(s3, CoProduct.Right<E, C>(Third(a, b))))(s2, default))(s1, rt);
}
