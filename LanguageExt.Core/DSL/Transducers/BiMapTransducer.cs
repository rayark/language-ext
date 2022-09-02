#nullable enable

namespace LanguageExt.DSL.Transducers;

internal sealed record BiMapTransducer<X, Y, A, B>(Transducer<X, Y> Left, Transducer<A, B> Right) : BiTransducer<X, Y, A, B>
{
    public override Transducer<X, Y> LeftTransducer => 
        Left;
    
    public override Transducer<A, B> RightTransducer => 
        Right;
}
