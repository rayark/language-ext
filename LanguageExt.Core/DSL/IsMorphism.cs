using LanguageExt.DSL.Transducers;

namespace LanguageExt.DSL;

public interface IsTransducer<in A, out B>
{
    Transducer<A, B> ToTransducer();
}
