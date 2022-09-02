#nullable enable
using System;
using System.Threading;
using System.Collections.Generic;
using static LanguageExt.Prelude;
using LanguageExt.DSL.Transducers;

namespace LanguageExt.DSL;

public static class Obj
{
    public static Obj<A> Pure<A>(A value) =>
        new PureObj<A, A>(value, Transducer<A>.identity);
    
    public static Obj<A> Many<A>(IEnumerable<A> value) =>
        new ManyObj<A, A>(value, Transducer<A>.identity);
    
    public static Obj<A> Flatten<A>(this Obj<Obj<A>> mma) =>
        mma.Bind(static x => x);

    public static Obj<CoProduct<Y, B>> BiMap<X, Y, A, B>(
        this Obj<CoProduct<X, A>> obj,
        BiTransducer<X, Y, A, B> transducer) =>
        obj.Map(co => co.Transduce(transducer) switch
        {
            var x when x.Faulted => CoProduct.Fail<Y, B>(x.ErrorUnsafe),
            var x => x.ValueUnsafe,
        });

    public static Obj<CoProduct<Y, B>> BiMap<X, Y, A, B>(
        this Obj<CoProduct<X, A>> obj,
        Func<X, Y> Left,
        Func<A, B> Right) =>
        obj.BiMap(BiTransducer.bimap(Left, Right));
    
    internal static TResult<Prim<A>> MapReduce<A>(TState<Prim<A>> state, A source) =>
        TResult.Continue(state + Prim.Pure(source));

    internal static TResult<Option<A>> MapNoReduce<A>(TState<Option<A>> _, A source) =>
        TResult.Continue(Some(source));
}

public abstract record Obj<A>
{
    public Prim<A> Run()
    {
        var state = TState<Prim<A>>.Create(Prim<A>.None);
        try
        {
            return Transduce(state, Obj.MapReduce).Match(
                Continue: static x => x,
                Complete: static x => x,
                Fail: Prim.Fail<A>);
        }
        catch (Exception e)
        {
            return Prim.Fail<A>(e);
        }
        finally
        {
            state.CleanUp();
        }
    }

    public abstract TResult<S> Transduce<S>(TState<S> seed, Func<TState<S>, A, TResult<S>> reducer);

    public Obj<B> Map<B>(Func<A, B> f) =>
        Map(Transducer.map(f));
    
    public abstract Obj<B> Map<B>(Transducer<A, B> f);
    public abstract Obj<B> Bind<B>(Func<A, Obj<B>> f);
}

public sealed record PureObj<X, A>(X Value, Transducer<X, A> Next) : Obj<A>
{
    public override TResult<S> Transduce<S>(TState<S> seed, Func<TState<S>, A, TResult<S>> reducer) =>
        Next.Transform(reducer)(seed, Value);

    public override Obj<B> Map<B>(Transducer<A, B> f) =>
        new PureObj<X, B>(Value, Transducer.compose(Next, f));

    public override Obj<B> Bind<B>(Func<A, Obj<B>> f) =>
        new PureObj<X, B>(Value, 
            Transducer.compose(
                Transducer.compose(Next, Transducer.map(f)),
                Transducer<B>.join));
}

public sealed record ManyObj<X, A>(IEnumerable<X> Values, Transducer<X, A> Next) : Obj<A>
{
    public override TResult<S> Transduce<S>(TState<S> seed, Func<TState<S>, A, TResult<S>> reducer)
    {
        var res = seed;
        var red = Next.Transform(reducer);
        
        foreach (var value in Values)
        {
            var res1 = red(res, value);
            if(res1.Complete) return TResult.Continue(res.Value);
            res = res.SetValue(res1);
        }
        return TResult.Continue(res.Value);
    }

    public override Obj<B> Map<B>(Transducer<A, B> f) =>
        new ManyObj<X, B>(Values, Transducer.compose(Next, f));
    
    public override Obj<B> Bind<B>(Func<A, Obj<B>> f) =>
        new ManyObj<X, B>(Values, 
            Transducer.compose(
                Transducer.compose(Next, Transducer.map(f)),
                Transducer<B>.join));
}

public sealed record ObservableObj<X, A>(IObservable<X> Values, Transducer<X, A> Next) : Obj<A>
{
    public override TResult<S> Transduce<S>(TState<S> seed, Func<TState<S>, A, TResult<S>> reducer)
    {
        var obs = new ObservableTransducer<S>(Values, Next, reducer, seed);
        var last = seed.Value;
        Exception? error = null;
        using var wait = new AutoResetEvent(false);

        using var sub = obs.Subscribe(
            onNext: x =>
            {
                last = x;
            },
            onError: e =>
            {
                error = e; 
                // ReSharper disable once AccessToDisposedClosure
                wait.Set();
            },
            onCompleted: () =>
            {
                // ReSharper disable once AccessToDisposedClosure
                wait.Set();
            });
        
        wait.WaitOne();
        return error == null ? TResult.Continue(last) : TResult.Fail<S>(error);
    }

    public override Obj<B> Map<B>(Transducer<A, B> f) =>
        new ObservableObj<X, B>(Values, Transducer.compose(Next, f));
    
    public override Obj<B> Bind<B>(Func<A, Obj<B>> f) =>
        new ObservableObj<X, B>(Values,  
            Transducer.compose(
                Transducer.compose(Next, Transducer.map(f)),
                Transducer<B>.join));
    
    class ObservableTransducer<S> : IObservable<S>
    {
        readonly IObservable<X> input;
        readonly Transducer<X,A> transducer;
        readonly Func<TState<S>, A, TResult<S>> reducer;
        readonly TState<S> seed;

        public ObservableTransducer(
            IObservable<X> input, 
            Transducer<X, A> transducer, 
            Func<TState<S>, A, TResult<S>> reducer, 
            TState<S> seed)
        {
            this.input = input;
            this.transducer = transducer;
            this.reducer = reducer;
            this.seed = seed;
        }

        public IDisposable Subscribe(IObserver<S> observer)
        {
            var red = transducer.Transform(reducer);
            var res = seed;

            return input.Subscribe(value => {
                try
                {
                    var res1 = red(res, value);
                    if (res1.Faulted)
                    {
                        observer.OnError(res1.ErrorUnsafe);
                    }
                    else if (res1.Complete)
                    {
                        observer.OnCompleted();
                    }
                    else
                    {
                        res = res.SetValue(res1);
                        observer.OnNext(res);
                    }
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }
            });
        }
    }
}

