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
using LanguageExt.Sys.Test;
using static LanguageExt.DSL.Prelude;

namespace TestBed;

using LanguageExt.DSL;

public static class DSLTests
{
    public static void Main()
    {
        Test1();
    }
 
    public static void Test1()
    {
        var seconds = Observable.Interval(TimeSpan.FromSeconds(1)).Take(5);

        var effect =
            from s in seconds
            from x in Right<Error, int>(10)
            select s * x;
        
        var result = effect.MatchMany(Left: _ => 0, Right: r => r);
        
        //var result = effect.Run(Runtime.New());
        
        Console.WriteLine(result);
    }
    
}
