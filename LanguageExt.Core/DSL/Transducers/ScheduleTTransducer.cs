#if !NET_STANDARD

using System;
using System.Threading;

namespace LanguageExt.DSL.Transducers;

internal record ScheduleTTransducer<F, E, A>(
    Transducer<E, K<F, A>> Morphism, 
    Schedule Schedule,
    Func<A, bool> Predicate) :
    Transducer<E, K<F, A>>
    where F : Applicative<F>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, K<F, A>, TResult<S>> reduce) =>
        (state, value) =>
        {
            var durations = Schedule.Run();

            var result = Morphism.Transform<S>((s1, ma) =>
                ma.Transform<S>((s2, p) =>
                    Predicate(p)
                        ? reduce(s2, F.Pure(p))
                        : TResult.Complete(s2.Value))(s1, default))(state, value);

            if (result.Complete) return result;
            if (result.Faulted) return result;

            var wait = new AutoResetEvent(false);
            using var enumerator = durations.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current != Duration.Zero) wait.WaitOne((int)enumerator.Current);
                
                result = Morphism.Transform<S>((s1, ma) =>
                    ma.Transform<S>((s2, a) =>
                        Predicate(a)
                            ? reduce(s2, F.Pure(a))
                            : TResult.Complete(s2.Value))(s1, default))(state, value);
                
 
                if (result.Complete) return result;
                if (result.Faulted) return result;
            }

            return result;
        };
}

internal sealed record ScheduleFoldUntilTTransducer2<ST, F, E, A>(
    Transducer<E, K<F, A>> Morphism, 
    ST State, 
    Func<ST, A, ST> Fold,
    Func<ST, bool> Predicate,
    Schedule Schedule) : 
    Transducer<E, K<F, ST>>
    where F : Applicative<F>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, K<F, ST>, TResult<S>> reducer) =>
        (state, value) =>
        {
            using var wait = new AutoResetEvent(false);
            using var durations = Schedule.Run().GetEnumerator();

            TResult<ST> fold(ST x) =>
                Predicate(x) 
                    ? TResult.Complete(x)
                    : TResult.Continue(x);

            var result = Morphism.Transform<ST>(
                (s1, ma) => ma.Transform<ST>(
                    (s2, a) => fold(Fold(s2.Value, a)))(s1, default))(state.SetValue(State), value);
            
            if (result.Faulted) return TResult.Fail<S>(result.ErrorUnsafe);
            if (result.Complete) return reducer(state, F.Pure(State));

            while (durations.MoveNext())
            {
                if (durations.Current != Duration.Zero) wait.WaitOne((int)durations.Current);

                var nresult = Morphism.Transform<ST>(
                    (s1, ma) => ma.Transform<ST>(
                        (s2, a) => fold(Fold(s2.Value, a)))(s1, default))(state.SetValue(result), value);
                
                if (nresult.Faulted) return TResult.Fail<S>(nresult.ErrorUnsafe);
                if (nresult.Complete) return reducer(state, F.Pure(result.ValueUnsafe));
                result = nresult;
            }

            return reducer(state, F.Pure(result.ValueUnsafe));
        };
}

internal sealed record ScheduleFoldUntilTTransducer<ST, F, E, A>(
    Transducer<E, K<F, A>> Morphism, 
    ST State, 
    Func<ST, A, ST> Fold,
    Func<A, bool> Predicate,
    Schedule Schedule) : 
    Transducer<E, K<F, ST>>
    where F : Applicative<F>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, K<F, ST>, TResult<S>> reducer) =>
        (state, value) =>
        {
            using var wait = new AutoResetEvent(false);
            using var durations = Schedule.Run().GetEnumerator();

            TResult<ST> fold(TState<ST> s, A x) =>
                Predicate(x) 
                    ? TResult.Complete(s.Value)
                    : TResult.Continue(Fold(s, x));

            var result = Morphism.Transform<ST>(
                (s1, ma) => ma.Transform<ST>(fold)(s1, default))(state.SetValue(State), value);
            
            if (result.Faulted) return TResult.Fail<S>(result.ErrorUnsafe);
            if (result.Complete) return reducer(state, F.Pure(State));

            while (durations.MoveNext())
            {
                if (durations.Current != Duration.Zero) wait.WaitOne((int)durations.Current);

                var nresult = Morphism.Transform<ST>(
                    (s1, ma) => ma.Transform<ST>(fold)(s1, default))(state.SetValue(result), value);
                
                if (nresult.Faulted) return TResult.Fail<S>(nresult.ErrorUnsafe);
                if (nresult.Complete) return reducer(state, F.Pure(result.ValueUnsafe));
                result = nresult;
            }

            return reducer(state, F.Pure(result.ValueUnsafe));
        };
}

