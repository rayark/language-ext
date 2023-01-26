#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LanguageExt.DSL2;

record FoldTransducer<E, A, B>(
        Transducer<A, Transducer<B, B>> Fold, 
        B State, 
        Transducer<E, A> TA,
        Func<A, B, bool> Predicate,           // TODO: MAKE INTO TRANSDUCER
        Schedule Schedule)
    : Transducer<E, B>
{
    public Func<TState, S, E, TResult<S>> Transform<S>(Func<TState, S, B, TResult<S>> reduce)
    {
        var state = State;
        return (st, s, e) =>
        {
            using var wait = new AutoResetEvent(false);
            foreach (var duration in Duration.Zero.Cons(Schedule.Run()))
            {
                if (duration != Duration.Zero) wait.WaitOne((int)duration);
 
                var r = TA.Transform<B>((st1, b1, a) =>
                            Predicate(a, b1)
                                ? Fold.Transform<B>((st2, b2, tbb) =>
                                    tbb.Transform<B>(static (_, _, b3) =>
                                        TResult.Continue(b3))(st2, b2, b2))(st1, b1, a)
                                : TResult.Complete(b1))(st, state, e);

                switch (r)
                {
                    case TContinue<B> tb:
                        state = tb.Value;
                        break;
                    
                    case TComplete<B> tb:
                        state = tb.Value;
                        return reduce(st, s, tb.Value);
                    
                    case TCancelled<B>:
                        return TResult.Cancel<S>();
                    
                    case TNone<B>:
                        return TResult.None<S>();
                    
                    case TFail<B> tb:
                        return TResult.Fail<S>(tb.Error);
                }
            }

            return reduce(st, s, state);
        };
    }

    public TransducerAsync<E, B> ToAsync() =>
        new FoldTransducerAsyncSync<E, A, B>(Fold.ToAsync(), State, TA.ToAsync(), Predicate, Schedule);
}

record FoldTransducerAsyncSync<E, A, B>(
        TransducerAsync<A, Transducer<B, B>> Fold, 
        B State, 
        TransducerAsync<E, A> TA, 
        Func<A, B, bool> Predicate,           // TODO: MAKE INTO TRANSDUCER
        Schedule Schedule)
    : TransducerAsync<E, B>
{
    public Func<TState, S, E, ValueTask<TResult<S>>> TransformAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduce)
    {
        var state = State;
        return async (st, s, e) =>
        {
            using var wait = new AutoResetEvent(false);
            foreach (var duration in Duration.Zero.Cons(Schedule.Run()))
            {
                if (duration != Duration.Zero) await wait.WaitOneAsync((int)duration);

                var r = await TA.TransformAsync<B>((st1, b1, a) =>
                    Predicate(a, b1)
                        ? Fold.TransformAsync<B>((st2, b2, tbb) =>
                            tbb.ToAsync().TransformAsync<B>(static (_, _, b3) =>
                                new ValueTask<TResult<B>>(TResult.Continue(b3)))(st2, b2, b2))(st1, b1, a)
                        : new ValueTask<TResult<B>>(TResult.Complete(b1))
                        )(st, state, e).ConfigureAwait(false);

                switch (r)
                {
                    case TContinue<B> tb:
                        state = tb.Value;
                        break;

                    case TComplete<B> tb:
                        state = tb.Value;
                        return await reduce(st, s, tb.Value).ConfigureAwait(false);

                    case TCancelled<B>:
                        return TResult.Cancel<S>();

                    case TNone<B>:
                        return TResult.None<S>();

                    case TFail<B> tb:
                        return TResult.Fail<S>(tb.Error);
                }
            }

            return await reduce(st, s, state).ConfigureAwait(false);
        };
    }
}

record FoldTransducerAsync<E, A, B>(
        TransducerAsync<A, TransducerAsync<B, B>> Fold,
        B State,
        TransducerAsync<E, A> TA,
        Func<A, B, bool> Predicate, // TODO: MAKE INTO TRANSDUCER
        Schedule Schedule)
    : TransducerAsync<E, B>
{
    public Func<TState, S, E, ValueTask<TResult<S>>> TransformAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduce)
    {
        var state = State;
        return async (st, s, e) =>
        {
            using var wait = new AutoResetEvent(false);
            foreach (var duration in Duration.Zero.Cons(Schedule.Run()))
            {
                if (duration != Duration.Zero) await wait.WaitOneAsync((int)duration);

                var r = await TA.TransformAsync<B>((st1, b1, a) =>
                    Predicate(a, b1)
                        ? Fold.TransformAsync<B>((st2, b2, tbb) =>
                                tbb.TransformAsync<B>(static (_, _, b3) =>
                                    new ValueTask<TResult<B>>(TResult.Continue(b3)))(st2, b2, b2))
                            (st1, b1, a)
                        : new ValueTask<TResult<B>>(TResult.Complete(b1)))(st, state, e).ConfigureAwait(false);

                switch (r)
                {
                    case TContinue<B> tb:
                        state = tb.Value;
                        break;

                    case TComplete<B> tb:
                        state = tb.Value;
                        return await reduce(st, s, tb.Value).ConfigureAwait(false);

                    case TCancelled<B>:
                        return TResult.Cancel<S>();

                    case TNone<B>:
                        return TResult.None<S>();

                    case TFail<B> tb:
                        return TResult.Fail<S>(tb.Error);
                }
            }

            return await reduce(st, s, state).ConfigureAwait(false);
        };
    }
}

