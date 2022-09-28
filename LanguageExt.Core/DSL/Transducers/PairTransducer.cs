#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

public record PairTransducer<A, B, X, Y>(Transducer<A, X> First, Transducer<B, Y> Second) :
    Transducer<(A, B), (X, Y)>
{
    public Func<TState<S>, (A, B), TResult<S>> Transform<S>(Func<TState<S>, (X, Y), TResult<S>> reduce) =>
        (state, value) =>
        {
            var fst = First.Transform<Prim<X>>(
                (s, v) => TResult.Continue(s.Value + Prim.Pure(v)))(state.SetValue(Prim<X>.None), value.Item1);

            if (fst.Complete) return TResult.Complete(state.Value); 
            if (fst.Faulted) return TResult.Fail<S>(fst.ErrorUnsafe); 
            
            var snd = Second.Transform<Prim<Y>>(
                (s, v) => TResult.Continue(s.Value + Prim.Pure(v)))(state.SetValue(Prim<Y>.None), value.Item2);

            if (snd.Complete) return TResult.Complete(state.Value); 
            if (fst.Faulted) return TResult.Fail<S>(fst.ErrorUnsafe);

            return Transducer.prim(fst.ValueUnsafe.Zip(snd.ValueUnsafe)).Transform(reduce)(state, default);
        };
}

public record MakePairTransducer<A> : Transducer<A, (A, A)>
{
    public static Transducer<A, (A, A)> Default = new MakePairTransducer<A>();
    
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, (A, A), TResult<S>> reduce) =>
        (s, v) => reduce(s, (v, v));
}
