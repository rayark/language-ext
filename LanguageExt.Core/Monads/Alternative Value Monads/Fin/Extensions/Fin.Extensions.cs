using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using static LanguageExt.Prelude;
using System.Diagnostics.Contracts;
using LanguageExt.TypeClasses;
using LanguageExt.Common;

/// <summary>
/// Extension methods for Fin
/// </summary>
public static partial class FinExtensions
{
    /// <summary>
    /// Monadic join
    /// </summary>
    [Pure]
    public static Fin<R> Flatten<R>(this Fin<Fin<R>> ma) =>
        ma.Bind(identity);

    /// <summary>
    /// Add the bound values of x and y, uses an Add type-class to provide the add
    /// operation for type A.  For example x.Add<TInteger,int>(y)
    /// </summary>
    /// <typeparam name="NUM">Num of A</typeparam>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <param name="x">Left hand side of the operation</param>
    /// <param name="y">Right hand side of the operation</param>
    /// <returns>Fin with y added to x</returns>
    [Pure]
    public static Fin<R> Plus<NUM, R>(this Fin<R> x, Fin<R> y) where NUM : struct, Num<R> =>
        from a in x
        from b in y
        select default(NUM).Plus(a, b);

    /// <summary>
    /// Find the subtract between the two bound values of x and y, uses a Subtract type-class 
    /// to provide the subtract operation for type A.  For example x.Subtract<TInteger,int>(y)
    /// </summary>
    /// <typeparam name="NUM">Num of A</typeparam>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <param name="x">Left hand side of the operation</param>
    /// <param name="y">Right hand side of the operation</param>
    /// <returns>Fin with the subtract between x and y</returns>
    [Pure]
    public static Fin<R> Subtract<NUM, R>(this Fin<R> x, Fin<R> y) where NUM : struct, Num<R> =>
        from a in x
        from b in y
        select default(NUM).Subtract(a, b);

    /// <summary>
    /// Find the product between the two bound values of x and y, uses a Product type-class 
    /// to provide the product operation for type A.  For example x.Product<TInteger,int>(y)
    /// </summary>
    /// <typeparam name="NUM">Num of A</typeparam>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <param name="x">Left hand side of the operation</param>
    /// <param name="y">Right hand side of the operation</param>
    /// <returns>Fin with the product of x and y</returns>
    [Pure]
    public static Fin<R> Product<NUM, R>(this Fin<R> x, Fin<R> y) where NUM : struct, Num<R> =>
        from a in x
        from b in y
        select default(NUM).Product(a, b);

    /// <summary>
    /// Divide the two bound values of x and y, uses a Divide type-class to provide the divide
    /// operation for type A.  For example x.Divide<TDouble,double>(y)
    /// </summary>
    /// <typeparam name="NUM">Num of A</typeparam>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <param name="x">Left hand side of the operation</param>
    /// <param name="y">Right hand side of the operation</param>
    /// <returns>Fin x / y</returns>
    [Pure]
    public static Fin<R> Divide<NUM, R>(this Fin<R> x, Fin<R> y) where NUM : struct, Num<R> =>
        from a in x
        from b in y
        select default(NUM).Divide(a, b);

    /// <summary>
    /// Extracts from a list of Fins all the Succ elements.
    /// </summary>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <param name="xs">Sequence of Fins</param>
    /// <returns>An enumerable of A</returns>
    [Pure]
    public static IEnumerable<A> Succs<A>(this IEnumerable<Fin<A>> xs) =>
        xs.Where(x => x.IsSucc).Select(x => x.Value);

    /// <summary>
    /// Extracts from a list of Fins all the Succ elements.
    /// </summary>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <param name="xs">Sequence of Fins</param>
    /// <returns>An enumerable of A</returns>
    [Pure]
    public static Seq<A> Succs<A>(this Seq<Fin<A>> xs) =>
        xs.Where(x => x.IsSucc).Select(x => x.Value);

    /// <summary>
    /// Extracts from a list of Fins all the Fail elements.
    /// </summary>
    /// <remarks>Bottom values are dropped</remarks>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <param name="xs">Sequence of Fins</param>
    /// <returns>An enumerable of Errors</returns>
    [Pure]
    public static IEnumerable<Error> Fails<A>(this IEnumerable<Fin<A>> xs) =>
        xs.Where(x => x.IsFail).Select(x => x.Error);

    /// <summary>
    /// Extracts from a list of Fins all the Fail elements.
    /// </summary>
    /// <remarks>Bottom values are dropped</remarks>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <param name="xs">Sequence of Fins</param>
    /// <returns>An enumerable of Errors</returns>
    [Pure]
    public static Seq<Error> Fails<A>(this Seq<Fin<A>> xs) =>
        xs.Filter(x => x.IsFail).Map(x => x.Error);

    /// <summary>
    /// Partitions a list of 'Fin' into two lists.
    /// All the Fail elements are extracted, in order, to the first
    /// component of the output.  Similarly the Succ elements are extracted
    /// to the second component of the output.
    /// </summary>
    /// <remarks>Bottom values are dropped</remarks>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <param name="xs">Fin list</param>
    /// <returns>A tuple containing the an enumerable of Erorr and an enumerable of Succ</returns>
    [Pure]
    public static (IEnumerable<Error> Fails, IEnumerable<A> Succs) Partition<A>(this IEnumerable<Fin<A>> xs)
    {
        var fs = new List<Error>();
        var rs = new List<A>();
        
        foreach(var x in xs)
        {
            if (x.IsSucc)
            {
                rs.Add(x.Value);
            }
            if (x.IsFail)
            {
                fs.Add(x.Error);
            }
        }

        return (fs, rs);
    }

    /// <summary>
    /// Partitions a list of 'Fin' into two lists.
    /// All the Fail elements are extracted, in order, to the first
    /// component of the output.  Similarly the Succ elements are extracted
    /// to the second component of the output.
    /// </summary>
    /// <remarks>Bottom values are dropped</remarks>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <param name="xs">Fin list</param>
    /// <returns>A tuple containing the an enumerable of Erorr and an enumerable of Succ</returns>
    [Pure]
    public static (Seq<Error> Fails, Seq<A> Succs) Partition<A>(this Seq<Fin<A>> xs)
    {
        var fs = Seq<Error>();
        var rs = Seq<A>();
        
        foreach(var x in xs)
        {
            if (x.IsSucc)
            {
                rs = rs.Add(x.Value);
            }
            if (x.IsFail)
            {
                fs = fs.Add(x.Error);
            }
        }

        return (fs, rs);
    }
}
