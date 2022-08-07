namespace LanguageExt.DSL;

public interface IsMorphism<in M, A, B>
{
    Morphism<A, B> ToMorphism(M value);
}
