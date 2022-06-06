using System;

namespace LanguageExt.SourceGen
{
    /// <summary>
    /// Union attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false)]
    public class UnionAttribute : Attribute
    {
    }
}
