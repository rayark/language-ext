namespace LanguageExt.SourceGen.Lang;

/// <summary>
/// Type 
/// </summary>
internal abstract record Ty;

/// <summary>
/// Type variable
/// </summary>
/// <param name="Name">Type variable</param>
/// <param name="Constraints">Type constraint</param>
internal record TyVar(string Name, Constraint[] Constraints) : Ty;

/// <summary>
/// Concrete type name
/// </summary>
/// <param name="Name">Type name</param>
internal record TyId(string Name) : Ty;

/// <summary>
/// Type arrow X -> Y (`Func<X, Y>`)
/// </summary>
/// <param name="X">From</param>
/// <param name="Y">To</param>
internal record TyArr(Ty X, Ty Y) : Ty;

/// <summary>
/// Type application X<Y> (List<int>)
/// </summary>
/// <param name="A">Type arrow</param>
/// <param name="Y">Type argument to apply</param>
internal record TyApp(Ty A, Ty B) : Ty;

/// <summary>
/// Array type
/// </summary>
/// <param name="Type">Value type</param>
internal record TyArray(Ty Type) : Ty;

/// <summary>
/// Named type - used for fields (for example)
/// </summary>
/// <param name="Name">Name</param>
/// <param name="Type">Type</param>
internal record TyNamed(string Name, Ty Type) : Ty;

/// <summary>
/// Tuple type
/// </summary>
/// <param name="Types">Item types</param>
internal record TyTuple(Ty[] Types) : Ty;

/// <summary>
/// Record type
/// </summary>
/// <param name="Fields"></param>
internal record TyRecord(TyNamed[] Fields);

/// <summary>
/// Union type
/// </summary>
/// <param name="Cases"></param>
internal record TyUnion(TyNamed[] Cases);
