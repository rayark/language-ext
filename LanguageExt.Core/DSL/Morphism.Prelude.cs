using System;
using System.Collections.Generic;

namespace LanguageExt.DSL;

public static partial class Prelude
{
    public static Morphism<Unit, A> map<A>(IObservable<A> ma) => 
        Morphism.each(ma);

    public static Morphism<Unit, CoProduct<X, A>> map<X, A>(IObservable<A> ma) => 
        Morphism.each(ma).Map(CoProduct.Right<X, A>);

    public static Morphism<CoProduct<X, Unit>, CoProduct<X, A>> bimap<X, A>(IObservable<A> ma) =>
        BiMorphism.bimap(Morphism<X>.identity, Morphism.each(ma));

    public static Obj<A> map<A>(IEnumerable<A> ma) =>
        Obj.Many(ma.Map(Prim.Pure));
}
