#nullable enable
using System;
using System.Collections.Generic;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace LanguageExt.DSL.Transducers;

public static class TransducerStatic2<X, Y>
{
    /// <summary>
    /// Extract the right value from the co-product (if possible)
    /// </summary>
    public static readonly Transducer<CoProduct<X, Y>, Y> rightValue =
        new MapRightBackTransducer2<X, Y, Y>(identity);
    
    /// <summary>
    /// Extract the left value from the co-product (if possible)
    /// </summary>
    public static readonly Transducer<CoProduct<X, Y>, X> leftValue =
        new MapLeftBackTransducer2<X, X, Y>(identity);
    
    /// <summary>
    /// Make a co-product from the right value
    /// </summary>
    public static readonly Transducer<Y, CoProduct<X, Y>> right =
        Transducer.map<Y, CoProduct<X, Y>>(CoProduct.Right<X, Y>);
    
    /// <summary>
    /// Make a co-product from the left value
    /// </summary>
    public static readonly Transducer<X, CoProduct<X, Y>> left =
        Transducer.map<X, CoProduct<X, Y>>(CoProduct.Left<X, Y>);
}
