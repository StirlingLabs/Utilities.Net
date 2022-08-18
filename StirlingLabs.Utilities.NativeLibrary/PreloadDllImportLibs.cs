using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities;

[PublicAPI]
public static class PreloadDllImportLibs
{
    private static readonly ConcurrentDictionary<Assembly, bool> Tracker = new();

    
    /// <summary>
    /// Runs <see cref="Execute"/> on all loaded and future assemblies.
    /// </summary>
    public static void Initialize()
    {
        var currentDomain = AppDomain.CurrentDomain;
        foreach (var asm in currentDomain.GetAssemblies())
            Execute(asm);

        currentDomain.AssemblyLoad += (_, args) => Execute(args.LoadedAssembly);
    }

    /// <summary>
    /// Preloads any dynamic shared library imports (<see cref="DllImportAttribute"/> )
    /// in an assembly if the PreloadDllImportLibsAttribute is applied to it.
    /// </summary>
    /// <param name="assembly">The assembly to check and preload imports for.</param>
    /// <returns>false if skipped, otherwise true</returns>
    public static bool Execute(Assembly assembly)
        => Tracker.GetOrAdd(assembly, asm => {
            var attrib = asm.GetCustomAttribute<PreloadDllImportLibsAttribute>();

            if (attrib is null)
                return false;

            foreach (var type in GetAllTypes(assembly))
            {
                foreach (var staticMethod in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    var dllImportAttrib = staticMethod.GetCustomAttribute<DllImportAttribute>();

                    if (dllImportAttrib is null) continue;

                    var libName = dllImportAttrib.Value;

                    try
                    {
                        NativeLibrary.Load(libName, assembly);
                    }
                    catch
                    {
                        // oh darn - may load later
                    }
                }
            }

            return true;
        });

    private static IEnumerable<Type> GetAllTypes(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            yield return type;

            foreach (var nestedType in GetNestedTypesRecursive(type))
                yield return nestedType;
        }
    }

    private static IEnumerable<Type> GetNestedTypesRecursive(Type type)
    {
        foreach (var nestedType in type.GetNestedTypes
                     (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
        {
            yield return nestedType;

            foreach (var subNestedType in GetNestedTypesRecursive(nestedType))
                yield return subNestedType;
        }
    }
}
