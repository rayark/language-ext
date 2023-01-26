using System;
using static LanguageExt.DSL2.TransducerAsync;

namespace LanguageExt.DSL2;

public static class TransducerAsyncExtensions
{
    public static TransducerAsync<A, C> Map<A, B, C>(this TransducerAsync<A, B> f, TransducerAsync<B, C> g) =>
        compose(f, g);
    
    public static TransducerAsync<A, C> Map<A, B, C>(this TransducerAsync<A, B> f, Func<B, C> g) =>
        compose(f, map(g));
    
    public static TransducerAsync<A, C> Select<A, B, C>(this TransducerAsync<A, B> f, TransducerAsync<B, C> g) =>
        compose(f, g);
    
    public static TransducerAsync<A, C> Select<A, B, C>(this TransducerAsync<A, B> f, Func<B, C> g) =>
        compose(f, map(g));
    
    public static TransducerAsync<A, B> Filter<A, B>(this TransducerAsync<A, B> f, TransducerAsync<B, bool> g) =>
        compose(f, filter(g));
    
    public static TransducerAsync<A, B> Filter<A, B>(this TransducerAsync<A, B> f, Func<B, bool> g) =>
        compose(f, filter(g));
    
    public static TransducerAsync<A, B> Where<A, B>(this TransducerAsync<A, B> f, TransducerAsync<B, bool> g) =>
        compose(f, filter(g));
    
    public static TransducerAsync<A, B> Where<A, B>(this TransducerAsync<A, B> f, Func<B, bool> g) =>
        compose(f, filter(g));
     
    public static TransducerAsync<E, B> Action<E, A, B>(
        this TransducerAsync<E, A> fa, 
        TransducerAsync<E, B> fb) =>
        action(fa, fb);

    public static TransducerAsync<E, B> Apply<E, A, B>(
        this TransducerAsync<E, TransducerAsync<A, B>> ff, 
        TransducerAsync<E, A> fa) =>
        apply(ff, fa);
    
    public static TransducerAsync<E, B> Apply<E, A, B>(
        this TransducerAsync<E, Transducer<A, B>> ff, 
        TransducerAsync<E, A> fa) =>
        apply(ff, fa);
    
    public static TransducerAsync<E, B> Apply<E, A, B>(
        this TransducerAsync<E, Func<A, B>> ff, 
        TransducerAsync<E, A> fa) =>
        apply(ff, fa);

    public static TransducerAsync<A, B> Flatten<A, B>(
        this TransducerAsync<A, TransducerAsync<A, B>> f) => 
        flatten(f);
        
    public static TransducerAsync<A, B> Flatten<A, B>(
        this TransducerAsync<A, TransducerAsync<Unit, B>> f) => 
        flatten(f);
 
    public static TransducerAsync<A, B> Flatten<A, B>(
        this TransducerAsync<A, Transducer<A, B>> f) => 
        flatten(f);
        
    public static TransducerAsync<A, B> Flatten<A, B>(
        this TransducerAsync<A, Transducer<Unit, B>> f) => 
        flatten(f);

    public static TransducerAsync<E, B> Bind<E, A, B>(this TransducerAsync<E, A> fa, TransducerAsync<A, TransducerAsync<E, B>> fab) =>
        bind(fa, fab);

    public static TransducerAsync<E, B> Bind<E, A, B>(this TransducerAsync<E, A> fa, Func<A, TransducerAsync<E, B>> fab) =>
        bind(fa, fab);

    public static TransducerAsync<E, B> SelectMany<E, A, B>(this TransducerAsync<E, A> fa, TransducerAsync<A, TransducerAsync<E, B>> fab) =>
        bind(fa, fab);

    public static TransducerAsync<E, B> SelectMany<E, A, B>(this TransducerAsync<E, A> fa, Func<A, TransducerAsync<E, B>> fab) =>
        bind(fa, fab);

    public static TransducerAsync<E, C> SelectMany<E, A, B, C>(
        this TransducerAsync<E, A> fa, 
        TransducerAsync<A, TransducerAsync<E, B>> fab,
        TransducerAsync<A, TransducerAsync<B, C>> fabc) =>
        bindMap(fa, fab, fabc);

    public static TransducerAsync<E, C> SelectMany<E, A, B, C>(
        this TransducerAsync<E, A> fa, 
        TransducerAsync<A, TransducerAsync<E, B>> fab,
        Func<A, B, C> fabc) =>
        bindMap(fa, fab, fabc);

    public static TransducerAsync<E, C> SelectMany<E, A, B, C>(
        this TransducerAsync<E, A> fa, 
        Func<A, TransducerAsync<E, B>> fab,
        Func<A, B, C> fabc) =>
        bindMap(fa, fab, fabc);

    public static TransducerAsync<E, B> Fold<E, A, B>(
        this TransducerAsync<E, A> ta, 
        TransducerAsync<A, TransducerAsync<B, B>> f, 
        B state) =>
        fold(f, state, ta);

    public static TransducerAsync<E, B> Fold<E, A, B>(
        this TransducerAsync<E, A> ta, 
        TransducerAsync<A, Transducer<B, B>> f, 
        B state) =>
        fold(f, state, ta);

    public static TransducerAsync<E, B> Fold<E, A, B>(
        this TransducerAsync<E, A> ta, 
        Func<B, A, B> f, 
        B state) =>
        fold((a, b) => f(b, a), state, ta);    
}
