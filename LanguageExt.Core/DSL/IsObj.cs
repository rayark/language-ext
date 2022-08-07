namespace LanguageExt.DSL;

public interface IsObj<M, A>
{
    Obj<A> ToObject(M value);
}
