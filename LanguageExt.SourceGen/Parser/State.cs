using System;

namespace LanguageExt.SourceGen.Parser;

internal record State(string Source, string Path, int Pos, int Line, int Column)
{
    /// <summary>
    /// True if the end of the stream has been reached
    /// </summary>
    public bool IsEOS => 
        Pos >= Source.Length;

    /// <summary>
    /// Value
    /// </summary>
    public char Value =>
        IsEOS
            ? throw new InvalidOperationException("end-of-stream")
            : Source[Pos];

    public State Next() =>
        IsEOS
            ? this
            : this with
            {
                Pos = Pos + 1, 
                Column = Value == '\n' ? 1 : Column + 1, 
                Line = Value == '\n' ? Line + 1 : Line
            };

}

