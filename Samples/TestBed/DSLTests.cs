////////////////////////////////////////////////////////////////////////////////////////////////////////
//                                                                                                    //
//                                                                                                    //
//     NOTE: This is just my scratch pad for quickly testing stuff, not for human consumption         //
//                                                                                                    //
//                                                                                                    //
////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reactive.Linq;
using System.Threading;
using LanguageExt.Common;
using LanguageExt.DSL.Transducers;
using static LanguageExt.DSL.Transducers.Transducer;
using static LanguageExt.DSL.Prelude;
using Unit = LanguageExt.Unit;

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
        var items = new [] { 10, 20, 30 };

        var effect = from r  in use(DisposeMe.New)
                     from s  in each(seconds)
                     from _1 in logEither("SECOND")
                     from x  in each(items)
                     from _3 in log($"{s * x}")
                     select s * x;

        var effect1 = from _1 in log("START for Either<Error, long>")
                      from r  in use(DisposeMe.New)
                      from e in scope(effect)
                      from _2 in logEither("DONE")
                      select e;
                      
        
        var result = effect1.Match(Left: e =>
        {
            Console.WriteLine($"ERROR RESULT: {e}");
            return default;
        }, Right: x => x);
        //var result = effect1.Apply(default);

        Console.WriteLine(result);
        Console.WriteLine();
        Console.WriteLine("========");
    }
 
    public static void Test2()
    {
        var seconds = Observable.Interval(TimeSpan.FromSeconds(1)).Take(5);
        var items = new [] { 10, 20, 30 };

        var effect = from s  in each(seconds)
                     from _1 in log("SECOND")
                     from x  in each(items)
                     from r  in use(DisposeMe.New)
                     from _2 in logEff($"{s * x}")
                     from _3 in release(r)
                     select s * x;

        var effect1 = from _1 in logEff("START for Eff<RT, long>")
                      from e in scope(effect)
                      from _2 in log("DONE")
                      select e;
        
        //var result = effect1.RunMany();
        var result = effect1.Run();
        //var result = effect1.Apply(default);

        Console.WriteLine(result); 
    }

    static Transducer<Unit, Unit> log(string x) =>
        map<Unit, Unit>(_ =>
        {
            Console.WriteLine(x);
            return default;
        });
    
    static Eff<Unit> logEff(string x) =>
        Eff<Unit>(_ =>
        {
            Console.WriteLine(x);
            return default;
        });
    
    static Either<Error, Unit> logEither(string x) =>
        RightLazy<Error, Unit>(() =>
        {
            Console.WriteLine(x);
            return default;
        });
    
    static Eff<Unit> failEff =>
        EffMaybe<Unit>(_ =>
        {
            Console.WriteLine("FAIL");
            return Error.New("Error time");
        });
    
    static Either<Error, Unit> failEither =>
        LeftLazy<Error, Unit>(() =>
        {
            Console.WriteLine("FAIL");
            return "Error time";
        });
    
    class DisposeMe : IDisposable
    {
        static volatile int idCount;
        readonly int Id;

        DisposeMe()
        {
            Id = Interlocked.Increment(ref idCount);
            Console.WriteLine($"USE {Id}");
        }

        public static readonly Transducer<Unit, DisposeMe> New =
            map<Unit, DisposeMe>(_ => new());
        
        public void Dispose() =>
            Console.WriteLine($"RELEASE {Id}");
    }
}
