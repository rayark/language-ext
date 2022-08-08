using System;
using LanguageExt.TypeClasses;

namespace LanguageExt.DSL;

internal static class ScheduleM
{
    public static Morphism<CoProduct<X, A>, CoProduct<X, B>> Repeat<FailX, X, A, B>(
        this Morphism<CoProduct<X, A>, CoProduct<X, B>> ma,
        Schedule schedule)
        where FailX : struct, Convertable<Exception, X> =>
        Morphism.schedule(
            ma,
            schedule,
            Prim<B>.None,
            Morphism.function<B, B, B>((_, x) => x),
            Morphism.function<CoProduct<X, B>, bool>(p => p.IsRight),
            default(FailX).Convert);

    public static Morphism<CoProduct<X, A>, CoProduct<X, B>> Retry<FailX, X, A, B>(
        this Morphism<CoProduct<X, A>, CoProduct<X, B>> ma,
        Schedule schedule)
        where FailX : struct, Convertable<Exception, X> =>
        Morphism.schedule(
            ma,
            schedule,
            Prim<B>.None,
            Morphism.function<B, B, B>((_, x) => x),
            Morphism.function<CoProduct<X, B>, bool>(p => p.IsLeft),
            default(FailX).Convert);

    public static Morphism<CoProduct<X, A>, CoProduct<X, B>> RepeatWhile<FailX, X, A, B>(
        this Morphism<CoProduct<X, A>, CoProduct<X, B>> ma,
        Schedule schedule, 
        Func<B, bool> pred)
        where FailX : struct, Convertable<Exception, X> =>
        Morphism.schedule(
            ma,
            schedule,
            Prim<B>.None,
            Morphism.function<B, B, B>((_, x) => x),
            Morphism.function<CoProduct<X, B>, bool>(p => p is CoProductRight<X, B> r && pred(r.Value)),
            default(FailX).Convert);

    public static Morphism<CoProduct<X, A>, CoProduct<X, B>> RetryWhile<FailX, X, A, B>(
        this Morphism<CoProduct<X, A>, CoProduct<X, B>> ma,
        Schedule schedule, 
        Func<X, bool> pred)
        where FailX : struct, Convertable<Exception, X> =>
        Morphism.schedule(
            ma,
            schedule,
            Prim<B>.None,
            Morphism.function<B, B, B>((_, x) => x),
            Morphism.function<CoProduct<X, B>, bool>(p => p is CoProductLeft<X, B> l && pred(l.Value)),
            default(FailX).Convert);

    public static Morphism<CoProduct<X, A>, CoProduct<X, B>> RepeaUntil<FailX, X, A, B>(
        this Morphism<CoProduct<X, A>, CoProduct<X, B>> ma,
        Schedule schedule,
        Func<B, bool> pred)
        where FailX : struct, Convertable<Exception, X> =>
        ma.RepeatWhile<FailX, X, A, B>(schedule, x => !pred(x));

    public static Morphism<CoProduct<X, A>, CoProduct<X, B>> RetryUntil<FailX, X, A, B>(
        this Morphism<CoProduct<X, A>, CoProduct<X, B>> ma,
        Schedule schedule, 
        Func<X, bool> pred)
        where FailX : struct, Convertable<Exception, X> =>
        ma.RetryWhile<FailX, X, A, B>(schedule, x => !pred(x));

    public static Morphism<CoProduct<X, A>, CoProduct<X, S>> Fold<FailX, X, S, A, B>(
        this Morphism<CoProduct<X, A>, CoProduct<X, B>> ma,
        Schedule schedule,
        S state,
        Func<S, B, S> fold)
        where FailX : struct, Convertable<Exception, X> =>
        Morphism.schedule(
            ma,
            schedule,
            Prim.Pure(state),
            Morphism.function(fold),
            Morphism.function<CoProduct<X, B>, bool>(p => p.IsRight),
            default(FailX).Convert);    

    public static Morphism<CoProduct<X, A>, CoProduct<X, S>> FoldWhile<FailX, X, S, A, B>(
        this Morphism<CoProduct<X, A>, CoProduct<X, B>> ma,
        Schedule schedule,
        S state,
        Func<S, B, S> fold, 
        Func<B, bool> pred)
        where FailX : struct, Convertable<Exception, X> =>
        Morphism.schedule(
            ma,
            schedule,
            Prim.Pure(state),
            Morphism.function(fold),
            Morphism.function<CoProduct<X, B>, bool>(p => p is CoProductRight<X, B> r && pred(r.Value)),
            default(FailX).Convert);

    public static Morphism<CoProduct<X, A>, CoProduct<X, S>> FoldUntil<FailX, X, S, A, B>(
        this Morphism<CoProduct<X, A>, CoProduct<X, B>> ma,
        Schedule schedule,
        S state,
        Func<S, B, S> fold,
        Func<B, bool> pred)
        where FailX : struct, Convertable<Exception, X> =>
        ma.FoldWhile<FailX, X, S, A, B>(schedule, state, fold, x => !pred(x));
}
