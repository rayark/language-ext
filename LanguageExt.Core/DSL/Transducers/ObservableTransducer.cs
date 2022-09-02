#nullable enable
using System;
using System.Threading;

namespace LanguageExt.DSL.Transducers;

internal sealed record ObservableTransducer<A> : Transducer<IObservable<A>, A>
{
    public Func<TState<S>, IObservable<A>, TResult<S>> Transform<S>(Func<TState<S>, A, TResult<S>> reduce) =>
        (seed, values) =>
        {
            var obs = new Reducer<S>(values, reduce, seed);
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
        };
    
    class Reducer<S> : IObservable<S>
    {
        readonly IObservable<A> input;
        readonly Func<TState<S>, A, TResult<S>> reducer;
        readonly TState<S> seed;
    
        public Reducer(
            IObservable<A> input, 
            Func<TState<S>, A, TResult<S>> reducer, 
            TState<S> seed)
        {
            this.input = input;
            this.reducer = reducer;
            this.seed = seed;
        }
    
        public IDisposable Subscribe(IObserver<S> observer)
        {
            var res = seed;
    
            return input.Subscribe(
                onNext: value => {
                    try
                    {
                        var res1 = reducer(res, value);
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
                }, 
                onCompleted: observer.OnCompleted, 
                onError: observer.OnError);
        }
    }
}
