using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LanguageExt.SourceGen.Parser;

internal static class Prim
{
    private static string[] keywords = {"using", "namespace", "alias", "union", "record"};
    
    static Prim()
    {
        any = new Parser<char>(state =>
                {
                    if (state.IsEOS) return Result.EOS<char>(state, "any character");
                    var ch = state.Value;
                    return Result.Success(state.Next(), ch);
                });
                
        space = satisfy(Char.IsWhiteSpace);

        spaces = new Parser<Unit>(state =>
            {
                while (true)
                {
                    if (state.IsEOS) return Result.Success(state, default(Unit));
                    var ch = state.Value;
                    if(!Char.IsWhiteSpace(ch)) return Result.Success(state, default(Unit));
                    state = state.Next();
                }
            });

        eos = new Parser<Unit>(state =>
            state.IsEOS
                ? Result.Success(state, default(Unit))
                : Result.Expected<Unit>(state, state.Value.ToString(), "end-of-stream"));
        
        path = new Parser<string>(state => Result.Success(state, state.Path));

        ident = new Parser<string>(state =>
            {
                var sb = new StringBuilder();
                var nstate = state;
                while (true)
                {
                    if (nstate.IsEOS) return sb.Length == 0 
                                                ? Result.EOS<string>(state)
                                                : sb.ToString() switch
                                                    {
                                                        var x when keywords.Contains(x) => Result.Expected<string>(state, x, "identifier"),
                                                        var x => Result.Success(nstate, x)
                                                    };

                    var c = nstate.Value;
                    if(!char.IsLetterOrDigit(c)) return sb.Length == 0 
                                                    ? Result.Expected<string>(state, $"{c}", "identifier")
                                                    : sb.ToString() switch
                                                        {
                                                            var x when keywords.Contains(x) => Result.Expected<string>(state, x, "identifier"),
                                                            var x => Result.Success(nstate, x)
                                                        };

                    sb.Append(c);
                    nstate = nstate.Next();
                }
            });

        dot = token(ch('.')).Map(static _ => default(Unit));
        comma = token(ch(',')).Map(static _ => default(Unit));

        openParen = token(ch('(')).Map(static _ => default(Unit));
        openBracket = token(ch('[')).Map(static _ => default(Unit));
        openBrace = token(ch('{')).Map(static _ => default(Unit));
        openAngle = token(ch('<')).Map(static _ => default(Unit));

        closeParen = token(ch(')')).Map(static _ => default(Unit));
        closeBracket = token(ch(']')).Map(static _ => default(Unit));
        closeBrace = token(ch('}')).Map(static _ => default(Unit));
        closeAngle = token(ch('>')).Map(static _ => default(Unit));

        usingKeyword = token(str("using")).Map(static _ => default(Unit));
        namespaceKeyword = token(str("namespace")).Map(static _ => default(Unit));
        aliasKeyword = token(str("alias")).Map(static _ => default(Unit));
        unionKeyword = token(str("union")).Map(static _ => default(Unit));
        recordKeyword = token(str("record")).Map(static _ => default(Unit));

        fqn = sepBy1(dot, ident).Map(FQN.New);
    }

    public static readonly Parser<char> any;
    public static readonly Parser<char> space; 
    public static readonly Parser<Unit> spaces;
    public static readonly Parser<Unit> eos;
    public static readonly Parser<string> path;
    public static readonly Parser<string> ident;
    public static readonly Parser<Unit> usingKeyword;
    public static readonly Parser<Unit> namespaceKeyword;
    public static readonly Parser<Unit> aliasKeyword;
    public static readonly Parser<Unit> unionKeyword;
    public static readonly Parser<Unit> recordKeyword;
    public static readonly Parser<FQN> fqn;
    public static readonly Parser<Unit> dot;
    public static readonly Parser<Unit> comma;
    public static readonly Parser<Unit> openParen;
    public static readonly Parser<Unit> openBracket;
    public static readonly Parser<Unit> openBrace;
    public static readonly Parser<Unit> openAngle;
    public static readonly Parser<Unit> closeParen;
    public static readonly Parser<Unit> closeBracket;
    public static readonly Parser<Unit> closeBrace;
    public static readonly Parser<Unit> closeAngle;

