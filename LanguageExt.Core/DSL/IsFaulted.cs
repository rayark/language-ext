using System;

namespace LanguageExt.DSL;

public interface IsFaulted<A>
{
    bool IsFaulted(A value);
    Prim<A> MakeFail(Exception e);
    
}
