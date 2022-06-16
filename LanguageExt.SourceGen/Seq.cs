using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LanguageExt.SourceGen;

internal static class Seq
{
    public static Seq<A> From<A>(params A[] items) =>
        new (items, 0, items.Length);

    public static Seq<A> From<A>(IEnumerable<A> items) =>
        From(items.ToArray());

    public static Seq<A> Cons<A>(this A x, Seq<A> xs) =>
        xs.Cons(x);

    public static Seq<A> ToSeq<A>(this A[] items) =>
        From(items);

    public static Seq<A> ToSeq<A>(this IEnumerable<A> items) =>
        From(items);
}

public class Seq<A> : IEnumerable<A>, IEquatable<Seq<A>>
{
    public static readonly Seq<A> Empty = new (Array.Empty<A>(), 0, 0);
    
    readonly A[] items;
    readonly int start;
    readonly int Count;

    public Seq(A[] items, int start, int count) =>
        (this.items, this.start, Count) = (items, start, count);

    public IEnumerator<A> GetEnumerator()
    {
        for (var i = start; i < start+Count; i++)
        {
            yield return items[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => 
        GetEnumerator();

    public static bool operator ==(Seq<A> lhs, Seq<A> rhs) =>
        lhs.Equals(rhs);
    
    public static bool operator !=(Seq<A> lhs, Seq<A> rhs) =>
        !(lhs == rhs);
    
    public bool Equals(Seq<A>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return start == other.start && 
               Count == other.Count && 
               items.Length == other.items.Length &&
               ItemsEq(this, other);
    }

    static bool ItemsEq(Seq<A> lhs, Seq<A> rhs)
    {
        var i = lhs.start;
        var j = rhs.start;
        var imax = lhs.start + lhs.Count;
        var jmax = rhs.start + rhs.Count;

        for (; i < imax && j < jmax; i++, j++)
        {
            var l = lhs.items[i];
            var r = rhs.items[i];
            if (ReferenceEquals(r, l)) continue;
            if (l is null && r is not null) return false;
            if (r is null && l is not null) return false;
            #nullable disable
            if (!l.Equals(r)) return false;
            #nullable enable
        }
        return true;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Seq<A>) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = items.GetHashCode();
            hashCode = (hashCode * 397) ^ start;
            hashCode = (hashCode * 397) ^ Count;
            return ItemsHash(hashCode);
        }
    }
    
    public int ItemsHash(int hash)
    {
        unchecked
        {
            for (var x = start; x < start + Count; x++)
            {
                hash = (hash ^ items[x]?.GetHashCode() ?? 0) * 16777619;
            }
            return hash;
        }
    }

    public Seq<A> Concat(Seq<A> rhs)
    {
        var xs = new A[Count + rhs.Count];
        Array.Copy(items, start, xs, 0, Count);
        Array.Copy(rhs.items, rhs.start, xs, Count, rhs.Count);
        return new Seq<A>(xs, 0, xs.Length);
    }

    public static Seq<A> operator +(Seq<A> lhs, Seq<A> rhs) =>
        lhs.Concat(rhs);

    public A this[int ix] =>
        items[start + ix];

    public Seq<A> Add(A value)
    {
        var xs = new A[Count + 1];
        Array.Copy(items, start, xs, 0, Count);
        xs[Count] = value;
        return new Seq<A>(xs, 0, xs.Length);
    }
    
    public Seq<A> Cons(A value)
    {
        var xs = new A[Count + 1];
        Array.Copy(items, start, xs, 1, Count);
        xs[0] = value;
        return new Seq<A>(xs, 0, xs.Length);
    }
}
