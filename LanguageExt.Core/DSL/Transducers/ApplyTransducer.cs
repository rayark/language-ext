#nullable enable
using System;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace LanguageExt.DSL.Transducers;

internal sealed record Apply1Transducer<E, A, B>(
    Transducer<E, Transducer<A, B>> Function, 
    Transducer<E, A> Arg1) : Transducer<E, B>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, B, TResult<S>> reducer) =>
        (state, value) =>
        {
            var astate = state.SetValue(Prim<A>.None);
            var arg1 = Arg1.Transform<Prim<A>>(static (s, x) => TResult.Continue(s.Value + Prim.Pure(x)))(astate, value)
                           .Match(Complete: identity, Continue: identity, Fail: Prim.Fail<A>)
                           .ToFin();

            if (arg1.IsFail) return TResult.Fail<S>((Error)arg1);

            foreach (var x in (Seq<A>)arg1)
            {
                var res = Function.Transform<S>((s, f) => f.Transform(reducer)(s, x))(state, value);
                if (res.Faulted) return res;
                if (res.Complete) return TResult.Complete(state.Value);
                state = state.SetValue(res);
            }
            return TResult.Complete(state.Value);
        };
}

internal sealed record Apply2Transducer<E, A, B, C>(
    Transducer<E, Transducer<A, Transducer<B, C>>> Function, 
    Transducer<E, A> Arg1, 
    Transducer<E, B> Arg2) : 
    Transducer<E, C>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, C, TResult<S>> reducer) =>
        (state, value) =>
        {
            var astate1 = state.SetValue(Prim<A>.None);
            var arg1 = Arg1.Transform<Prim<A>>(static (s, x) => TResult.Continue(s.Value + Prim.Pure(x)))(astate1, value)
                .Match(Complete: identity, Continue: identity, Fail: Prim.Fail<A>)
                .ToFin();

            var astate2 = state.SetValue(Prim<B>.None);
            var arg2 = Arg2.Transform<Prim<B>>(static (s, x) => TResult.Continue(s.Value + Prim.Pure(x)))(astate2, value)
                .Match(Complete: identity, Continue: identity, Fail: Prim.Fail<B>)
                .ToFin();
 
            if(arg1.IsFail && arg2.IsFail) return TResult.Fail<S>((Error)arg1 + (Error)arg2);
            if (arg1.IsFail) return TResult.Fail<S>((Error)arg1);
            if (arg2.IsFail) return TResult.Fail<S>((Error)arg2);

            foreach (var x in (Seq<A>)arg1)
            {
                foreach (var y in (Seq<B>)arg2)
                {
                    var res = Function.Transform<S>(
                        (s1, f1) => f1.Transform<S>(
                            (s2, f2) => f2.Transform(reducer)(s2, y))(s1, x))(state, value);
                    
                    if (res.Faulted) return res;
                    if (res.Complete) return TResult.Complete(state.Value);
                    state = state.SetValue(res);
                }
            }
            return TResult.Complete(state.Value);
        };
}

