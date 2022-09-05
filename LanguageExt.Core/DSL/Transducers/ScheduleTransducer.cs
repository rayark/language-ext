using System;
using System.Threading;

namespace LanguageExt.DSL.Transducers;

internal record ScheduleTransducer<A, B>(
    Transducer<A, B> Morphism, 
    Schedule Schedule,
    Func<B, bool> Predicate) :
    Transducer<A, B>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, B, TResult<S>> reduce) =>
        (state, value) =>
        {
            var durations = Schedule.Run();
            
            var result = Morphism.Transform<S>((s, b) => 
                            Predicate(b) 
                                ? reduce(s, b) 
                                : TResult.Complete<S>(s))(state, value);

            if (result.Complete) return result;
            if (result.Faulted) return result;

            var wait = new AutoResetEvent(false);
            using var enumerator = durations.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current != Duration.Zero) wait.WaitOne((int)enumerator.Current);
                
                result = Morphism.Transform<S>((s, b) => 
                    Predicate(b) 
                        ? reduce(s, b) 
                        : TResult.Complete<S>(s))(state, value);

                if (result.Complete) return result;
                if (result.Faulted) return result;
            }

            return result;
        };
}

internal sealed record ScheduleFoldUntilTransducer2<ST, A, B>(
    Transducer<A, B> Morphism,
    ST State, 
    Func<ST, B, ST> Fold,
    Func<ST, bool> Predicate,
    Schedule Schedule) : Transducer<A, ST>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, ST, TResult<S>> reducer) =>
        (state, value) =>
        {
            using var wait = new AutoResetEvent(false);
            using var durations = Schedule.Run().GetEnumerator();

            TResult<ST> fold(ST x) =>
                Predicate(x) 
                    ? TResult.Complete(x)
                    : TResult.Continue(x);

            var result = Morphism.Transform<ST>((s, b) => fold(Fold(s, b)))(state.SetValue(State), value);
            if (result.Faulted) return TResult.Fail<S>(result.ErrorUnsafe);
            if (result.Complete) return reducer(state, State);

            while (durations.MoveNext())
            {
                if (durations.Current != Duration.Zero) wait.WaitOne((int)durations.Current);
                var nresult = Morphism.Transform<ST>((s, b) => fold(Fold(s, b)))(state.SetValue(result), value);
                if (nresult.Faulted) return TResult.Fail<S>(nresult.ErrorUnsafe);
                if (nresult.Complete) return reducer(state, result.ValueUnsafe);
                result = nresult;
            }

            return reducer(state, result.ValueUnsafe);
        };

}

internal sealed record ScheduleFoldWhileTransducer2<ST, A, B>(
    Transducer<A, B> Morphism,
    ST State, 
    Func<ST, B, ST> Fold,
    Func<ST, bool> Predicate,
    Schedule Schedule
    ) : Transducer<A, ST>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, ST, TResult<S>> reducer) =>
        (state, value) =>
        {
            using var wait = new AutoResetEvent(false);
            using var durations = Schedule.Run().GetEnumerator();

            TResult<ST> fold(ST x) =>
                Predicate(x) 
                    ? TResult.Continue(x) 
                    : TResult.Complete(x);

            var result = Morphism.Transform<ST>((s, b) => fold(Fold(s, b)))(state.SetValue(State), value);
            if (result.Faulted) return TResult.Fail<S>(result.ErrorUnsafe);
            if (result.Complete) return reducer(state, State);

            while (durations.MoveNext())
            {
                if (durations.Current != Duration.Zero) wait.WaitOne((int)durations.Current);
                var nresult = Morphism.Transform<ST>((s, b) => fold(Fold(s, b)))(state.SetValue(result), value);
                if (nresult.Faulted) return TResult.Fail<S>(nresult.ErrorUnsafe);
                if (nresult.Complete) return reducer(state, result.ValueUnsafe);
                result = nresult;
            }

            return reducer(state, result.ValueUnsafe);
        };
}

