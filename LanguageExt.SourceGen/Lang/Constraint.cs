
namespace LanguageExt.SourceGen.Lang;

internal abstract record Constraint
{
    /// <summary>
    /// Subtype constraint
    /// </summary>
    /// <param name="type">Super type</param>
    public static Constraint Type(Ty type) => new TypeConstraint(type);
}

/// <summary>
/// Subtype constraint
/// </summary>
/// <param name="SuperType">Super type</param>
internal record TypeConstraint(Ty SuperType) : Constraint; 
