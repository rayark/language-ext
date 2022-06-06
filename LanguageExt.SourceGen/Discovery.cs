using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LanguageExt.SourceGen;

[Generator]
public class Discovery : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) => 
        throw new System.NotImplementedException();

    public void Execute(GeneratorExecutionContext context) => 
        throw new System.NotImplementedException();
}

class Receiver : ISyntaxReceiver
{
    /// <summary>
    /// Set of attributes that are acceptable on an interface
    /// </summary>
    static readonly string[] InterfaceAttrs = new[] { "Union" };

    /// <summary>
    /// The operation to perform
    /// </summary>
    public Operation[] Operation { get; private set; } = Array.Empty<Operation>();

    /// <summary>
    /// Visitor that accepts nodes or not 
    /// </summary>
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        Operation = syntaxNode switch
        {
//            InterfaceDeclarationSyntax @interface => Handle(@interface),
//            ClassDeclarationSyntax @class         => Handle(@class),
//            _                                     => Operation.None    
        };
    }

    /*
    static Operation[] Handle(InterfaceDeclarationSyntax @interface) =>
        Attr.GetAttr(@interface.AttributeLists, InterfaceAttrs)
            .SelectMany(a => a.Name.ToString() switch
                            {
                                "Union" => HandleUnion(@interface)
                            })
            .ToArray();
            */

    //static Operation[] HandleUnion(InterfaceDeclarationSyntax @interface) =>
    //    Operation.One(Operation.Union(@interface));
}
