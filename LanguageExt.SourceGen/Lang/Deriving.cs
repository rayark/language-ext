namespace LanguageExt.SourceGen.Lang;

/// <summary>
/// Auto derivings union
/// </summary>
internal abstract record Deriving
{
    public static readonly Deriving Eq = new EqDeriving();
    public static readonly Deriving Ord = new OrdDeriving();
    public static readonly Deriving Functor = new FunctorDeriving();
    public static readonly Deriving Show = new ShowDeriving();
    public static readonly Deriving Json = new JsonDeriving();
}

internal record EqDeriving : Deriving;
internal record OrdDeriving : Deriving;
internal record FunctorDeriving : Deriving;
internal record ShowDeriving : Deriving;
internal record JsonDeriving : Deriving;
