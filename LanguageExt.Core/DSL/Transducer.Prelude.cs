using System;
using System.Collections.Generic;
using LanguageExt.DSL.Transducers;
using static LanguageExt.DSL.Transducers.Transducer;

namespace LanguageExt.DSL;

public static partial class Prelude
{
    public static Transducer<Unit, A> each<A>(IObservable<A> ma) => 
         Transducer<A>.observable.Inject(ma);

    public static Transducer<Unit, A> each<A>(IEnumerable<A> ma) =>
        Transducer<A>.enumerable.Inject(ma);

    public static Transducer<Unit, A> use<A>(Transducer<Unit, A> disposable) where A : IDisposable =>
        compose(disposable, TransducerD<A>.use);

    public static Transducer<Unit, A> use<A>(Transducer<Unit, A> disposable, Func<A, Unit> release) =>
        compose(disposable, Transducer.use(release));
    
    public static Transducer<Unit, Unit> release<A>(A resource) =>
        compose(constant<Unit, A>(resource), Transducer<A>.release);
}

