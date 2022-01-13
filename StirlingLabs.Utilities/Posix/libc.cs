using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace StirlingLabs.Utilities
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal static unsafe class libc
    {
        public const string LibName = "libc";

#if NET5_0_OR_GREATER
        [SuppressGCTransition]
#endif
        [DllImport(LibName, SetLastError = true)]
        public static extern void* malloc(nuint size);

#if NET5_0_OR_GREATER
        [SuppressGCTransition]
#endif
        [DllImport(LibName, SetLastError = true)]
        public static extern void* calloc(nuint number, nuint size);

#if NET5_0_OR_GREATER
        [SuppressGCTransition]
#endif
        [DllImport(LibName, SetLastError = true)]
        public static extern void free(void* ptr);

#if NET5_0_OR_GREATER
        [SuppressGCTransition]
#endif
        [DllImport(LibName, SetLastError = true)]
        public static extern void* realloc(void* ptr, nuint newSize);

#if NET5_0_OR_GREATER
        [SuppressGCTransition]
#endif
        [DllImport(LibName, SetLastError = true)]
        public static extern void* memmove(void* to, void* from, nuint size);
    }
}
