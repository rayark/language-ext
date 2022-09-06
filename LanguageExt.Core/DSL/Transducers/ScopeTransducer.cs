#nullable enable
using System;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace LanguageExt.DSL.Transducers;

internal sealed record ScopeTransducer<A, B>(Transducer<A, B> Function) : Transducer<A, B>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, B, TResult<S>> reducer) =>
        (state, value) =>
        {
            var red = Function.Transform<Option<B>>((_, v) => TResult.Continue(Some(v)));
            var nstate = state.Scope().SetValue(Option<B>.None);
            TResult<Option<B>>? res;
            try
            {
                res = red(nstate, value);
                if (res.Faulted) return TResult.Fail<S>(res.ErrorUnsafe);
            }
            finally
            {
                nstate.CleanUp();
            }            
            if (res.Faulted) return TResult.Fail<S>(res.ErrorUnsafe);
            if(res.ValueUnsafe.IsNone) return TResult.Fail<S>(Errors.Bottom);
            return reducer(state, (B)res.ValueUnsafe);
        };
}

internal sealed record ScopeManyTransducer<A, B>(Transducer<A, B> Function) : Transducer<A, Seq<B>>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, Seq<B>, TResult<S>> reducer) =>
        (state, value) =>
        {
            var red = Function.Transform<Prim<B>>((s, v) => TResult.Continue(s.Value + Prim.Pure(v)));
            var nstate = state.Scope().SetValue(Prim<B>.None);
            TResult<Prim<B>>? res;
            try
            {
                res = red(nstate, value);
                if (res.Faulted) return TResult.Fail<S>(res.ErrorUnsafe);
            }
            finally
            {
                nstate.CleanUp();
            }
            var items = ToSeq(res.ValueUnsafe);
            return items.Match(
                Succ: xs => reducer(state, xs),
                Fail: TResult.Fail<S>); 
        };
    
    Fin<Seq<X>> ToSeq<X>(Prim<X> ma) =>
        ma switch
        {
            PurePrim<X> v => Fin<Seq<X>>.Succ(Seq1(v.Value)),
            ManyPrim<X> m => m.Items.Sequence(ToSeq).Map(static x => x.Flatten()),
            FailPrim<X> f => Fin<Seq<X>>.Fail(f.Value),
            _ => throw new NotSupportedException()
        };    
}

internal sealed record ScopeManyTransducer2<A, B>(Transducer<A, CoProduct<Error, B>> Function) : Transducer<A, Seq<B>>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, Seq<B>, TResult<S>> reducer) =>
        (state, value) =>
        {
            var red = Function.Transform<Prim<B>>((s, v) => v switch
            {
                CoProductRight<Error, B> r => TResult.Continue(s.Value + Prim.Pure(r.Value)),
                CoProductLeft<Error, B> l => TResult.Fail<Prim<B>>(l.Value),
                CoProductFail<Error, B> f => TResult.Fail<Prim<B>>(f.Value),
                _ => throw new NotSupportedException()
            });
            var nstate = state.Scope().SetValue(Prim<B>.None);
            TResult<Prim<B>>? res;
            try
            {
                res = red(nstate, value);
                if (res.Faulted) return TResult.Fail<S>(res.ErrorUnsafe);
            }
            finally
            {
                nstate.CleanUp();
            }
            var items = ToSeq(res.ValueUnsafe);
            return items.Match(
                Succ: xs => reducer(state, xs),
                Fail: TResult.Fail<S>); 
        };
    
    Fin<Seq<X>> ToSeq<X>(Prim<X> ma) =>
        ma switch
        {
            PurePrim<X> v => Fin<Seq<X>>.Succ(Seq1(v.Value)),
            ManyPrim<X> m => m.Items.Sequence(ToSeq).Map(static x => x.Flatten()),
            FailPrim<X> f => Fin<Seq<X>>.Fail(f.Value),
            _ => throw new NotSupportedException()
        };    
}
