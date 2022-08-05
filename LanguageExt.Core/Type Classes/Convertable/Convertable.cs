namespace LanguageExt.TypeClasses;

/// <summary>
/// A trait that allows conversion from one type to another
/// </summary>
public interface Convertable<FROM, TO>
{
    TO Convert(FROM source);
}
