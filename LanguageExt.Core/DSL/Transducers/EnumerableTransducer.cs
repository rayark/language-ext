#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;

namespace LanguageExt.DSL.Transducers;

internal sealed record EnumerableTransducer<A> : Transducer<IEnumerable<A>, A>
{
    public Func<TState<S>, IEnumerable<A>, TResult<S>> Transform<S>(Func<TState<S>, A, TResult<S>> reduce) =>
        (seed, values) =>
        {
            foreach (var value in values)
            {
                var res = reduce(seed, value);
                if (res.Complete) return TResult.Continue(seed.Value);
                seed = seed.SetValue(res);
            }
            return TResult.Continue(seed.Value);
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
    
            return input.Subscribe(value => {
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
            });
        }
    }
}
