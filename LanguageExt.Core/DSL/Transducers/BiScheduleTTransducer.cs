// TODO: Deprecate if really not needed

/*
#if !NET_STANDARD

using System;
using System.Threading;

namespace LanguageExt.DSL.Transducers;

internal record BiScheduleTTransducer<F, E, X, A>(
    Transducer<E, K<F, CoProduct<X, A>>> Morphism, 
    Schedule Schedule,
    Func<CoProduct<X, A>, bool> Predicate) :
    Transducer<E, K<F, CoProduct<X, A>>>
    where F : Bi<X>.Applicative<F>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, K<F, CoProduct<X, A>>, TResult<S>> reduce) =>
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

internal sealed record BiScheduleFoldUntilTTransducer2<ST, F, E, X, A>(
    Transducer<E, K<F, CoProduct<X, A>>> Morphism, 
    ST State, 
    Func<ST, CoProduct<X, A>, ST> Fold,
    Func<ST, bool> Predicate,
    Schedule Schedule) : 
    Transducer<E, K<F, CoProduct<X, ST>>>
    where F : Bi<X>.Applicative<F>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, K<F, CoProduct<X, ST>>, TResult<S>> reducer) =>
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
            if (result.Complete) return reducer(state, F.Right(State));

            while (durations.MoveNext())
            {
                if (durations.Current != Duration.Zero) wait.WaitOne((int)durations.Current);

                var nresult = Morphism.Transform<ST>(
                    (s1, ma) => ma.Transform<ST>(
                        (s2, a) => fold(Fold(s2.Value, a)))(s1, default))(state.SetValue(result), value);
                
                if (nresult.Faulted) return TResult.Fail<S>(nresult.ErrorUnsafe);
                if (nresult.Complete) return reducer(state, F.Right(result.ValueUnsafe));
                result = nresult;
            }

            return reducer(state, F.Right(result.ValueUnsafe));
        };
}

internal sealed record BiScheduleFoldUntilTTransducer<ST, F, E, X, A>(
    Transducer<E, K<F, CoProduct<X, A>>> Morphism, 
    ST State, 
    Func<ST, CoProduct<X, A>, ST> Fold,
    Func<CoProduct<X, A>, bool> Predicate,
    Schedule Schedule) : 
    Transducer<E, K<F, CoProduct<X, ST>>>
    where F : Bi<X>.Applicative<F>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, K<F, CoProduct<X, ST>>, TResult<S>> reducer) =>
        (state, value) =>
        {
            using var wait = new AutoResetEvent(false);
            using var durations = Schedule.Run().GetEnumerator();

            TResult<ST> fold(TState<ST> s, CoProduct<X, A> x) =>
                Predicate(x) 
                    ? TResult.Complete(s.Value)
                    : TResult.Continue(Fold(s, x));

            var result = Morphism.Transform<ST>(
                (s1, ma) => ma.Transform<ST>(fold)(s1, default))(state.SetValue(State), value);
            
            if (result.Faulted) return TResult.Fail<S>(result.ErrorUnsafe);
            if (result.Complete) return reducer(state, F.Right(State));

            while (durations.MoveNext())
            {
                if (durations.Current != Duration.Zero) wait.WaitOne((int)durations.Current);

                var nresult = Morphism.Transform<ST>(
                    (s1, ma) => ma.Transform<ST>(fold)(s1, default))(state.SetValue(result), value);
                
                if (nresult.Faulted) return TResult.Fail<S>(nresult.ErrorUnsafe);
                if (nresult.Complete) return reducer(state, F.Right(result.ValueUnsafe));
                result = nresult;
            }

            return reducer(state, F.Right(result.ValueUnsafe));
        };
}

internal sealed record BiScheduleFoldTTransducer<ST, F, E, X, A>(
    Transducer<E, K<F, CoProduct<X, A>>> Morphism, 
    ST State, 
    Func<ST, CoProduct<X, A>, ST> Fold,
    Schedule Schedule) : 
    Transducer<E, K<F, CoProduct<X, ST>>>
    where F : Bi<X>.Applicative<F>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, K<F, CoProduct<X, ST>>, TResult<S>> reducer) =>
        (state, value) =>
        {
            using var wait = new AutoResetEvent(false);
            using var durations = Schedule.Run().GetEnumerator();

            var result = Morphism.Transform<ST>(
                (s1, ma) => ma.Transform<ST>(
                    (s2, a) => TResult.Continue(Fold(s2.Value, a)))(s1, default))(state.SetValue(State), value);
            
            if (result.Faulted) return TResult.Fail<S>(result.ErrorUnsafe);
            if (result.Complete) return reducer(state, F.Right(result.ValueUnsafe));

            while (durations.MoveNext())
            {
                if (durations.Current != Duration.Zero) wait.WaitOne((int)durations.Current);
                
                var nresult = Morphism.Transform<ST>(
                    (s1, ma) => ma.Transform<ST>(
                        (s2, a) => TResult.Continue(Fold(s2.Value, a)))(s1, default))(state.SetValue(result), value);
                if (nresult.Faulted) return TResult.Fail<S>(nresult.ErrorUnsafe);
                if (nresult.Complete) return reducer(state, F.Right(result.ValueUnsafe));
                result = nresult;
            }

            return reducer(state, F.Right(result.ValueUnsafe));
        };
}


internal record BiScheduleTTransducer2<F, E, X, A>(
    Transducer<E, K<F, E, CoProduct<X, A>>> Morphism, 
    Schedule Schedule,
    Func<CoProduct<X, A>, bool> Predicate) :
    Transducer<E, K<F, E, CoProduct<X, A>>>
    where F : Bi<X>.Applicative<F, E>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, K<F, E, CoProduct<X, A>>, TResult<S>> reduce) =>
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

internal sealed record BiScheduleFoldUntilTTransducer2E<ST, F, E, X, A>(
    Transducer<E, K<F, E, CoProduct<X, A>>> Morphism, 
    ST State, 
    Func<ST, CoProduct<X, A>, ST> Fold,
    Func<ST, bool> Predicate,
    Schedule Schedule) : 
    Transducer<E, K<F, E, CoProduct<X, ST>>>
    where F : Bi<X>.Applicative<F, E>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, K<F, E, CoProduct<X, ST>>, TResult<S>> reducer) =>
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
            if (result.Complete) return reducer(state, F.Right(State));

            while (durations.MoveNext())
            {
                if (durations.Current != Duration.Zero) wait.WaitOne((int)durations.Current);

                var nresult = Morphism.Transform<ST>(
                    (s1, ma) => ma.Transform<ST>(
                        (s2, a) => fold(Fold(s2.Value, a)))(s1, value))(state.SetValue(result), value);
                
                if (nresult.Faulted) return TResult.Fail<S>(nresult.ErrorUnsafe);
                if (nresult.Complete) return reducer(state, F.Right(result.ValueUnsafe));
                result = nresult;
            }

            return reducer(state, F.Right(result.ValueUnsafe));
        };
}

internal sealed record BiScheduleFoldUntilTTransducer3<ST, F, E, X, A>(
    Transducer<E, K<F, E, CoProduct<X, A>>> Morphism, 
    ST State, 
    Func<ST, CoProduct<X, A>, ST> Fold,
    Func<CoProduct<X, A>, bool> Predicate,
    Schedule Schedule) : 
    Transducer<E, K<F, E, CoProduct<X, ST>>>
    where F : Bi<X>.Applicative<F, E>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, K<F, E, CoProduct<X, ST>>, TResult<S>> reducer) =>
        (state, value) =>
        {
            using var wait = new AutoResetEvent(false);
            using var durations = Schedule.Run().GetEnumerator();

            TResult<ST> fold(TState<ST> s, CoProduct<X, A> x) =>
                Predicate(x) 
                    ? TResult.Complete(s.Value)
                    : TResult.Continue(Fold(s, x));

            var result = Morphism.Transform<ST>(
                (s1, ma) => ma.Transform<ST>(fold)(s1, value))(state.SetValue(State), value);
            
            if (result.Faulted) return TResult.Fail<S>(result.ErrorUnsafe);
            if (result.Complete) return reducer(state, F.Right(State));

            while (durations.MoveNext())
            {
                if (durations.Current != Duration.Zero) wait.WaitOne((int)durations.Current);

                var nresult = Morphism.Transform<ST>(
                    (s1, ma) => ma.Transform<ST>(fold)(s1, value))(state.SetValue(result), value);
                
                if (nresult.Faulted) return TResult.Fail<S>(nresult.ErrorUnsafe);
                if (nresult.Complete) return reducer(state, F.Right(result.ValueUnsafe));
                result = nresult;
            }

            return reducer(state, F.Right(result.ValueUnsafe));
        };
}

internal sealed record BiScheduleFoldTTransducer2<ST, F, E, X, A>(
    Transducer<E, K<F, E, CoProduct<X, A>>> Morphism, 
    ST State, 
    Func<ST, CoProduct<X, A>, ST> Fold,
    Schedule Schedule) : 
    Transducer<E, K<F, E, CoProduct<X, ST>>>
    where F : Bi<X>.Applicative<F, E>
{
    public Func<TState<S>, E, TResult<S>> Transform<S>(Func<TState<S>, K<F, E, CoProduct<X, ST>>, TResult<S>> reducer) =>
        (state, value) =>
        {
            using var wait = new AutoResetEvent(false);
            using var durations = Schedule.Run().GetEnumerator();

            var result = Morphism.Transform<ST>(
                (s1, ma) => ma.Transform<ST>(
                    (s2, a) => TResult.Continue(Fold(s2.Value, a)))(s1, value))(state.SetValue(State), value);
            
            if (result.Faulted) return TResult.Fail<S>(result.ErrorUnsafe);
            if (result.Complete) return reducer(state, F.Right(result.ValueUnsafe));

            while (durations.MoveNext())
            {
                if (durations.Current != Duration.Zero) wait.WaitOne((int)durations.Current);
                
                var nresult = Morphism.Transform<ST>(
                    (s1, ma) => ma.Transform<ST>(
                        (s2, a) => TResult.Continue(Fold(s2.Value, a)))(s1, value))(state.SetValue(result), value);
                if (nresult.Faulted) return TResult.Fail<S>(nresult.ErrorUnsafe);
                if (nresult.Complete) return reducer(state, F.Right(result.ValueUnsafe));
                result = nresult;
            }

            return reducer(state, F.Right(result.ValueUnsafe));
        };
}


#endif
*/