internal sealed record ScheduleFoldTTransducer<ST, F, E, A>(
    Transducer<E, K<F, A>> Morphism, 
    ST State, 
    Func<ST, A, ST> Fold,
    Schedule Schedule) : 
    Transducer<E, K<F, ST>>
    where F : Applicative<F>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, K<F, ST>, TResult<S>> reducer) =>
        (state, value) =>
        {
            using var wait = new AutoResetEvent(false);
            using var durations = Schedule.Run().GetEnumerator();

            var result = Morphism.Transform<ST>(
                (s1, ma) => ma.Transform<ST>(
                    (s2, a) => TResult.Continue(Fold(s2.Value, a)))(s1, default))(state.SetValue(State), value);
            
            if (result.Faulted) return TResult.Fail<S>(result.ErrorUnsafe);
            if (result.Complete) return reducer(state, F.Pure(result.ValueUnsafe));

            while (durations.MoveNext())
            {
                if (durations.Current != Duration.Zero) wait.WaitOne((int)durations.Current);
                
                var nresult = Morphism.Transform<ST>(
                    (s1, ma) => ma.Transform<ST>(
                        (s2, a) => TResult.Continue(Fold(s2.Value, a)))(s1, default))(state.SetValue(result), value);
                if (nresult.Faulted) return TResult.Fail<S>(nresult.ErrorUnsafe);
                if (nresult.Complete) return reducer(state, F.Pure(result.ValueUnsafe));
                result = nresult;
            }

            return reducer(state, F.Pure(result.ValueUnsafe));
        };
}


internal record ScheduleTTransducer2<F, E, A>(
    Transducer<E, K<F, E, A>> Morphism, 
    Schedule Schedule,
    Func<A, bool> Predicate) :
    Transducer<E, K<F, E, A>>
    where F : ApplicativeE<F>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, K<F, E, A>, TResult<S>> reduce) =>
        (state, value) =>
        {
            var durations = Schedule.Run();

            var result = Morphism.Transform<S>((s1, ma) =>
                ma.Transform<S>((s2, p) =>
                    Predicate(p)
                        ? reduce(s2, F.Pure<E, A>(p))
                        : TResult.Complete(s2.Value))(s1, default))(state, value);

            if (result.Complete) return result;
            if (result.Faulted) return result;

            var wait = new AutoResetEvent(false);
            using var enumerator = durations.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current != Duration.Zero) wait.WaitOne((int)enumerator.Current);
                
                result = Morphism.Transform<S>((s1, ma) =>
                    ma.Transform<S>((s2, a) =>
                        Predicate(a)
                            ? reduce(s2, F.Pure<E, A>(a))
                            : TResult.Complete(s2.Value))(s1, default))(state, value);
                
 
                if (result.Complete) return result;
                if (result.Faulted) return result;
            }

            return result;
        };
}

