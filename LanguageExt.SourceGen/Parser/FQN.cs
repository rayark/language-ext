
using LanguageExt.SourceGen;

public record FQN(Seq<string> Idents)
{
    public static FQN New(Seq<string> idents) =>
        new FQN(idents);
}
