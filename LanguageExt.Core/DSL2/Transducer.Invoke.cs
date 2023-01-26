#nullable enable
using System;
using System.Threading.Tasks;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace LanguageExt.DSL2;

// ---------------------------------------------------------------------------------------------------------------------

public static partial class TransducerExtensions
{
    public static Fin<B> Invoke1<A, B>(this Transducer<A, B> t, A value)
    {
        var st = new TState();
        try
        {
            var s = FinFail<B>(Errors.Bottom);
            return t.Transform<Fin<B>>(static (_, _, b) => TResult.Continue(FinSucc(b)))(st, s, value)
                switch
                {
                    TContinue<Fin<B>> x => x.ValueUnsafe,
                    TComplete<Fin<B>> x => x.ValueUnsafe,
                    TCancelled<Fin<B>> => Errors.Cancelled,
                    TNone<Fin<B>> => Errors.None,
                    TFail<Fin<B>> x => x.ErrorUnsafe,
                    _ => throw new InvalidOperationException()
                };
        }
        catch (Exception e)
        {
            return (Error)e;
        }
        finally
        {
            st.Dispose();
        }
    }
    
    public static Fin<Seq<B>> InvokeMany<A, B>(this Transducer<A, B> t, A value)
    {
        var st = new TState();
        try
        {
            var s = Seq<B>.Empty;
            return t.Transform<Seq<B>>(static (_, s1, b) => TResult.Continue(s1.Add(b)))(st, s, value)
                switch
                {
                    TContinue<Seq<B>> x => x.ValueUnsafe,
                    TComplete<Seq<B>> x => x.ValueUnsafe,
                    TCancelled<Seq<B>> => Errors.Cancelled,
                    TNone<Seq<B>> => Errors.None,
                    TFail<Seq<B>> x => x.ErrorUnsafe,
                    _ => throw new InvalidOperationException()
                };
        }
        catch (Exception e)
        {
            return (Error)e;
        }
        finally
        {
            st.Dispose();
        }
    }    
    
    public static Fin<(Y, B)> Invoke1<X, Y, A, B>(this ProductTransducer<X, Y, A, B> transducer, (X, A) pair) =>
        Transducer.merge(transducer).Invoke1(pair);
    
    public static Fin<Seq<(Y, B)>> InvokeMany<X, Y, A, B>(this ProductTransducer<X, Y, A, B> transducer, (X, A) pair) =>
        Transducer.merge(transducer).InvokeMany(pair);
    
    public static Fin<(Y, B)> Invoke1<X, Y, A, B>(
        this ProductTransducer<X, Y, A, B> transducer, 
        X first, A second) =>
        Transducer.merge(transducer).Invoke1((first, second));
    
    public static Fin<Seq<(Y, B)>> InvokeMany<X, Y, A, B>(
        this ProductTransducer<X, Y, A, B> transducer, 
        X first, A second) =>
        Transducer.merge(transducer).InvokeMany((first, second));
    
    public static Fin<Sum<Y, B>> Invoke1<X, Y, A, B>(
        this SumTransducer<X, Y, A, B> transducer,
        Sum<X, A> value) =>
        Transducer.merge(transducer).Invoke1(value);
    
    public static Fin<Seq<Sum<Y, B>>> InvokeMany<X, Y, A, B>(
        this SumTransducer<X, Y, A, B> transducer,
        Sum<X, A> value) =>
        Transducer.merge(transducer).InvokeMany(value);

    public static Fin<B> Invoke1<A, B>(this SumTransducer<Error, Error, A, B> transducer, A value) =>
        Transducer
           .merge(transducer)
           .Invoke1(Sum<Error, A>.Right(value))
           .Match(Succ: x => x switch
            {
                SumRight<Error, B> r => FinSucc(r.Value),
                SumLeft<Error, B> l => FinFail<B>(l.Value),
                _ => throw new NotSupportedException()
            }, Fail: FinFail<B>);

    public static Fin<Seq<B>> InvokeMany<A, B>(this SumTransducer<Error, Error, A, B> transducer, A value) =>
        Transducer
           .merge(transducer)
           .InvokeMany(Sum<Error, A>.Right(value))
           .Map(xs =>
                xs.Map(x => x switch
                    {
                        SumRight<Error, B> r => FinSucc(r.Value),
                        SumLeft<Error, B> l => FinFail<B>(l.Value),
                        _ => throw new NotSupportedException()
                    }))
           .Map(xs => xs.Sequence())
           .Flatten();

}