internal sealed record ScheduleFoldUntilTTransducer2E<ST, F, E, A>(
    Transducer<E, K<F, E, A>> Morphism, 
    ST State, 
    Func<ST, A, ST> Fold,
    Func<ST, bool> Predicate,
    Schedule Schedule) : 
    Transducer<E, K<F, E, ST>>
    where F : ApplicativeE<F>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, K<F, E, ST>, TResult<S>> reducer) =>
        (state, value) =>
        {
            using var wait = new AutoResetEvent(false);
            using var durations = Schedule.Run().GetEnumerator();

            TResult<ST> fold(ST x) =>
                Predicate(x) 
                    ? TResult.Complete(x)
                    : TResult.Continue(x);

            var result = Morphism.Transform<ST>(
                (s1, ma) => ma.Transform<ST>(
                    (s2, a) => fold(Fold(s2.Value, a)))(s1, value))(state.SetValue(State), value);
            
            if (result.Faulted) return TResult.Fail<S>(result.ErrorUnsafe);
            if (result.Complete) return reducer(state, F.Pure<E, ST>(State));

            while (durations.MoveNext())
            {
                if (durations.Current != Duration.Zero) wait.WaitOne((int)durations.Current);

                var nresult = Morphism.Transform<ST>(
                    (s1, ma) => ma.Transform<ST>(
                        (s2, a) => fold(Fold(s2.Value, a)))(s1, value))(state.SetValue(result), value);
                
                if (nresult.Faulted) return TResult.Fail<S>(nresult.ErrorUnsafe);
                if (nresult.Complete) return reducer(state, F.Pure<E, ST>(result.ValueUnsafe));
                result = nresult;
            }

            return reducer(state, F.Pure<E, ST>(result.ValueUnsafe));
        };
}

internal sealed record ScheduleFoldUntilTTransducer3<ST, F, E, A>(
    Transducer<E, K<F, E, A>> Morphism, 
    ST State, 
    Func<ST, A, ST> Fold,
    Func<A, bool> Predicate,
    Schedule Schedule) : 
    Transducer<E, K<F, E, ST>>
    where F : ApplicativeE<F>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, K<F, E, ST>, TResult<S>> reducer) =>
        (state, value) =>
        {
            using var wait = new AutoResetEvent(false);
            using var durations = Schedule.Run().GetEnumerator();

            TResult<ST> fold(TState<ST> s, A x) =>
                Predicate(x) 
                    ? TResult.Complete(s.Value)
                    : TResult.Continue(Fold(s, x));

            var result = Morphism.Transform<ST>(
                (s1, ma) => ma.Transform<ST>(fold)(s1, value))(state.SetValue(State), value);
            
            if (result.Faulted) return TResult.Fail<S>(result.ErrorUnsafe);
            if (result.Complete) return reducer(state, F.Pure<E, ST>(State));

            while (durations.MoveNext())
            {
                if (durations.Current != Duration.Zero) wait.WaitOne((int)durations.Current);

                var nresult = Morphism.Transform<ST>(
                    (s1, ma) => ma.Transform<ST>(fold)(s1, value))(state.SetValue(result), value);
                
                if (nresult.Faulted) return TResult.Fail<S>(nresult.ErrorUnsafe);
                if (nresult.Complete) return reducer(state, F.Pure<E, ST>(result.ValueUnsafe));
                result = nresult;
            }

            return reducer(state, F.Pure<E, ST>(result.ValueUnsafe));
        };
}

internal sealed record ScheduleFoldTTransducer2<ST, F, E, A>(
    Transducer<E, K<F, E, A>> Morphism, 
    ST State, 
    Func<ST, A, ST> Fold,
    Schedule Schedule) : 
    Transducer<E, K<F, E, ST>>
    where F : ApplicativeE<F>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, K<F, E, ST>, TResult<S>> reducer) =>
        (state, value) =>
        {
            using var wait = new AutoResetEvent(false);
            using var durations = Schedule.Run().GetEnumerator();

            var result = Morphism.Transform<ST>(
                (s1, ma) => ma.Transform<ST>(
                    (s2, a) => TResult.Continue(Fold(s2.Value, a)))(s1, value))(state.SetValue(State), value);
            
            if (result.Faulted) return TResult.Fail<S>(result.ErrorUnsafe);
            if (result.Complete) return reducer(state, F.Pure<E, ST>(result.ValueUnsafe));

            while (durations.MoveNext())
            {
                if (durations.Current != Duration.Zero) wait.WaitOne((int)durations.Current);
                
                var nresult = Morphism.Transform<ST>(
                    (s1, ma) => ma.Transform<ST>(
                        (s2, a) => TResult.Continue(Fold(s2.Value, a)))(s1, value))(state.SetValue(result), value);
                if (nresult.Faulted) return TResult.Fail<S>(nresult.ErrorUnsafe);
                if (nresult.Complete) return reducer(state, F.Pure<E, ST>(result.ValueUnsafe));
                result = nresult;
            }

            return reducer(state, F.Pure<E, ST>(result.ValueUnsafe));
        };
}


#endif