record FoldTransducer2<E, A, B>(
        Func<A, B, B> Fold, 
        B State, Transducer<E, A> TA,
        Func<A, B, bool> Predicate, // TODO: MAKE INTO TRANSDUCER
        Schedule Schedule)
    : Transducer<E, B>
{
    public Func<TState, S, E, TResult<S>> Transform<S>(Func<TState, S, B, TResult<S>> reduce)
    {
        var state = State;
        return (st, s, e) =>
        {
            using var wait = new AutoResetEvent(false);
            foreach (var duration in Duration.Zero.Cons(Schedule.Run()))
            {
                if (duration != Duration.Zero) wait.WaitOne((int)duration);

                var r = TA.Transform<B>((_, b1, a) =>
                    Predicate(a, b1)
                        ? TResult.Continue(Fold(a, b1))
                        : TResult.Complete(b1))(st, state, e);

                switch (r)
                {
                    case TContinue<B> tb:
                        state = tb.Value;
                        break;

                    case TComplete<B> tb:
                        state = tb.Value;
                        return reduce(st, s, tb.Value);

                    case TCancelled<B>:
                        return TResult.Cancel<S>();

                    case TNone<B>:
                        return TResult.None<S>();

                    case TFail<B> tb:
                        return TResult.Fail<S>(tb.Error);
                }
            }

            return reduce(st, s, state);
        };
    }

    public TransducerAsync<E, B> ToAsync() => 
        new FoldTransducerAsync2<E, A, B>(Fold, State, TA.ToAsync(), Predicate, Schedule);
}

record FoldTransducerAsync2<E, A, B>(
        Func<A, B, B> Fold, 
        B State, 
        TransducerAsync<E, A> TA,
        Func<A, B, bool> Predicate, // TODO: MAKE INTO TRANSDUCER
        Schedule Schedule)
    : TransducerAsync<E, B>
{
    public Func<TState, S, E, ValueTask<TResult<S>>> TransformAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduce)
    {
        var state = State;
        return async (st, s, e) =>
        {
            using var wait = new AutoResetEvent(false);
            foreach (var duration in Duration.Zero.Cons(Schedule.Run()))
            {
                if (duration != Duration.Zero) await wait.WaitOneAsync((int)duration);

                var r = await TA.TransformAsync<B>((_, b1, a) => 
                        Predicate(a, b1)
                            ? new ValueTask<TResult<B>>(TResult.Continue(Fold(a, b1)))
                            : new ValueTask<TResult<B>>(TResult.Complete(b1)))(st, state, e);
                
                switch (r)
                {
                    case TContinue<B> tb:
                        state = tb.Value;
                        break;

                    case TComplete<B> tb:
                        state = tb.Value;
                        return await reduce(st, s, tb.Value).ConfigureAwait(false);

                    case TCancelled<B>:
                        return TResult.Cancel<S>();

                    case TNone<B>:
                        return TResult.None<S>();

                    case TFail<B> tb:
                        return TResult.Fail<S>(tb.Error);
                }
            }

            return await reduce(st, s, state).ConfigureAwait(false);
        };
    }    
}

