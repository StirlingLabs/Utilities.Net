// This is a hack to trick the JIT into applying Intrinsic traits where it otherwise wouldn't

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(
        AttributeTargets.Class
        | AttributeTargets.Struct
        | AttributeTargets.Method
        | AttributeTargets.Constructor
        | AttributeTargets.Field,
        Inherited = false)]
    internal sealed class IntrinsicAttribute : Attribute { }
}
