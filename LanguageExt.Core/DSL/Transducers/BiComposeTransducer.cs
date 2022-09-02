#nullable enable
using System;

namespace LanguageExt.DSL.Transducers;

internal sealed record BiComposeTransducer<X, Y, Z, A, B, C>(
    BiTransducer<X, Y, A, B> First, 
    BiTransducer<Y, Z, B, C> Second
    ) : BiTransducer<X, Z, A, C>
{
    public override Transducer<X, Z> LeftTransducer => 
        new ComposeTransducer<X, Y, Z>(First.LeftTransducer, Second.LeftTransducer);
    
    public override Transducer<A, C> RightTransducer => 
        new ComposeTransducer<A, B, C>(First.RightTransducer, Second.RightTransducer);
}

internal sealed record BiComposeTransducer2<X, Y, Z, A, B, C>(
    Transducer<X, Y> LeftFirst, Transducer<Y, Z> LeftSecond,
    Transducer<A, B> RightFirst, Transducer<B, C> RightSecond
) : BiTransducer<X, Z, A, C>
{
    public override Transducer<X, Z> LeftTransducer => 
        new ComposeTransducer<X, Y, Z>(LeftFirst, LeftSecond);
    
    public override Transducer<A, C> RightTransducer => 
        new ComposeTransducer<A, B, C>(RightFirst, RightSecond);
}
