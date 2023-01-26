using LanguageExt.DSL.Transducers;

namespace LanguageExt.DSL2;

public interface IsTransducer<in A, out B>
{
    Transducer<A, B> ToTransducer();
}

public interface IsTransducerAsync<in A, out B>
{
    TransducerAsync<A, B> ToTransducerAsync();
}

public interface IsSumTransducer<X, out Y, A, out B>
{
    SumTransducer<X, Y, A, B> ToSumTransducer();
}

public interface IsSumTransducerAsync<X, out Y, A, out B>
{
    SumTransducerAsync<X, Y, A, B> ToSumTransducerAsync();
}

public interface IsProductTransducer<X, out Y, A, out B>
{
    ProductTransducer<X, Y, A, B> ToProductTransducer();
}

public interface IsProductTransducerAsync<X, out Y, A, out B>
{
    ProductTransducerAsync<X, Y, A, B> ToProductTransducerAsync();
}
