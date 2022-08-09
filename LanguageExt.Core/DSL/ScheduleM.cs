using System;
using LanguageExt.TypeClasses;

namespace LanguageExt.DSL;

internal static class ScheduleM
{
    public static Morphism<CoProduct<X, A>, CoProduct<X, B>> Repeat<X, A, B>(
        this Morphism<CoProduct<X, A>, CoProduct<X, B>> ma,
        Schedule schedule) =>
        Morphism.schedule(
            ma,
            schedule,
            Prim<B>.None,
            Morphism.function<B, B, B>((_, x) => x),
            Morphism.function<CoProduct<X, B>, bool>(p => p.IsRight));

    public static Morphism<CoProduct<X, A>, CoProduct<X, B>> RepeatWhile<X, A, B>(
        this Morphism<CoProduct<X, A>, CoProduct<X, B>> ma,
        Schedule schedule,
        Func<B, bool> pred) =>
        Morphism.schedule(
            ma,
            schedule,
            Prim<B>.None,
            Morphism.function<B, B, B>((_, x) => x),
            Morphism.function<CoProduct<X, B>, bool>(p => p is CoProductRight<X, B> r && pred(r.Value)));

    public static Morphism<CoProduct<X, A>, CoProduct<X, B>> RepeatUntil<X, A, B>(
        this Morphism<CoProduct<X, A>, CoProduct<X, B>> ma,
        Schedule schedule,
        Func<B, bool> pred) =>
        ma.RepeatWhile(schedule, x => !pred(x));

    public static Morphism<CoProduct<X, A>, CoProduct<X, B>> Retry<X, A, B>(
        this Morphism<CoProduct<X, A>, CoProduct<X, B>> ma,
        Schedule schedule) =>
        Morphism.schedule(
            ma,
            schedule,
            Prim<B>.None,
            Morphism.function<B, B, B>((_, x) => x),
            Morphism.function<CoProduct<X, B>, bool>(p => p.IsError));

    public static Morphism<CoProduct<X, A>, CoProduct<X, B>> RetryWhile<X, A, B>(
        this Morphism<CoProduct<X, A>, CoProduct<X, B>> ma,
        Schedule schedule,
        Func<X, bool> pred) =>
        Morphism.schedule(
            ma,
            schedule,
            Prim<B>.None,
            Morphism.function<B, B, B>((_, x) => x),
            Morphism.function<CoProduct<X, B>, bool>(p => p is CoProductLeft<X, B> l && pred(l.Value)));

    public static Morphism<CoProduct<X, A>, CoProduct<X, B>> RetryUntil<X, A, B>(
        this Morphism<CoProduct<X, A>, CoProduct<X, B>> ma,
        Schedule schedule,
        Func<X, bool> pred) =>
        ma.RetryWhile(schedule, x => !pred(x));

    public static Morphism<CoProduct<X, A>, CoProduct<X, S>> Fold<X, S, A, B>(
        this Morphism<CoProduct<X, A>, CoProduct<X, B>> ma,
        Schedule schedule,
        S state,
        Func<S, B, S> fold) =>
        Morphism.schedule(
            ma,
            schedule,
            Prim.Pure(state),
            Morphism.function(fold),
            Morphism.function<CoProduct<X, B>, bool>(p => p.IsRight));

    public static Morphism<CoProduct<X, A>, CoProduct<X, S>> FoldWhile<X, S, A, B>(
        this Morphism<CoProduct<X, A>, CoProduct<X, B>> ma,
        Schedule schedule,
        S state,
        Func<S, B, S> fold,
        Func<B, bool> pred) =>
        Morphism.schedule(
            ma,
            schedule,
            Prim.Pure(state),
            Morphism.function(fold),
            Morphism.function<CoProduct<X, B>, bool>(p => p is CoProductRight<X, B> r && pred(r.Value)));

