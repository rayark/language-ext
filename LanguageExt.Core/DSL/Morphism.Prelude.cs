using System;
using System.Collections.Generic;
using LanguageExt.DSL.Transducers;

namespace LanguageExt.DSL;

public static partial class Prelude
{
    public static Transducer<Unit, A> map<A>(IObservable<A> ma) => 
         Transducer<A>.observable.Inject(ma);

    public static Transducer<Unit, A> map<A>(IEnumerable<A> ma) =>
        Transducer<A>.enumerable.Inject(ma);
}

