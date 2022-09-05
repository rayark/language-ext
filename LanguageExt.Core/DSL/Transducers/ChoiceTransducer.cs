#nullable enable
using System;
using LanguageExt.Common;

namespace LanguageExt.DSL.Transducers;

internal sealed record ChoiceTransducer<A, B>(Transducer<A, B> First, Transducer<A, B> Second) : Transducer<A, B>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, B, TResult<S>> reducer) =>
        (state, value) =>
        {
            var res = First.Transform(reducer)(state, value);
            if (res.Faulted) return Second.Transform(reducer)(state, value);
            return res;
        };
}

internal sealed record ChoiceTransducer<X, A, B>(Transducer<A, CoProduct<X, B>> First, Transducer<A, CoProduct<X, B>> Second) : 
    Transducer<A, CoProduct<X, B>>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, CoProduct<X, B>, TResult<S>> reducer) =>
        (state, value) =>
        {
            var res = First.Transform<S>((s, p) =>
                p switch
                {
                    CoProductRight<X, B> => reducer(s, p),
                    _                    => TResult.Fail<S>(Errors.Bottom)
                })(state, value);
            
            if (res.Faulted) return Second.Transform(reducer)(state, value);
            return res;
        };
}
