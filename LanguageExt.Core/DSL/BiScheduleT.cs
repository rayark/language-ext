/*
using System;
using LanguageExt.DSL.Transducers;

namespace LanguageExt.DSL;

internal static class BiScheduleT
{
    public static Transducer<E, K<F, CoProduct<X, A>>> RepeatT<F, E, X, A>(
        this Transducer<E, K<F, CoProduct<X, A>>> ma,
        Schedule schedule) where F : Bi<X>.Applicative<F> =>
        Transducer.bischeduleT(ma, schedule, p => p.IsRight);

    public static Transducer<E, K<F, CoProduct<X, A>>> RepeatWhileT<F, E, X, A>(
        this Transducer<E, K<F, CoProduct<X, A>>> ma,
        Schedule schedule,
        Func<A, bool> pred) where F : Bi<X>.Applicative<F> =>
        Transducer.bischeduleT(ma, schedule, p => p is CoProductRight<X, A> r && pred(r.Value));

    public static Transducer<E, K<F, CoProduct<X, A>>> RepeatUntilT<F, E, X, A>(
        this Transducer<E, K<F, CoProduct<X, A>>> ma,
        Schedule schedule,
        Func<A, bool> pred) where F : Bi<X>.Applicative<F> =>
        Transducer.bischeduleT(ma, schedule, p => p is CoProductRight<X, A> r && !pred(r.Value));


    public static Transducer<E, K<F, CoProduct<X, A>>> RetryT<F, E, X, A>(
        this Transducer<E, K<F, CoProduct<X, A>>> ma,
        Schedule schedule) where F : Bi<X>.Applicative<F> =>
        Transducer.bischeduleT(ma, schedule, p => p.IsError);

    public static Transducer<E, K<F, CoProduct<X, A>>> RetryWhileT<F, E, X, A>(
        this Transducer<E, K<F, CoProduct<X, A>>> ma,
        Schedule schedule,
        Func<X, bool> pred) where F : Bi<X>.Applicative<F> =>
        Transducer.bischeduleT(ma, schedule, p => p is CoProductLeft<X, A> l && pred(l.Value));

    public static Transducer<E, K<F, CoProduct<X, A>>> RetryUntilT<F, E, X, A>(
        this Transducer<E, K<F, CoProduct<X, A>>> ma,
        Schedule schedule,
        Func<X, bool> pred) where F : Bi<X>.Applicative<F> =>
        Transducer.bischeduleT(ma, schedule, p => p is CoProductLeft<X, A> l && !pred(l.Value));


    public static Transducer<E, K<F, CoProduct<X, S>>> FoldT<S, F, E, X, A>(
        this Transducer<E, K<F, CoProduct<X, A>>> ma,
        Schedule schedule,
        S state,
        Func<S, CoProduct<X, A>, S> fold) where F : Bi<X>.Applicative<F> =>
        Transducer.bifoldT(ma, state, fold, schedule);

    public static Transducer<E, K<F, CoProduct<X, S>>> FoldWhileT<S, F, E, X, A>(
        this Transducer<E, K<F, CoProduct<X, A>>> ma,
        Schedule schedule,
        S state,
        Func<S, CoProduct<X, A>, S> fold,
        Func<CoProduct<X, A>, bool> predicate) where F : Bi<X>.Applicative<F> =>
        Transducer.bifoldWhileT(ma, state, fold, predicate, schedule);

    public static Transducer<E, K<F, CoProduct<X, S>>> FoldUntilT<S, F, E, X, A>(
        this Transducer<E, K<F, CoProduct<X, A>>> ma,
        Schedule schedule,
        S state,
        Func<S, CoProduct<X, A>, S> fold,
        Func<CoProduct<X, A>, bool> predicate) where F : Bi<X>.Applicative<F> =>
        Transducer.bifoldUntilT(ma, state, fold, predicate, schedule);


    public static Transducer<E, K<F, CoProduct<X, S>>> FoldWhileT2<S, F, E, X, A>(
        this Transducer<E, K<F, CoProduct<X, A>>> ma,
        Schedule schedule,
        S state,
        Func<S, CoProduct<X, A>, S> fold,
        Func<S, bool> predicate) where F : Bi<X>.Applicative<F> =>
        Transducer.bifoldWhileT2(ma, state, fold, predicate, schedule);

    public static Transducer<E, K<F, CoProduct<X, S>>> FoldUntilT2<S, F, E, X, A>(
        this Transducer<E, K<F, CoProduct<X, A>>> ma,
        Schedule schedule,
        S state,
        Func<S, CoProduct<X, A>, S> fold,
        Func<S, bool> predicate) where F : Bi<X>.Applicative<F> =>
        Transducer.bifoldUntilT2(ma, state, fold, predicate, schedule);
}
*/
