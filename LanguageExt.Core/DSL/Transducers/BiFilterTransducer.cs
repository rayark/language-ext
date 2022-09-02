#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record BiFilterTransducer<X, A>(Func<X, bool> LeftPredicate, Func<A, bool> RightPredicate) : 
    BiTransducer<X, X, A, A>
{
    public override Transducer<X, X> LeftTransducer => 
        new FilterTransducer<X>(LeftPredicate);
    
    public override Transducer<A, A> RightTransducer => 
        new FilterTransducer<A>(RightPredicate);
}
