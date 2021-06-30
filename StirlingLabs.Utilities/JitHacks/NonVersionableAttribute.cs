// This is a hack to trick the JIT into applying NonVersionable traits where it otherwise wouldn't

namespace System.Runtime.Versioning
{
    [AttributeUsage(
        AttributeTargets.Class
        | AttributeTargets.Struct
        | AttributeTargets.Method
        | AttributeTargets.Constructor,
        Inherited = false)]
    sealed class NonVersionableAttribute : Attribute { }
}
