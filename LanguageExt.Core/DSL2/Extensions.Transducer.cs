using System;
using static LanguageExt.DSL2.Transducer;

namespace LanguageExt.DSL2;

public static partial class TransducerExtensions
{
    public static Transducer<A, C> Map<A, B, C>(this Transducer<A, B> f, Transducer<B, C> g) =>
        compose(f, g);
    
    public static Transducer<A, C> Map<A, B, C>(this Transducer<A, B> f, Func<B, C> g) =>
        compose(f, map(g));
    
    public static Transducer<A, C> Select<A, B, C>(this Transducer<A, B> f, Transducer<B, C> g) =>
        compose(f, g);
    
    public static Transducer<A, C> Select<A, B, C>(this Transducer<A, B> f, Func<B, C> g) =>
        compose(f, map(g));
    
    public static Transducer<A, B> Filter<A, B>(this Transducer<A, B> f, Transducer<B, bool> g) =>
        compose(f, filter(g));
    
    public static Transducer<A, B> Filter<A, B>(this Transducer<A, B> f, Func<B, bool> g) =>
        compose(f, filter(g));
    
    public static Transducer<A, B> Where<A, B>(this Transducer<A, B> f, Transducer<B, bool> g) =>
        compose(f, filter(g));
    
    public static Transducer<A, B> Where<A, B>(this Transducer<A, B> f, Func<B, bool> g) =>
        compose(f, filter(g));
    
    public static Transducer<E, B> Action<E, A, B>(
        this Transducer<E, A> fa, 
        Transducer<E, B> fb) =>
        action(fa, fb);

    public static Transducer<E, B> Apply<E, A, B>(
        this Transducer<E, Transducer<A, B>> ff, 
        Transducer<E, A> fa) =>
        apply(ff, fa);
    
    public static Transducer<E, B> Apply<E, A, B>(
        this Transducer<E, Func<A, B>> ff, 
        Transducer<E, A> fa) =>
        apply(ff, fa);

    public static Transducer<A, B> Flatten<A, B>(this Transducer<A, Transducer<A, B>> ffa) =>
        flatten(ffa);

    public static Transducer<A, B> Flatten<A, B>(this Transducer<A, Transducer<Unit, B>> ffa) =>
        flatten(ffa);

    public static Transducer<E, B> Bind<E, A, B>(this Transducer<E, A> fa, Transducer<A, Transducer<E, B>> fab) =>
        bind(fa, fab);

    public static Transducer<E, B> Bind<E, A, B>(this Transducer<E, A> fa, Func<A, Transducer<E, B>> fab) =>
        bind(fa, fab);

    public static Transducer<E, B> SelectMany<E, A, B>(this Transducer<E, A> fa, Transducer<A, Transducer<E, B>> fab) =>
        bind(fa, fab);

    public static Transducer<E, B> SelectMany<E, A, B>(this Transducer<E, A> fa, Func<A, Transducer<E, B>> fab) =>
        bind(fa, fab);

    public static Transducer<E, C> SelectMany<E, A, B, C>(
        this Transducer<E, A> fa, 
        Transducer<A, Transducer<E, B>> fab,
        Transducer<A, Transducer<B, C>> fabc) =>
        bindMap(fa, fab, fabc);

    public static Transducer<E, C> SelectMany<E, A, B, C>(
        this Transducer<E, A> fa, 
        Transducer<A, Transducer<E, B>> fab,
        Func<A, B, C> fabc) =>
        bindMap(fa, fab, fabc);

    public static Transducer<E, C> SelectMany<E, A, B, C>(
        this Transducer<E, A> fa, 
        Func<A, Transducer<E, B>> fab,
        Func<A, B, C> fabc) =>
        bindMap(fa, fab, fabc);

    public static Transducer<E, B> Fold<E, A, B>(
        this Transducer<E, A> ta,
        Transducer<A, Transducer<B, B>> f, B state) =>
        fold(f, state, ta);

    public static Transducer<E, B> Fold<E, A, B>(
        this Transducer<E, A> ta,
        Func<B, A, B> f, B state) =>
        fold((a, b) => f(b, a), state, ta);

    public static Transducer<E, B> FoldWhile<E, A, B>(
        Transducer<A, Transducer<B, B>> fold, 
        B state, 
        Transducer<E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        foldWhile(fold, state, ta, pred, schedule);

    public static Transducer<E, B> FoldWhile<E, A, B>(
        Func<A, B, B> fold, 
        B state, 
        Transducer<E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        foldWhile(fold, state, ta, pred, schedule);

    public static Transducer<E, B> FoldUntil<E, A, B>(
        Transducer<A, Transducer<B, B>> fold, 
        B state, 
        Transducer<E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        foldUntil(fold, state, ta, pred, schedule);

    public static Transducer<E, B> FoldUntil<E, A, B>(
        Func<A, B, B> fold, 
        B state, 
        Transducer<E, A> ta,
        Func<A, bool> pred,
        Schedule schedule) =>
        foldUntil(fold, state, ta, pred, schedule);
}