record SumFoldTransducer<E, X, Y, A, B>(
        Transducer<A, Transducer<B, B>> Fold, 
        B State, 
        SumTransducer<X, Y, E, A> TA,
        Func<A, B, bool> Predicate,             // TODO: MAKE INTO TRANSDUCER
        Schedule Schedule)
    : SumTransducer<X, Y, E, B>
{
    public Func<TState, S, Sum<X, E>, TResult<S>> BiTransform<S>(
        Func<TState, S, Y, TResult<S>> Left, 
        Func<TState, S, B, TResult<S>> Right)
    {
        var state = State;
        return (st, s, e) =>
        {
            using var wait = new AutoResetEvent(false);
            foreach (var duration in Duration.Zero.Cons(Schedule.Run()))
            {
                if (duration != Duration.Zero) wait.WaitOne((int)duration);

                var r = TA.BiTransform<Sum<Y, B>>(
                    Left: (_, _, y) => TResult.Complete(Sum<Y, B>.Left(y)),
                    Right: (st1, yb, a) =>
                        yb switch
                        {
                            SumRight<Y, B> b1 =>
                                Predicate(a, b1.Value)
                                    ? Fold.Transform<B>((st2, b2, tbb) =>
                                        tbb.Transform<B>(static (_, _, b3) => TResult.Continue(b3))
                                            (st2, b2, b2))(st1, b1.Value, a).Map(Sum<Y, B>.Right)
                                    : TResult.Complete(yb),
                            _ => TResult.Complete(yb)
                        })(st, Sum<Y, B>.Right(state), e);

                switch (r)
                {
                    case TContinue<Sum<Y, B>> {Value: SumRight<Y, B> br}:
                        state = br.Value;
                        break;

                    case TContinue<Sum<Y, B>> {Value: SumLeft<Y, B> bl}:
                        return Left(st, s, bl.Value);

                    case TComplete<Sum<Y, B>> {Value: SumRight<Y, B> br}:
                        return Right(st, s, br.Value);

                    case TComplete<Sum<Y, B>> {Value: SumLeft<Y, B> bl}:
                        return Left(st, s, bl.Value);

                    case TCancelled<Sum<Y, B>>:
                        return TResult.Cancel<S>();

                    case TNone<Sum<Y, B>>:
                        return TResult.None<S>();

                    case TFail<Sum<Y, B>> tb:
                        return TResult.Fail<S>(tb.Error);
                }
            }

            return Right(st, s, state);

        };
    }

    public Func<TState, S, E, TResult<S>> TransformRight<S>(Func<TState, S, B, TResult<S>> reduceRight) =>
        SumTransducerDefault<X, Y, E, B>.TransformRight(this, reduceRight);

    public Func<TState, S, X, TResult<S>> TransformLeft<S>(Func<TState, S, Y, TResult<S>> reduceLeft) => 
        SumTransducerDefault<X, Y, E, B>.TransformLeft(this, reduceLeft);
    
    public SumTransducerAsync<X, Y, E, B> ToSumAsync() => 
        new SumFoldTransducerAsyncSync<E, X, Y, A, B>(Fold.ToAsync(), State, TA.ToSumAsync(), Predicate, Schedule);
}

record SumFoldTransducerAsync<E, X, Y, A, B>(
        TransducerAsync<A, TransducerAsync<B, B>> Fold, 
        B State, 
        SumTransducerAsync<X, Y, E, A> TA,
        Func<A, B, bool> Predicate,             // TODO: MAKE INTO TRANSDUCER
        Schedule Schedule)
    : SumTransducerAsync<X, Y, E, B>
{
    public Func<TState, S, Sum<X, E>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> Left, 
        Func<TState, S, B, ValueTask<TResult<S>>> Right)
    {
        var state = State;
        return async (st, s, e) =>
        {
            using var wait = new AutoResetEvent(false);
            foreach (var duration in Duration.Zero.Cons(Schedule.Run()))
            {
                if (duration != Duration.Zero) await wait.WaitOneAsync((int)duration);

                var r = await TA.BiTransformAsync<Sum<Y, B>>(
                    Left: (_, _, y) => new ValueTask<TResult<Sum<Y, B>>>(TResult.Complete(Sum<Y, B>.Left(y))),
                    Right: async (st1, yb, a) =>
                        yb switch
                        {
                            SumRight<Y, B> b1 =>
                                Predicate(a, b1.Value)
                                    ? (await Fold.TransformAsync<B>((st2, b2, tbb) =>
                                        tbb.TransformAsync<B>(
                                                static (_, _, b3) => new ValueTask<TResult<B>>(TResult.Continue(b3)))
                                            (st2, b2, b2))(st1, b1.Value, a).ConfigureAwait(false))
                                            .Map(Sum<Y, B>.Right)
                                    : TResult.Complete(yb),
                            _ => TResult.Complete(yb)
                        })(st, Sum<Y, B>.Right(state), e).ConfigureAwait(false);

                switch (r)
                {
                    case TContinue<Sum<Y, B>> {Value: SumRight<Y, B> br}:
                        state = br.Value;
                        break;

                    case TContinue<Sum<Y, B>> {Value: SumLeft<Y, B> bl}:
                        return await Left(st, s, bl.Value).ConfigureAwait(false);

                    case TComplete<Sum<Y, B>> {Value: SumRight<Y, B> br}:
                        return await Right(st, s, br.Value).ConfigureAwait(false);

                    case TComplete<Sum<Y, B>> {Value: SumLeft<Y, B> bl}:
                        return await Left(st, s, bl.Value).ConfigureAwait(false);

                    case TCancelled<Sum<Y, B>>:
                        return TResult.Cancel<S>();

                    case TNone<Sum<Y, B>>:
                        return TResult.None<S>();

                    case TFail<Sum<Y, B>> tb:
                        return TResult.Fail<S>(tb.Error);
                }
            }

            return await Right(st, s, state).ConfigureAwait(false);

        };
    }

    public Func<TState, S, E, ValueTask<TResult<S>>> TransformRightAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) =>
        SumTransducerAsyncDefault<X, Y, E, B>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(Func<TState, S, Y, ValueTask<TResult<S>>> reduceLeft) => 
        SumTransducerAsyncDefault<X, Y, E, B>.TransformLeftAsync(this, reduceLeft);
}

