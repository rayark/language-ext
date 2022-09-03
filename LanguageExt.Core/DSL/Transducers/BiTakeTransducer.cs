#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record BiTakeUntilTransducer<X, A>(Func<X, bool> LeftPredicate, Func<A, bool> RightPredicate) 
    : BiTransducer<X, X, A, A>
{
    public override Transducer<X, X> LeftTransducer => 
        Transducer.takeUntil(LeftPredicate);
    
    public override Transducer<A, A> RightTransducer => 
        Transducer.takeUntil(RightPredicate);
}

internal sealed record BiTakeWhileTransducer<X, A>(Func<X, bool> LeftPredicate, Func<A, bool> RightPredicate) 
    : BiTransducer<X, X, A, A>
{
    public override Transducer<X, X> LeftTransducer => 
        Transducer.takeWhile(LeftPredicate);
    
    public override Transducer<A, A> RightTransducer => 
        Transducer.takeWhile(RightPredicate);
}

internal sealed record BiTakeTransducer<X, A>(int LeftCount, int RightCount) : BiTransducer<X, X, A, A>
{
    public override Transducer<X, X> LeftTransducer => 
        Transducer.take<X>(LeftCount);
    
    public override Transducer<A, A> RightTransducer => 
        Transducer.take<A>(RightCount);
}