    public static Parser<A> result<A>(A value) =>
        new(s => Result.Success(s, value));

    public static Parser<A> failure<A>(Error error) =>
        new(s => Result.Fail<A>(s, error));

    public static Parser<A> unexpected<A>(string thing) =>
        new(s => Result.Fail<A>(s, Error.Unexpected(thing)));

    public static Parser<A> expected<A>(string thing, string got) =>
        new(s => Result.Fail<A>(s, Error.Expected(got, thing)));
    
    public static Parser<char> satisfy(Func<char, bool> f) => 
        new (state =>
        {
            if (state.IsEOS) return Result.EOS<char>(state);
            var ch = state.Value;
            if(!f(ch)) return Result.Expected<char>(state, $"'{ch}'", "<satisfy predicate>");
            return Result.Success(state.Next(), ch);
        });

    public static Parser<char> ch(char c) => 
        new (state =>
        {
            if (state.IsEOS) return Result.EOS<char>(state);
            var ch = state.Value;
            if(ch != c) return Result.Expected<char>(state, $"'{ch}'", $"'{c}'");
            return Result.Success(state.Next(), ch);
        });

    public static Parser<char> oneOf(params char[] chs) => 
        new (state =>
        {
            if (state.IsEOS) return Result.EOS<char>(state);
            var ch = state.Value;
            if(!Contains(chs, ch)) return Result.Expected<char>(state, $"'{ch}'", string.Join(", ", chs.Select(c => $"'{c}'")));
            return Result.Success(state.Next(), ch);
        });

    public static Parser<string> str(string str) =>
        new Parser<string>(state =>
        {
            var nstate = state;
            foreach (var c in str)
            {
                if (nstate.IsEOS) return Result.EOS<string>(state, $"'{c}'");
                var ch = nstate.Source[nstate.Pos];
                if(c != ch) return Result.Expected<string>(state, $"'{ch}'", $"'{c}'");
                nstate = nstate.Next();
            }
            return Result.Success(nstate, str);
        }).label(str);

    public static Parser<IEnumerable<A>> many<A>(Parser<A> p) =>
        new(state =>
        {
            var rs = new List<A>();
            while (true)
            {
                if (state.IsEOS) return Result.Success<IEnumerable<A>>(state, rs);
                var r = p.F(state);
                if (r.IsFail) return Result.Success<IEnumerable<A>>(state, rs);
                if (r.IsEmpty) throw new InvalidOperationException("Parser within many can return 'empty', this isn't allowed");
                rs.Add(r.Value);
                state = r.State;
            }
        });

    public static Parser<IEnumerable<A>> many1<A>(Parser<A> p) =>
        new(state =>
        {
            var rs = new List<A>();
            var nstate = state;
            while (true)
            {
                if (nstate.IsEOS) return rs.Count == 0 
                                            ? Result.EOS<IEnumerable<A>>(state) 
                                            : Result.Success<IEnumerable<A>>(state, rs);
                var r = p.F(nstate);
                if (r.IsFail) return rs.Count == 0 
                                        ? r.Cast<IEnumerable<A>>() 
                                        : Result.Success<IEnumerable<A>>(nstate, rs);
                if (r.IsEmpty) throw new InvalidOperationException("Parser within many can return 'empty', this isn't allowed");
                rs.Add(r.Value);
                nstate = r.State;
            }
        });

    public static Parser<A> choice<A>(params Parser<A>[] ps) =>
        new(state =>
        {
            if (state.IsEOS) return Result.EOS<A>(state);

            var errs = new List<Error>();
            
            foreach (var p in ps)
            {
                var r = p.F(state);
                if (r is FailResult<A> fail)
                {
                    errs.Add(fail.Error);
                }
                else
                {
                    return r;
                }
            }
            return new FailResult<A>(state, new ManyErrors(errs.ToSeq()));
        });

