using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LanguageExt.SourceGen;

/// <summary>
/// DSL of operations
/// </summary>
abstract record Operation
{
    public static Operation[] One(Operation op) => 
        new[] {op};
    
    public static Operation[] Many(params Operation[] ops) => 
        ops;
    
    public static Operation[] Many(IEnumerable<Operation> ops) => 
        ops.ToArray();
    
    public static Operation Union(InterfaceDeclarationSyntax @interface) => 
        new UnionOperation(@interface);
}

/// <summary>
/// Union 
/// </summary>
record UnionOperation(
    
    
    InterfaceDeclarationSyntax Interface
    
    ) : Operation;
