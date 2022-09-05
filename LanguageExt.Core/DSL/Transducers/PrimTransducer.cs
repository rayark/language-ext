#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record PrimTransducer<A>(Prim<A> Value) : Transducer<Unit, A>
{
    public Func<TState<S>, Unit, TResult<S>> Transform<S>(Func<TState<S>, A, TResult<S>> reducer) =>
        (state, _) => Go(Value, state, reducer);
    
    static TResult<S> Go<S>(Prim<A> ma, TState<S> state, Func<TState<S>, A, TResult<S>> reducer) =>
        ma switch
        {
            PurePrim<A> v => reducer(state, v.Value),
            ManyPrim<A> m => FoldItems(m.Items, state, reducer),
            FailPrim<A> f => TResult.Fail<S>(f.Value),
            _ => throw new NotSupportedException()
        };

    static TResult<S> FoldItems<S>(Seq<Prim<A>> ps, TState<S> state, Func<TState<S>, A, TResult<S>> reducer)
    {
        foreach (var p in ps)
        {
            var r = Go(p, state, reducer);
            if (r.Complete) return r;
            if (r.Faulted) return r;
            state = state.SetValue(r.ValueUnsafe);
        }
        return TResult.Complete(state.Value);
    }
}
