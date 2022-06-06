using System;

namespace LanguageExt.SourceGen.Parser;

internal static class Prim
{
    static Prim()
    {
        any = new Parser<char>(state =>
                {
                    if (state.IsEOS) return Result.Unexpected<char>(state, "eof");
                    var ch = state.Source[state.Pos];
                    return Result.Success(state with {
                        Pos = state.Pos + 1,
                        Column = ch == '\n' ? 1 : state.Column + 1,
                        Line = ch == '\n' ? state.Line + 1 : state.Line }, ch);
                });
                
        space = satisfy(Char.IsWhiteSpace);            
    }

    public static readonly Parser<char> any;
    public static readonly Parser<char> space; 

    public static Parser<char> satisfy(Func<char, bool> f) => 
        new (state =>
        {
            if (state.IsEOS) return Result.Unexpected<char>(state, "eof");
            var ch = state.Source[state.Pos];
            if(!f(ch)) return Result.Unexpected<char>(state, $"'{ch}'");
            return Result.Success(state with {
                Pos = state.Pos + 1,
                Column = ch == '\n' ? 1 : state.Column + 1,
                Line = ch == '\n' ? state.Line + 1 : state.Line }, ch);
        });

    public static Parser<char> ch(char c) => 
        new (state =>
        {
            if (state.IsEOS) return Result.Unexpected<char>(state, "eof");
            var ch = state.Source[state.Pos];
            if(ch != c) return Result.Unexpected<char>(state, $"'{ch}'");
            return Result.Success(state with {
                Pos = state.Pos + 1,
                Column = ch == '\n' ? 1 : state.Column + 1,
                Line = ch == '\n' ? state.Line + 1 : state.Line }, ch);
        });

    public static Parser<Unit> spaces => 
        new (state =>
        {
            while(true)
            {
                if (state.IsEOS) return Result.Success<Unit>(state, default);
                var ch = state.Source[state.Pos];
                if(Char.IsWhiteSpace(ch)) return Result.Success<Unit>(state, default);
                return Result.Success<Unit>(state with {
                    Pos = state.Pos + 1,
                    Column = ch == '\n' ? 1 : state.Column + 1,
                    Line = ch == '\n' ? state.Line + 1 : state.Line }, default);
            }
        });

    public static Parser<char> oneOf(params char[] chs) => 
        new (state =>
        {
            if (state.IsEOS) return Result.Unexpected<char>(state, "eof");
            var ch = state.Source[state.Pos];
            if(!Contains(chs, ch)) return Result.Unexpected<char>(state, $"'{ch}'");
            return Result.Success(state with {
                Pos = state.Pos + 1,
                Column = ch == '\n' ? 1 : state.Column + 1,
                Line = ch == '\n' ? state.Line + 1 : state.Line }, ch);
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
