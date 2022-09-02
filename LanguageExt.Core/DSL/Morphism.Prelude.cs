using System;
using System.Collections.Generic;
using LanguageExt.DSL.Transducers;

namespace LanguageExt.DSL;

public static partial class Prelude
{
    //public static Transducer<Unit, CoProduct<X, A>> map<X, A>(IObservable<A> ma) => 
    //    Transducer.each(ma).Map(CoProduct.Right<X, A>);

    //public static MorphismTransducer<CoProduct<X, Unit>, CoProduct<X, A>> bimap<X, A>(IObservable<A> ma) =>
    //    BiMorphism.bimap(Morphism<X>.identity, Morphism.each(ma));

    public static Transducer<Unit, A> map<A>(IObservable<A> ma) => 
         Transducer<A>.observable.Inject(ma);

    public static Transducer<Unit, A> map<A>(IEnumerable<A> ma) =>
        Transducer<A>.enumerable.Inject(ma);
}

