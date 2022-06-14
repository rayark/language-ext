using System;
using System.Linq;
using LanguageExt.SourceGen.Lang;
using LanguageExt.SourceGen.Parser;
using static LanguageExt.SourceGen.Parser.Prim;

namespace LanguageExt.SourceGen.Parser;

internal record Decl
{
    static Parser<Decl> usingParser; 
    static Parser<Decl> namespaceParser; 
    static Parser<Decl> aliasParser; 
    static Parser<Decl> unionParser; 
    static Parser<Decl> recordParser; 
    
    static Decl()
    {
    }


    public static Seq<Result<Decl>> Parse(Seq<Block> blocks) =>
        blocks.Select(b => b.Keyword switch
        {
            "using"     => usingParser.Parse(b.Body, b.Path),
            "namespace" => namespaceParser.Parse(b.Body, b.Path),
            "alias"     => aliasParser.Parse(b.Body, b.Path),
            "union"     => unionParser.Parse(b.Body, b.Path),
            "record"    => recordParser.Parse(b.Body, b.Path),
            _           => throw new InvalidProgramException()
        }).ToSeq();
}

/// <summary>
/// namespace An.Example;
/// </summary>
/// <param name="Name">Fully qualified namespace</param>
internal record NamespaceDecl(FQN Name) : Decl;

/// <summary>
/// alias Name X = Y
/// </summary>
/// <param name="X">Type</param>
/// <param name="Y">Type</param>
internal record AliasDecl(string Name, Ty X, Ty Y) : Decl;

/// <summary>
/// Union declaration
/// </summary>
/// <param name="Name">Name</param>
/// <param name="Ty">Definition</param>
/// <param name="Derivings">Derivings</param>
internal record UnionDecl(string Name, TyUnion Ty, Seq<Deriving> Derivings) : Decl;

/// <summary>
/// Record declaration
/// </summary>
/// <param name="Name">Name</param>
/// <param name="Ty">Definition</param>
/// <param name="Derivings">Derivings</param>
internal record RecordDecl(string Name, TyRecord Ty, Seq<Deriving> Derivings) : Decl;