    public static Parser<Seq<A>> sepBy1<S, A>(Parser<S> sep, Parser<A> item) =>
        new(state =>
            {
                if (state.IsEOS) return Result.EOS<Seq<A>>(state);

                var s = state;
                var xs = new List<A>();

                var x = item.F(s);
                if (x.IsFail) return x.Cast<Seq<A>>();
                if (x.IsEmpty) throw new InvalidOperationException("Parser within `sepBy1` can return 'empty', this isn't allowed [item]");
                xs.Add(x.Value);
                s = x.State;
                
                while (true)
                {
                    var sr = sep.F(s);
                    if (sr.IsFail) return Result.Success(s, xs.ToSeq());
                    if (sr.IsEmpty) throw new InvalidOperationException("Parser within `sepBy1` can return 'empty', this isn't allowed [sep]");
                    s = sr.State;
                    x = item.F(s);
                    if (sr.IsFail) return x.Cast<Seq<A>>();
                    if (sr.IsEmpty) throw new InvalidOperationException("Parser within `sepBy1` can return 'empty', this isn't allowed [item]");
                    xs.Add(x.Value);
                    s = sr.State;
                }
            });

    public static Parser<Seq<A>> sepBy<S, A>(Parser<S> sep, Parser<A> item) =>
        new(state =>
        {
            if (state.IsEOS) return Result.Success(state, Seq<A>.Empty);

            var s = state;
            var xs = new List<A>();
                
            while (true)
            {
                var sr = sep.F(s);
                if (sr.IsFail) return Result.Success(s, xs.ToSeq());
                if (sr.IsEmpty) throw new InvalidOperationException("Parser within `sepBy` can return 'empty', this isn't allowed [sep]");
                s = sr.State;
                var x = item.F(s);
                if (sr.IsFail) return x.Cast<Seq<A>>();
                if (sr.IsEmpty) throw new InvalidOperationException("Parser within `sepBy` can return 'empty', this isn't allowed [item]");
                xs.Add(x.Value);
                s = sr.State;
            }
        });    

    public static Parser<A> parens<A>(Parser<A> p) =>
        from o in openParen
        from x in p
        from c in closeParen
        select x;

    public static Parser<A> brackets<A>(Parser<A> p) =>
        from o in openBracket
        from x in p
        from c in closeBracket
        select x;

    public static Parser<A> braces<A>(Parser<A> p) =>
        from o in openBrace
        from x in p
        from c in closeBrace
        select x;

    public static Parser<A> angles<A>(Parser<A> p) =>
        from o in openAngle
        from x in p
        from c in closeAngle
        select x;

    public static Parser<Seq<A>> array1<A>(Parser<A> p) =>
        brackets(sepBy1(comma, p));

    public static Parser<Seq<A>> tuple1<A>(Parser<A> p) =>
        parens(sepBy1(comma, p));

    public static Parser<Seq<A>> array<A>(Parser<A> p) =>
        brackets(sepBy(comma, p));

    public static Parser<Seq<A>> tuple<A>(Parser<A> p) =>
        parens(sepBy(comma, p));
    
    public static Parser<A> token<A>(Parser<A> p) =>
        new(state =>
        {
            if (state.IsEOS) return Result.EOS<A>(state);
            var r = p.F(state);
            if (r.IsEmpty) return r;
            var sr = spaces.F(r.State);
            return Result.Success(sr.State, r.Value);
        });

    public static Parser<A> label<A>(this Parser<A> p, string label) =>
        new(state =>
        {
            var r = p.F(state);
            if (r is FailResult<A> fail)
            {
                return fail.Error switch
                                  {
                                      ExpectedError e => Result.Expected<A>(r.State, e.UnexpectedValue, label),
                                      UnexpectedError e => Result.Expected<A>(r.State, e.UnexpectedValue, label),
                                      _ => r
                                  };
            }
            else
            {
                return r;
            }
        });
    
    static bool Contains(char[] chs, char ch)
    {
        foreach(var c in chs)
        {
            if(c == ch) return true;
        }
        return false;
    }
        
    static bool Contains(string chs, char ch)
    {
        foreach(var c in chs)
        {
            if(c == ch) return true;
        }
        return false;
    }
}
