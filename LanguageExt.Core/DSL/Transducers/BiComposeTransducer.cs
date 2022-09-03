#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record BiComposeTransducer<X, Y, Z, A, B, C>(
    BiTransducer<X, Y, A, B> First, 
    BiTransducer<Y, Z, B, C> Second
    ) : BiTransducer<X, Z, A, C>
{
    public override Transducer<X, Z> LeftTransducer => 
        Transducer.compose(First.LeftTransducer, Second.LeftTransducer);
    
    public override Transducer<A, C> RightTransducer => 
        Transducer.compose(First.RightTransducer, Second.RightTransducer);
}

internal sealed record BiComposeTransducer2<X, Y, Z, A, B, C>(
    Transducer<X, Y> LeftFirst, Transducer<Y, Z> LeftSecond,
    Transducer<A, B> RightFirst, Transducer<B, C> RightSecond
) : BiTransducer<X, Z, A, C>
{
    public override Transducer<X, Z> LeftTransducer => 
        Transducer.compose(LeftFirst, LeftSecond);
    
    public override Transducer<A, C> RightTransducer => 
        Transducer.compose(RightFirst, RightSecond);
}
