namespace LanguageExt.SourceGen.Parser;

internal record State(string Source, string Path, int Pos, int Line, int Column)
{
    /// <summary>
    /// True if the end of the stream has been reached
    /// </summary>
    public bool IsEOS => 
        Pos >= Source.Length; 
}

