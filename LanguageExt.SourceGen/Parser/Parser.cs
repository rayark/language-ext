using System;

namespace LanguageExt.SourceGen.Parser;

/// <summary>
/// Parser
/// </summary>
internal record Parser<A>(Func<State, Result<A>> F)
{
    public Result<A> Parse(string source, string path) =>
        F(new State(source, path, 0, 1, 1));
    
    public static Parser<A> operator |(Parser<A> mx, Parser<A> my) =>
        new (s =>
        {
            var r = mx.F(s);
            if(r.IsSuccess) return r;
            return my.F(s);
        });

    public Parser<B> Select<B>(Func<A, B> f) =>
        new(s => F(s).Map(f));

    public Parser<B> Map<B>(Func<A, B> f) =>
        Select(f);
    
    public Parser<B> SelectMany<B>(Func<A, Parser<B>> f) =>
        new(s =>
        {
            var r = F(s);
            if (r.IsEmpty) return r.Cast<B>();
            return f(r.Value).F(r.State);
        });
    
    public Parser<B> Bind<B>(Func<A, Parser<B>> f) =>
        SelectMany(f);

    public Parser<C> SelectMany<B, C>(Func<A, Parser<B>> bind, Func<A, B, C> project) =>
        new(s =>
        {
            var ra = F(s);
            if (ra.IsEmpty) return ra.Cast<C>();
            var rb = bind(ra.Value).F(ra.State);
            if (rb.IsEmpty) return rb.Cast<C>();
            var c = project(ra.Value, rb.Value);
            return Result.Success(rb.State, c);
        });

    
}

