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
using P = LanguageExt.Prelude;

public static class DSLTests
{
    public static void Main()
    {
        Test1();
        Test2();
    }
 
    public static void Test1()
    {
        var seconds = Observable.Interval(TimeSpan.FromSeconds(1)).Take(5);
        var items = P.Seq(10, 20, 30);

        var effect = from s  in map(seconds)
                     from _1 in logEither<Error>("SECOND")
                     from x  in map(items)
                     from _3 in logEither<Error>($"{s * x}")
                     select s * x;

        var effect1 = from _1 in logEither<Error>("START for Either<Error, long>")
                      from e in scope(effect)
                      from _2 in logEither<Error>("DONE")
                      select e;
                      
        
        //var result = effect1.RunMany(Runtime.New());
        var result = effect1.Match(Left: x => default, Right: x => x); 

        Console.WriteLine(result);
    }
 
    public static void Test2()
    {
        var seconds = Observable.Interval(TimeSpan.FromSeconds(1)).Take(5);
        var items = P.Seq(10, 20, 30);

        var effect = from s  in map(seconds)
                     from _1 in logEff<Runtime>("SECOND")
                     from x  in map(items)
                     from _3 in logEff<Runtime>($"{s * x}")
                     select s * x;

        var effect1 = from _1 in logEff<Runtime>("START for Eff<RT, long>")
                      from e in scope(effect)
                      from _2 in logEff<Runtime>("DONE")
                      select e;
                      
        
        //var result = effect1.RunMany(Runtime.New());
        var result = effect1.Run(Runtime.New());

        Console.WriteLine(result);  // [0, 10, 20, 30, 40]
    }

    static Eff<RT, Unit> logEff<RT>(string x) =>
        Eff<RT, Unit>(_ =>
        {
            Console.WriteLine(x);
            return default;
        });

    static Either<L, Unit> logEither<L>(string x) =>
        RightLazy<L, Unit>(() =>
        {
            Console.WriteLine(x);
            return default;
        });
}
