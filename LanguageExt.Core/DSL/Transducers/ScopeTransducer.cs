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
                if (res.Complete) return TResult.Complete(state.Value);
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
            var red = Function.Transform<Seq<B>>((s, v) => TResult.Continue(s.Value.Add(v)));
            var nstate = state.Scope().SetValue(Seq<B>());
            TResult<Seq<B>>? res;
            try
            {
                res = red(nstate, value);
                if (res.Faulted) return TResult.Fail<S>(res.ErrorUnsafe);
                if (res.Complete) return TResult.Complete(state.Value);
                return reducer(state, res.ValueUnsafe);
            }
            finally
            {
                nstate.CleanUp();
            }
        };
}

internal sealed record ScopeManyTransducer<X, A, B>(Transducer<A, CoProduct<X, B>> Function) : Transducer<A, CoProduct<X, B>>
{
    readonly record struct Collector(X? Left, bool IsLeft, B? Value)
    {
        public static readonly Collector Default = new (default, default, default);
    }

    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, CoProduct<X, B>, TResult<S>> reducer) =>
        (state, value) =>
        {
            var red = Function.Transform<Collector>((s, v) =>
                (s.Value, v) switch
                {
                    ({IsLeft: false} os, CoProductRight<X, B> r) => TResult.Continue(os with { Value =  r.Value }),
                    (var os, CoProductLeft<X, B> l) => TResult.Complete(os with { IsLeft = true, Left = l.Value }),
                    (_, CoProductFail<X, B> f) => TResult.Fail<Collector>(f.Value),
                    _ => throw new NotSupportedException()
                });
            
            var nstate = state.Scope().SetValue(Collector.Default);
            TResult<Collector>? res;
            try
            {
                #nullable disable
                res = red(nstate, value);
                if (res.Faulted) return TResult.Fail<S>(res.ErrorUnsafe);
                if (res.ValueUnsafe.IsLeft) return reducer(state, CoProduct.Left<X, B>(res.ValueUnsafe.Left));
                if (res.Complete) return TResult.Complete(state.Value);
                return reducer(state, CoProduct.Right<X, B>(res.ValueUnsafe.Value));
                #nullable enable
            }
            finally
            {
                nstate.CleanUp();
            }
        };
}

internal sealed record ScopeManyTransducer2<X, A, B>(Transducer<A, CoProduct<X, B>> Function) : Transducer<A, CoProduct<X, Seq<B>>>
{
    readonly record struct Collector(X? Left, bool IsLeft, Seq<B> Values)
    {
        public static readonly Collector Default = new (default, default, default);
    }

    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, CoProduct<X, Seq<B>>, TResult<S>> reducer) =>
        (state, value) =>
        {
            var red = Function.Transform<Collector>((s, v) =>
                (s.Value, v) switch
                {
                    ({IsLeft: false} os, CoProductRight<X, B> r) => TResult.Continue(os with { Values = os.Values.Add(r.Value)}),
                    (_, CoProductLeft<X, B> l) => TResult.Complete(new Collector(IsLeft: true, Left: l.Value, Values: Seq<B>())),
                    (_, CoProductFail<X, B> f) => TResult.Fail<Collector>(f.Value),
                    _ => throw new NotSupportedException()
                });
            
            var nstate = state.Scope().SetValue(Collector.Default);
            TResult<Collector>? res;
            try
            {
                res = red(nstate, value);
                if (res.Faulted) return TResult.Fail<S>(res.ErrorUnsafe);
                if (res.ValueUnsafe.IsLeft && res.ValueUnsafe.Left != null) return reducer(state, CoProduct.Left<X, Seq<B>>(res.ValueUnsafe.Left));
                if (res.Complete) return TResult.Complete(state.Value);
                return reducer(state, CoProduct.Right<X, Seq<B>>(res.ValueUnsafe.Values));
            }
            finally
            {
                nstate.CleanUp();
            }
        };
}
