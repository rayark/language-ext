namespace LanguageExt.SourceGen.Lang;

/// <summary>
/// Type 
/// </summary>
internal abstract record Ty
{
    public static readonly Ty Bool = Id("System.Boolean"); 
    public static readonly Ty Byte = Id("System.Byte"); 
    public static readonly Ty SByte = Id("System.SByte"); 
    public static readonly Ty Char = Id("System.Char"); 
    public static readonly Ty Decimal = Id("System.Decimal"); 
    public static readonly Ty Double = Id("System.Double"); 
    public static readonly Ty Single = Id("System.Single"); 
    public static readonly Ty Int32 = Id("System.Int32"); 
    public static readonly Ty UInt32 = Id("System.UInt32"); 
    public static readonly Ty IntPtr = Id("System.IntPtr"); 
    public static readonly Ty UIntPtr = Id("System.UIntPtr"); 
    public static readonly Ty Long = Id("System.Long"); 
    public static readonly Ty ULong = Id("System.ULong"); 
    public static readonly Ty Short = Id("System.Short"); 
    public static readonly Ty UShort = Id("System.UShort"); 
    public static readonly Ty Object = Id("System.Object"); 
    public static readonly Ty String = Id("System.String"); 
    public static readonly Ty Dynamic = Id("System.Dynamic");

    public static Ty Array(Ty itemTy) =>
        Lam("x", Arr(Var("x"), Id("Arr")));
    
    public static Ty Option(Ty itemTy) =>
        Lam("x", Arr(Var("x"), Id("Option")));
    
    public static Ty Var(string name, params Constraint[] constraints) =>
        new TyVar(name, constraints);
    
    public static Ty Id(string name) =>
        new TyId(name);
    
    public static Ty Arr(Ty x, Ty y) =>
        new TyArr(x, y);
    
    public static Ty App(Ty x, Ty y) =>
        new TyApp(x, y);
    
    public static Ty Lam(string name, Ty body) =>
        new TyLam(name, body);

    public static Ty Named(string name, Ty ty) =>
        new TyNamed(name, ty);

    public static Ty Tuple(Ty[] itemTys) =>
        new TyTuple(itemTys);

    public static Ty Record(TyNamed[] fieldTys) =>
        new TyRecord(fieldTys);

    public static Ty Union(TyNamed[] caseTys) =>
        new TyUnion(caseTys);
}

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
/// Type arrow `X => Y` (`Y<X>`)
/// </summary>
/// <param name="X">Argument</param>
/// <param name="Y">Return value</param>
internal record TyArr(Ty X, Ty Y) : Ty;

/// <summary>
/// Type application
///
/// Should be applied to `TyLam` values.
///
///     b(a)
///     
/// </summary>
/// <param name="A">Type lambda</param>
/// <param name="Y">Type argument to apply</param>
internal record TyApp(Ty A, Ty B) : Ty;

/// <summary>
/// Type lambda
///
///     Name => Body
///
/// For example a type of `List<A>`:
///
///     a => List a
/// 
/// </summary>
/// <param name="Name">Argument</param>
/// <param name="Body">Type</param>
internal record TyLam(string Name, Ty Body) : Ty;

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
internal record TyRecord(TyNamed[] Fields) : Ty;

/// <summary>
/// Union type
/// </summary>
/// <param name="Cases"></param>
internal record TyUnion(TyNamed[] Cases) : Ty;
