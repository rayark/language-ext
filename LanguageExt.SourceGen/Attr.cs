using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LanguageExt.SourceGen;

/// <summary>
/// Tooling for working with attributes
/// </summary>
internal class Attr
{
    /// <summary>
    /// Get the attributes that have names that match the names array provided
    /// </summary>
    public static AttributeSyntax[] GetAttr(SyntaxList<AttributeListSyntax> attrs, string[] names) =>
        attrs.Any()
            ? GetAttr(attrs, a => names.Contains(a.Name.ToString()))
            : Array.Empty<AttributeSyntax>();
        
    /// <summary>
    /// Run a predicate on all the attributes and return the ones that success
    /// </summary>
    public static AttributeSyntax[] GetAttr(SyntaxList<AttributeListSyntax> attrs, Func<AttributeSyntax, bool> f) =>
        attrs.Any()
            ? attrs.SelectMany(al => al.Attributes.Where(f)).ToArray()
            : Array.Empty<AttributeSyntax>();
}