record SumFoldTransducerAsyncSync<E, X, Y, A, B>(
        TransducerAsync<A, Transducer<B, B>> Fold, 
        B State, 
        SumTransducerAsync<X, Y, E, A> TA,
        Func<A, B, bool> Predicate,             // TODO: MAKE INTO TRANSDUCER
        Schedule Schedule)
    : SumTransducerAsync<X, Y, E, B>
{
    public Func<TState, S, Sum<X, E>, ValueTask<TResult<S>>> BiTransformAsync<S>(
        Func<TState, S, Y, ValueTask<TResult<S>>> Left, 
        Func<TState, S, B, ValueTask<TResult<S>>> Right)
    {
        var state = State;
        return async (st, s, e) =>
        {
            using var wait = new AutoResetEvent(false);
            foreach (var duration in Duration.Zero.Cons(Schedule.Run()))
            {
                if (duration != Duration.Zero) await wait.WaitOneAsync((int)duration);

                var r = await TA.BiTransformAsync<Sum<Y, B>>(
                        Left: (_, _, y) => new ValueTask<TResult<Sum<Y, B>>>(TResult.Complete(Sum<Y, B>.Left(y))),
                        Right: async (st1, yb, a) =>
                            yb switch
                            {
                                SumRight<Y, B> b1 =>
                                    Predicate(a, b1.Value)
                                        ? (await Fold.TransformAsync<B>(
                                            (st2, b2, tbb) =>
                                                tbb.ToAsync().TransformAsync<B>(
                                                        static (_, _, b3) => new ValueTask<TResult<B>>(TResult.Continue(b3)))
                                                    (st2, b2, b2))(st1, b1.Value, a).ConfigureAwait(false))
                                                    .Map(Sum<Y, B>.Right)
                                        : TResult.Complete(yb)
                                    ,

                                _ => TResult.Complete(yb)
                            })(st, Sum<Y, B>.Right(state), e)
                    .ConfigureAwait(false);
                
                switch (r)
                {
                    case TContinue<Sum<Y, B>> {Value: SumRight<Y, B> br}:
                        state = br.Value;
                        break;

                    case TContinue<Sum<Y, B>> {Value: SumLeft<Y, B> bl}:
                        return await Left(st, s, bl.Value).ConfigureAwait(false);

                    case TComplete<Sum<Y, B>> {Value: SumRight<Y, B> br}:
                        return await Right(st, s, br.Value).ConfigureAwait(false);

                    case TComplete<Sum<Y, B>> {Value: SumLeft<Y, B> bl}:
                        return await Left(st, s, bl.Value).ConfigureAwait(false);

                    case TCancelled<Sum<Y, B>>:
                        return TResult.Cancel<S>();

                    case TNone<Sum<Y, B>>:
                        return TResult.None<S>();

                    case TFail<Sum<Y, B>> tb:
                        return TResult.Fail<S>(tb.Error);
                }
            }

            return await Right(st, s, state).ConfigureAwait(false);
        };
    }    


    public Func<TState, S, E, ValueTask<TResult<S>>> TransformRightAsync<S>(Func<TState, S, B, ValueTask<TResult<S>>> reduceRight) =>
        SumTransducerAsyncDefault<X, Y, E, B>.TransformRightAsync(this, reduceRight);

    public Func<TState, S, X, ValueTask<TResult<S>>> TransformLeftAsync<S>(Func<TState, S, Y, ValueTask<TResult<S>>> reduceLeft) => 
        SumTransducerAsyncDefault<X, Y, E, B>.TransformLeftAsync(this, reduceLeft);
}
