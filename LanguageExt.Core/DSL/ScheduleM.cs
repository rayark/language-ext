#nullable enable
using System;
using LanguageExt.DSL.Transducers;

namespace LanguageExt.DSL;

internal static class ScheduleM
{
    public static Transducer<A, CoProduct<X, B>> Repeat<X, A, B>(
        this Transducer<A, CoProduct<X, B>> ma,
        Schedule schedule) =>
        Transducer.schedule(ma, schedule, p => p.IsRight);

    public static Transducer<A, CoProduct<X, B>> RepeatWhile<X, A, B>(
        this Transducer<A, CoProduct<X, B>> ma,
        Schedule schedule,
        Func<B, bool> pred) =>
        Transducer.schedule(ma, schedule, p => p is CoProductRight<X, B> r && pred(r.Value));

    public static Transducer<A, CoProduct<X, B>> RepeatUntil<X, A, B>(
        this Transducer<A, CoProduct<X, B>> ma,
        Schedule schedule,
        Func<B, bool> pred) =>
        Transducer.schedule(ma, schedule, p => p is CoProductRight<X, B> r && !pred(r.Value));


    public static Transducer<A, CoProduct<X, B>> Retry<X, A, B>(
        this Transducer<A, CoProduct<X, B>> ma,
        Schedule schedule) =>
        Transducer.schedule(ma, schedule, p => p.IsError);

    public static Transducer<A, CoProduct<X, B>> RetryWhile<X, A, B>(
        this Transducer<A, CoProduct<X, B>> ma,
        Schedule schedule,
        Func<X, bool> pred) =>
        Transducer.schedule(
            ma,
            schedule,
            p => p is CoProductLeft<X, B> l && pred(l.Value));

    public static Transducer<A, CoProduct<X, B>> RetryUntil<X, A, B>(
        this Transducer<A, CoProduct<X, B>> ma,
        Schedule schedule,
        Func<X, bool> pred) =>
        Transducer.schedule(
            ma,
            schedule,
            p => p is CoProductLeft<X, B> l && !pred(l.Value));


    /*public static Transducer<A, S> Fold<S, A, B>(
        this Transducer<A, B> ma,
        Schedule schedule,
        S state,
        Func<S, B, S> fold) =>
        Transducer.fold(ma, state, fold, schedule);*/

    public static Transducer<A, S> FoldWhile<S, A, B>(
        this Transducer<A, B> ma,
        Schedule schedule,
        S state,
        Func<S, B, S> fold,
        Func<B, bool> predicate) =>
        Transducer.foldWhile(ma, state, fold, predicate, schedule);

    public static Transducer<A, S> FoldUntil2<S, A, B>(
        this Transducer<A, B> ma,
        Schedule schedule,
        S state,
        Func<S, B, S> fold,
        Func<B, bool> predicate) =>
        Transducer.foldUntil(ma, state, fold, predicate, schedule);

    public static Transducer<A, S> FoldWhile2<S, A, B>(
        this Transducer<A, B> ma,
        Schedule schedule,
        S state,
        Func<S, B, S> fold,
        Func<S, bool> predicate) =>
        Transducer.foldWhile2(ma, state, fold, predicate, schedule);

    public static Transducer<A, S> FoldUntil2<S, A, B>(
        this Transducer<A, B> ma,
        Schedule schedule,
        S state,
        Func<S, B, S> fold,
        Func<S, bool> predicate) =>
        Transducer.foldUntil2(ma, state, fold, predicate, schedule);


    public static Transducer<A, CoProduct<X, S>> Fold<X, S, A, B>(
        this Transducer<A, CoProduct<X, B>> ma,
        Schedule schedule,
        S state,
        Func<S, B, S> fold) =>
        Transducer.compose(
            Transducer.foldWhile(
                ma,
                state,
                (s, p) => p is CoProductRight<X, B> r ? fold(s, r.Value) : s,
                p => p.IsRight,
                schedule),
            Transducer.right<X, S>());

    public static Transducer<A, CoProduct<X, S>> FoldWhile<X, S, A, B>(
        this Transducer<A, CoProduct<X, B>> ma,
        Schedule schedule,
        S state,
        Func<S, B, S> fold,
        Func<B, bool> pred) =>
        Transducer.compose(
            Transducer.foldWhile(
                ma,
                state,
                (s, p) => p is CoProductRight<X, B> r ? fold(s, r.Value) : s,
                p => p is CoProductRight<X, B> r && pred(r.Value),
                schedule),
            Transducer.right<X, S>());

    public static Transducer<A, CoProduct<X, S>> FoldUntil<X, S, A, B>(
        this Transducer<A, CoProduct<X, B>> ma,
        Schedule schedule,
        S state,
        Func<S, B, S> fold,
        Func<B, bool> pred) =>
        Transducer.compose(
            Transducer.foldUntil(
                ma,
                state,
                (s, p) => p is CoProductRight<X, B> r ? fold(s, r.Value) : s,
                p => p is CoProductRight<X, B> r && pred(r.Value),
                schedule),
            Transducer.right<X, S>());

    public static Transducer<A, CoProduct<X, S>> FoldWhile2<X, S, A, B>(
        this Transducer<A, CoProduct<X, B>> ma,
        Schedule schedule,
        S state,
        Func<S, B, S> fold,
        Func<S, bool> pred) =>
        Transducer.compose(
            Transducer.foldWhile2(
                ma,
                state,
                (s, p) => p is CoProductRight<X, B> r ? fold(s, r.Value) : s,
                pred,
                schedule),
            Transducer.right<X, S>());

    public static Transducer<A, CoProduct<X, S>> FoldUntil2<X, S, A, B>(
        this Transducer<A, CoProduct<X, B>> ma,
        Schedule schedule,
        S state,
        Func<S, B, S> fold,
        Func<S, bool> pred) =>
        Transducer.compose(
            Transducer.foldUntil2(
                ma,
                state,
                (s, p) => p is CoProductRight<X, B> r ? fold(s, r.Value) : s,
                pred,
                schedule),
            Transducer.right<X, S>());
}

