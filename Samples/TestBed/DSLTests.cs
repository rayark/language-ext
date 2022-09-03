////////////////////////////////////////////////////////////////////////////////////////////////////////
//                                                                                                    //
//                                                                                                    //
//     NOTE: This is just my scratch pad for quickly testing stuff, not for human consumption         //
//                                                                                                    //
//                                                                                                    //
////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reactive;
using System.Reactive.Linq;
using LanguageExt.Common;
using LanguageExt.DSL.Transducers;
using LanguageExt.Sys.Live;
using static LanguageExt.DSL.Prelude;

namespace TestBed;

using LanguageExt.DSL;

public static class DSLTests
{
    public static void Main()
    {
        Test2();
        //Test1();
    }
 
    /*public static void Test1()
    {
        var seconds = Observable.Interval(TimeSpan.FromSeconds(1)).Take(5);

        var morphism = from s in map<Error, long>(seconds)
                       from _1 in log("X")
                       from x in Right<Error, int>(10)
                       from _2 in log("Y")
                       select s * x;
        
        var effect = morphism.ToEither();
        var result = effect.MatchMany(Left: e => 0, Right: r => r);

        Console.WriteLine(result);  // [0, 10, 20, 30, 40]
        
        var xxx = from x in Right<Error, string?>(null)
                  from y in Right<Error, string?>(null)
                  select x + y;
        
        //var result = effect.MatchMany(Left: _ => 0, Right: r => r);
        //var result = effect.Run(Runtime.New());
    }*/
 
    public static void Test2()
    {
        var seconds = Observable.Interval(TimeSpan.FromSeconds(1)).Take(5);

        var effect = from s  in map(seconds)
                     from _1 in logEff<Runtime>("X")
                     from x  in SuccessEff<Runtime, int>(10)
                     from _2 in logEff<Runtime>("Y")
                     from _3 in logEff<Runtime>($"{s * x}")
                     select s * x;

        var effect1 = from e in effect.Head
                      from _2 in logEff<Runtime>("Done")
                      select e;
                      
        
        var result = effect1.RunMany(Runtime.New());
        //var result = effect1.Run(Runtime.New());

        Console.WriteLine(result);  // [0, 10, 20, 30, 40]
    }

    /*
    static Either<Error, Unit> log(string x)
    {
        var m = Morphism.function<Unit, CoProduct<Error, Unit>>(_ =>
        {
            Console.WriteLine(x);
            return CoProduct.Right<Error, Unit>(default);
        });

        return m.Apply(Obj.Pure<Unit>(default));
    }
    */


    static Eff<RT, Unit> logEff<RT>(string x) =>
        Eff<RT, Unit>(_ =>
        {
            Console.WriteLine(x);
            return default;
        });
}
