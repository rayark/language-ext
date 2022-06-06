using System;

namespace LanguageExt.SourceGen.Parser;

/// <summary>
/// Parser
/// </summary>
internal record Parser<A>(Func<State, Result<A>> F)
{
    public static Parser<A> operator |(Parser<A> mx, Parser<A> my) =>
        new (s =>
        {
            var r = mx.F(s);
            if(r.IsSuccess) return r;
            return my.F(s);
        });
}

