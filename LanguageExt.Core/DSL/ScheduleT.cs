#if !NET_STANDARD
#nullable enable
using System;
using LanguageExt.DSL.Transducers;

namespace LanguageExt.DSL;

internal static class ScheduleT
{
    public static Transducer<E, K<F, CoProduct<X, A>>> RepeatT<F, E, X, A>(
        this Transducer<E, K<F, CoProduct<X, A>>> ma,
        Schedule schedule) where F : Applicative<F> =>
        Transducer.scheduleT(ma, schedule, p => p.IsRight);

    public static Transducer<E, K<F, CoProduct<X, A>>> RepeatWhileT<F, E, X, A>(
        this Transducer<E, K<F, CoProduct<X, A>>> ma,
        Schedule schedule,
        Func<A, bool> pred) where F : Applicative<F> =>
        Transducer.scheduleT(ma, schedule, p => p is CoProductRight<X, A> r && pred(r.Value));

    public static Transducer<E, K<F, CoProduct<X, A>>> RepeatUntilT<F, E, X, A>(
        this Transducer<E, K<F, CoProduct<X, A>>> ma,
        Schedule schedule,
        Func<A, bool> pred) where F : Applicative<F> =>
        Transducer.scheduleT(ma, schedule, p => p is CoProductRight<X, A> r && !pred(r.Value));


    public static Transducer<E, K<F, CoProduct<X, A>>> RetryT<F, E, X, A>(
        this Transducer<E, K<F, CoProduct<X, A>>> ma,
        Schedule schedule) where F : Applicative<F> =>
        Transducer.scheduleT(ma, schedule, p => p.IsError);

    public static Transducer<E, K<F, CoProduct<X, A>>> RetryWhileT<F, E, X, A>(
        this Transducer<E, K<F, CoProduct<X, A>>> ma,
        Schedule schedule,
        Func<X, bool> pred) where F : Applicative<F> =>
        Transducer.scheduleT(ma, schedule, p => p is CoProductLeft<X, A> l && pred(l.Value));

    public static Transducer<E, K<F, CoProduct<X, A>>> RetryUntilT<F, E, X, A>(
        this Transducer<E, K<F, CoProduct<X, A>>> ma,
        Schedule schedule,
        Func<X, bool> pred) where F : Applicative<F> =>
        Transducer.scheduleT(ma, schedule, p => p is CoProductLeft<X, A> l && !pred(l.Value));


    public static Transducer<E, K<F, S>> FoldT<S, F, E, A>(
        this Transducer<E, K<F, A>> ma,
        Schedule schedule,
        S state,
        Func<S, A, S> fold) where F : Applicative<F> =>
        Transducer.foldT(ma, state, fold, schedule);

    public static Transducer<E, K<F, S>> FoldWhileT<S, F, E, A>(
        this Transducer<E, K<F, A>> ma,
        Schedule schedule,
        S state,
        Func<S, A, S> fold,
        Func<A, bool> predicate) where F : Applicative<F> =>
        Transducer.foldWhileT(ma, state, fold, predicate, schedule);

    public static Transducer<E, K<F, S>> FoldUntilT<S, F, E, A>(
        this Transducer<E, K<F, A>> ma,
        Schedule schedule,
        S state,
        Func<S, A, S> fold,
        Func<A, bool> predicate) where F : Applicative<F> =>
        Transducer.foldUntilT(ma, state, fold, predicate, schedule);


    public static Transducer<E, K<F, S>> FoldWhileT2<S, F, E, A>(
        this Transducer<E, K<F, A>> ma,
        Schedule schedule,
        S state,
        Func<S, A, S> fold,
        Func<S, bool> predicate) where F : Applicative<F> =>
        Transducer.foldWhileT2(ma, state, fold, predicate, schedule);

    public static Transducer<E, K<F, S>> FoldUntilT2<S, F, E, A>(
        this Transducer<E, K<F, A>> ma,
        Schedule schedule,
        S state,
        Func<S, A, S> fold,
        Func<S, bool> predicate) where F : Applicative<F> =>
        Transducer.foldUntilT2(ma, state, fold, predicate, schedule);
}
#endif