    public static Morphism<CoProduct<X, A>, CoProduct<X, S>> FoldUntil<X, S, A, B>(
        this Morphism<CoProduct<X, A>, CoProduct<X, B>> ma,
        Schedule schedule,
        S state,
        Func<S, B, S> fold,
        Func<B, bool> pred) =>
        ma.FoldWhile(schedule, state, fold, x => !pred(x));

    
    public static Morphism<X, CoProduct<A, B>> Repeat<X, A, B>(
        this Morphism<X, CoProduct<A, B>> ma,
        Schedule schedule) =>
        Morphism.schedule(
            ma,
            schedule,
            Prim<B>.None,
            Morphism.function<B, B, B>((_, x) => x),
            Morphism.function<CoProduct<A, B>, bool>(p => p.IsRight));

    public static Morphism<X, CoProduct<A, B>> RepeatWhile<X, A, B>(
        this Morphism<X, CoProduct<A, B>> ma,
        Schedule schedule,
        Func<B, bool> pred) =>
        Morphism.schedule(
            ma,
            schedule,
            Prim<B>.None,
            Morphism.function<B, B, B>((_, x) => x),
            Morphism.function<CoProduct<A, B>, bool>(p => p is CoProductRight<X, B> r && pred(r.Value)));

    public static Morphism<X, CoProduct<A, B>> RepeatUntil<X, A, B>(
        this Morphism<X, CoProduct<A, B>> ma,
        Schedule schedule,
        Func<B, bool> pred) =>
        ma.RepeatWhile(schedule, x => !pred(x));

    public static Morphism<X, CoProduct<A, B>> Retry<X, A, B>(
        this Morphism<X, CoProduct<A, B>> ma,
        Schedule schedule) =>
        Morphism.schedule(
            ma,
            schedule,
            Prim<B>.None,
            Morphism.function<B, B, B>((_, x) => x),
            Morphism.function<CoProduct<A, B>, bool>(p => p.IsError));

    public static Morphism<X, CoProduct<A, B>> RetryWhile<X, A, B>(
        this Morphism<X, CoProduct<A, B>> ma,
        Schedule schedule,
        Func<A, bool> pred) =>
        Morphism.schedule(
            ma,
            schedule,
            Prim<B>.None,
            Morphism.function<B, B, B>((_, x) => x),
            Morphism.function<CoProduct<A, B>, bool>(p => p is CoProductLeft<A, B> l && pred(l.Value)));

    public static Morphism<X, CoProduct<A, B>> RetryUntil<X, A, B>(
        this Morphism<X, CoProduct<A, B>> ma,
        Schedule schedule,
        Func<A, bool> pred) =>
        ma.RetryWhile(schedule, x => !pred(x));

    public static Morphism<X, CoProduct<A, S>> Fold<X, S, A, B>(
        this Morphism<X, CoProduct<A, B>> ma,
        Schedule schedule,
        S state,
        Func<S, B, S> fold) =>
        Morphism.schedule(
            ma,
            schedule,
            Prim.Pure(state),
            Morphism.function(fold),
            Morphism.function<CoProduct<A, B>, bool>(p => p.IsRight));

    public static Morphism<X, CoProduct<A, S>> FoldWhile<X, S, A, B>(
        this Morphism<X, CoProduct<A, B>> ma,
        Schedule schedule,
        S state,
        Func<S, B, S> fold,
        Func<B, bool> pred) =>
        Morphism.schedule(
            ma,
            schedule,
            Prim.Pure(state),
            Morphism.function(fold),
            Morphism.function<CoProduct<A, B>, bool>(p => p is CoProductRight<X, B> r && pred(r.Value)));

    public static Morphism<X, CoProduct<A, S>> FoldUntil<X, S, A, B>(
        this Morphism<X, CoProduct<A, B>> ma,
        Schedule schedule,
        S state,
        Func<S, B, S> fold,
        Func<B, bool> pred) =>
        ma.FoldWhile(schedule, state, fold, x => !pred(x));

    
}
