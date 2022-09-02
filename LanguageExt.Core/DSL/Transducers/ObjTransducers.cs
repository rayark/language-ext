#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record JoinObjTransducer<A> : Transducer<Obj<A>, A>
{
    public Func<TState<S>, Obj<A>, TResult<S>> Transform<S>(Func<TState<S>, A, TResult<S>> reduce) => 
        (s, v) => v.Transduce(s, reduce);
}

internal sealed record BindObjTransducer<A, B>(Func<A, Obj<B>> Bind) : Transducer<A, B>
{
    public Func<TState<S>, A, TResult<S>> Transform<S>(Func<TState<S>, B, TResult<S>> reduce) =>
        (s, v) => Bind(v).Transduce(s, reduce);
}

internal sealed record BiJoinObjTransducer<X, A> : BiTransducer<Obj<X>, X, Obj<A>, A>
{
    public static BiTransducer<Obj<X>, X, Obj<A>, A> Default = new BiJoinObjTransducer<X, A>();
    
    public override Transducer<Obj<X>, X> LeftTransducer => 
        Transducer<X>.join;
    
    public override Transducer<Obj<A>, A> RightTransducer => 
        Transducer<A>.join;
}

internal sealed record BiBindObjTransducer<X, Y, A, B>(Func<X, Obj<Y>> Left, Func<A, Obj<B>> Right): BiTransducer<X, Y, A, B>
{
    public override Transducer<X, Y> LeftTransducer => 
        new BindObjTransducer<X, Y>(Left);
    
    public override Transducer<A, B> RightTransducer => 
        new BindObjTransducer<A, B>(Right);
}
