#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record BiSkipUntilTransducer<X, A>(Func<X, bool> LeftPredicate, Func<A, bool> RightPredicate) 
    : BiTransducer<X, X, A, A>
{
    public override Transducer<X, X> LeftTransducer => 
        Transducer.skipUntil(LeftPredicate);
    
    public override Transducer<A, A> RightTransducer => 
        Transducer.skipUntil(RightPredicate);
}

internal sealed record BiSkipWhileTransducer<X, A>(Func<X, bool> LeftPredicate, Func<A, bool> RightPredicate) 
    : BiTransducer<X, X, A, A>
{
    public override Transducer<X, X> LeftTransducer => 
        Transducer.skipWhile(LeftPredicate);
    
    public override Transducer<A, A> RightTransducer => 
        Transducer.skipWhile(RightPredicate);
}

internal sealed record BiSkipTransducer<X, A>(int LeftCount, int RightCount) : BiTransducer<X, X, A, A>
{
    public override Transducer<X, X> LeftTransducer => 
        Transducer.skip<X>(LeftCount);
    
    public override Transducer<A, A> RightTransducer => 
        Transducer.skip<A>(RightCount);
}
