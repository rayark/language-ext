using Microsoft.CodeAnalysis;

namespace LanguageExt.SourceGen.Lang;

/// <summary>
/// Declaration
/// </summary>
/// <param name="Name">Name</param>
/// <param name="Location">Source location</param>
internal abstract record Decl(Location Location, string Name)
{
    /// <summary>
    /// Module import 
    /// </summary>
    /// <param name="Name">Name of module</param>
    /// <param name="Location">Source location</param>
    public static Decl Import(Location Location, string Name) => new ImportDecl(Location, Name);

    /// <summary>
    /// Module declaration 
    /// </summary>
    /// <param name="Name">Name of module</param>
    /// <param name="Decls">Declarations that make up the module</param>
    /// <param name="Location">Source location</param>
    public static Decl Module(Location Location, string Name, Decl[] Decls) => 
        new ModuleDecl(Location, Name, Decls);
    
    /// <summary>
    /// Type declaration 
    /// </summary>
    /// <param name="Name">Name of module</param>
    /// <param name="Type">Type definition</param>
    /// <param name="Derivings">Auto derivings</param>
    /// <param name="Location">Source location</param>
    public static Decl Type(Location Location, string Name, Ty Type, Deriving[] Derivings) => 
        new TypeDecl(Location, Name, Type, Derivings);
}

/// <summary>
/// Module import 
/// </summary>
/// <param name="Name">Name of module</param>
/// <param name="Location">Source location</param>
internal record ImportDecl(Location Location, string Name) : Decl(Location, Name);

/// <summary>
/// Module declaration 
/// </summary>
/// <param name="Name">Name of module</param>
/// <param name="Decls">Declarations that make up the module</param>
/// <param name="Location">Source location</param>
internal record ModuleDecl(Location Location, string Name, Decl[] Decls) : Decl(Location, Name);

/// <summary>
/// Type declaration 
/// </summary>
/// <param name="Name">Name of module</param>
/// <param name="Derivings">Auto derivings</param>
/// <param name="Definition">Type definition</param>
/// <param name="Location">Source location</param>
internal record TypeDecl(Location Location, string Name, Ty Definition, Deriving[] Derivings) : Decl(Location, Name);