internal sealed record ScheduleFoldUntilTransducer<ST, A, B>(
    Transducer<A, B> Morphism,
    ST State, 
    Func<ST, B, ST> Fold,
    Func<B, bool> Predicate,
    Schedule Schedule) : Transducer<A, ST>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, ST, TResult<S>> reducer) =>
        (state, value) =>
        {
            using var wait = new AutoResetEvent(false);
            using var durations = Schedule.Run().GetEnumerator();

            TResult<ST> fold(TState<ST> s, B b) =>
                Predicate(b)
                    ? TResult.Complete(s.Value)
                    : TResult.Continue(Fold(s, b));

            var result = Morphism.Transform<ST>(fold)(state.SetValue(State), value);
            if (result.Faulted) return TResult.Fail<S>(result.ErrorUnsafe);
            if (result.Complete) return reducer(state, State);

            while (durations.MoveNext())
            {
                if (durations.Current != Duration.Zero) wait.WaitOne((int)durations.Current);
                var nresult = Morphism.Transform<ST>(fold)(state.SetValue(result), value);
                if (nresult.Faulted) return TResult.Fail<S>(nresult.ErrorUnsafe);
                if (nresult.Complete) return reducer(state, result.ValueUnsafe);
                result = nresult;
            }

            return reducer(state, result.ValueUnsafe);
        };
}

internal sealed record ScheduleFoldWhileTransducer<ST, A, B>(
    Transducer<A, B> Morphism,
    ST State, 
    Func<ST, B, ST> Fold,
    Func<B, bool> Predicate,
    Schedule Schedule
    ) : Transducer<A, ST>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, ST, TResult<S>> reducer) =>
        (state, value) =>
        {
            using var wait = new AutoResetEvent(false);
            using var durations = Schedule.Run().GetEnumerator();

            TResult<ST> fold(TState<ST> s, B b) =>
                Predicate(b) 
                    ? TResult.Continue(Fold(s, b)) 
                    : TResult.Complete(s.Value);

            var result = Morphism.Transform<ST>(fold)(state.SetValue(State), value);
            if (result.Faulted) return TResult.Fail<S>(result.ErrorUnsafe);
            if (result.Complete) return reducer(state, State);

            while (durations.MoveNext())
            {
                if (durations.Current != Duration.Zero) wait.WaitOne((int)durations.Current);
                var nresult = Morphism.Transform<ST>(fold)(state.SetValue(result), value);
                if (nresult.Faulted) return TResult.Fail<S>(nresult.ErrorUnsafe);
                if (nresult.Complete) return reducer(state, result.ValueUnsafe);
                result = nresult;
            }

            return reducer(state, result.ValueUnsafe);
        };
}

internal sealed record ScheduleFoldTransducer<ST, A, B>(
    Transducer<A, B> Morphism,
    ST State, 
    Func<ST, B, ST> Fold,
    Schedule Schedule
    ) : Transducer<A, ST>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, ST, TResult<S>> reducer) =>
        (state, value) =>
        {
            using var wait = new AutoResetEvent(false);
            using var durations = Schedule.Run().GetEnumerator();

            var result = Morphism.Transform<ST>((s, b) => TResult.Continue(Fold(s, b)))(state.SetValue(State), value);
            if (result.Faulted) return TResult.Fail<S>(result.ErrorUnsafe);
            if (result.Complete) return reducer(state, State);

            while (durations.MoveNext())
            {
                if (durations.Current != Duration.Zero) wait.WaitOne((int)durations.Current);
                var nresult = Morphism.Transform<ST>((s, b) => TResult.Continue(Fold(s, b)))(state.SetValue(result), value);
                if (nresult.Faulted) return TResult.Fail<S>(nresult.ErrorUnsafe);
                if (nresult.Complete) return reducer(state, result.ValueUnsafe);
                result = nresult;
            }

            return reducer(state, result.ValueUnsafe);
        };
}
