using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using LLVMSharp.Interop;
using StirlingLabs.Utilities.Extensions;

namespace StirlingLabs.Utilities;

[PublicAPI]
internal static class LlvmMcJitContext
{
    private static long _modCounter;

    private static long _dlgCounter;

    internal static readonly int LlvmSuccess = default;

    private static readonly unsafe string TargetTriple = GetStringFromLlvmUtf8Error(LLVM.GetDefaultTargetTriple(), true, false);

    internal static readonly ConcurrentQueue<Action> ShutdownActions = new();

    static unsafe LlvmMcJitContext()
    {
        LLVM.LinkInMCJIT();

        LlvmOptions.OptLevel = 3;
        LlvmOptions.NoFramePointerElim = 0;
        LlvmOptions.EnableFastISel = 1;
        LlvmOptions.CodeModel = LLVMCodeModel.LLVMCodeModelJITDefault; // LLVMCodeModelDefault

        LLVM.ContextSetDiagnosticHandler(
            LLVM.GetGlobalContext(),
            LlvmDiagnosticHandlerHandle,
            (void*)(IntPtr)_modCounter
        );

        try
        {
            LLVM.InitializeNativeTarget();
        }
        catch (EntryPointNotFoundException)
        {
            // ffs
        }

        /*LLVM.InitializeWebAssemblyTargetInfo();
        LLVM.InitializeWebAssemblyTarget();
        LLVM.InitializeWebAssemblyTargetMC();
        LLVM.InitializeWebAssemblyAsmParser();
        LLVM.InitializeWebAssemblyAsmPrinter();*/

        switch (RuntimeInformation.ProcessArchitecture)
        {
            case Architecture.X86:
            case Architecture.X64:
                LLVM.InitializeX86TargetInfo();
                LLVM.InitializeX86Target();
                LLVM.InitializeX86TargetMC();
                LLVM.InitializeX86AsmParser();
                LLVM.InitializeX86AsmPrinter();
                break;
            case Architecture.Arm:
                LLVM.InitializeARMTargetInfo();
                LLVM.InitializeARMTarget();
                LLVM.InitializeARMTargetMC();
                LLVM.InitializeARMAsmParser();
                LLVM.InitializeARMAsmPrinter();
                break;
            case Architecture.Arm64:
                LLVM.InitializeAArch64TargetInfo();
                LLVM.InitializeAArch64Target();
                LLVM.InitializeAArch64TargetMC();
                LLVM.InitializeAArch64AsmParser();
                LLVM.InitializeAArch64AsmPrinter();
                break;
            default:
                throw new PlatformNotSupportedException(nameof(RuntimeInformation.ProcessArchitecture));
        }

        if (LLVM.IsMultithreaded() == 0)
            LLVM.StartMultithreaded();
        if (LLVM.IsMultithreaded() == 0)
            throw new PlatformNotSupportedException("LLVM must be able to operate in thread-safe mode.");

        AppDomain.CurrentDomain.ProcessExit += (_, _) => {
            foreach (var action in ShutdownActions)
                action();
            LLVM.StopMultithreaded();
            LLVM.Shutdown();
            Marshal.CleanupUnusedObjectsInCurrentContext();
        };
    }

    [DllImport("libLLVM", CallingConvention = CallingConvention.Cdecl, EntryPoint = "LLVMPassManagerBuilderSetOptLevel", ExactSpelling = true)]
    public static extern unsafe void PassManagerBuilderSetOptLevel(LLVMOpaquePassManagerBuilder* PMB, uint OptLevel);


    private static unsafe LLVMOpaqueModule* CreateModule(string name, out LLVMOpaqueExecutionEngine* engine, out LLVMOpaquePassManager* mpm,
        out LLVMOpaquePassManager* fpm)
    {

        LLVMOpaqueModule* module;
        fixed (byte* utf8Buf = &Encoding.UTF8.GetBytes($"{nameof(StirlingLabs)}_" + name)[0])
            module = LLVM.ModuleCreateWithName((sbyte*)utf8Buf);

        var defaultTriple = LLVM.GetDefaultTargetTriple();
        LLVMTarget* target;
        sbyte* error;
        var result = LLVM.GetTargetFromTriple(defaultTriple, &target, &error);
        var cpu = LLVM.GetHostCPUName();
        var feats = LLVM.GetHostCPUFeatures();
        var tm = LLVM.CreateTargetMachine(
            target,
            defaultTriple,
            cpu,
            feats,
            LLVMCodeGenOptLevel.LLVMCodeGenLevelAggressive,
            LLVMRelocMode.LLVMRelocDefault,
            LLVMCodeModel.LLVMCodeModelJITDefault
        );
        LLVM.SetTargetMachineAsmVerbosity(tm, 1);
        LLVM.SetTarget(module, defaultTriple);
        var td = LLVM.CreateTargetDataLayout(tm);
        LLVM.SetModuleDataLayout(module, td);

        fixed (LLVMMCJITCompilerOptions* pLlvmOptions = &LlvmOptions)
        fixed (LLVMOpaqueExecutionEngine** pEngine = &engine)
        {

            var optsSize = (UIntPtr)sizeof(LLVMMCJITCompilerOptions);
            error = null;

            if (LLVM.CreateMCJITCompilerForModule(pEngine, module, pLlvmOptions, optsSize, &error) != LlvmSuccess)
                throw new("Machine Code Just-In-Time Compiler failed to initialize.",
                    new(GetStringFromLlvmUtf8Error(error, true, true)));
        }


        fpm = LLVM.CreateFunctionPassManagerForModule(module);
        mpm = LLVM.CreatePassManager();

        LLVM.AddAnalysisPasses(tm, fpm);
        LLVM.AddAnalysisPasses(tm, mpm);
        
        var pmb = LLVM.PassManagerBuilderCreate();
        PassManagerBuilderSetOptLevel(pmb, 3);
        LLVM.PassManagerBuilderSetSizeLevel(pmb, 0);
        LLVM.PassManagerBuilderUseInlinerWithThreshold(pmb, 4096);
        LLVM.PassManagerBuilderAddCoroutinePassesToExtensionPoints(pmb);
        LLVM.PassManagerBuilderPopulateFunctionPassManager(pmb, fpm);
        LLVM.PassManagerBuilderPopulateModulePassManager(pmb, mpm);
        LLVM.PassManagerBuilderDispose(pmb);

        var engineCopy = engine;
        var fpmCopy = fpm;
        var mpmCopy = mpm;

        ShutdownActions.Enqueue(() => {
            if (module != null)
                LLVM.DisposeModule(module);
            if (tm != null)
                LLVM.DisposeTargetMachine(tm);
            if (td != null)
                LLVM.DisposeTargetData(td);
            if (fpmCopy != null)
                LLVM.DisposePassManager(fpmCopy);
            if (mpmCopy != null)
                LLVM.DisposePassManager(mpmCopy);
            if (engineCopy != null)
                LLVM.DisposeExecutionEngine(engineCopy);
        });

        if (LLVM.InitializeFunctionPassManager(fpm) != LlvmSuccess)
            throw new("Machine Code Just-In-Time Compiler failed to initialize.",
                new("Function Pass Manager failed to initialize."));

        return module;
    }

    private static unsafe void VerifyModule(LLVMOpaqueModule* mod)
    {
        sbyte* error = null;
        if (LLVM.VerifyModule(mod, LLVMVerifierFailureAction.LLVMPrintMessageAction, &error) != LlvmSuccess)
            throw new("Module verification failed.\n" + GetStringFromLlvmUtf8Error(error, true, true));
    }
    private static unsafe void VerifyFunction(LLVMOpaqueValue* fn)
    {
        if (LLVM.VerifyFunction(fn, LLVMVerifierFailureAction.LLVMReturnStatusAction) != LlvmSuccess)
            throw new("Function verification failed.");
    }

    internal static unsafe LLVMOpaqueType* GetLlvmType(Type t)
    {
        if (t.IsPointer || t.IsByRef)
            return LLVM.PointerType(GetLlvmType(t.GetElementType()!), 0);

        if (t == typeof(bool) || t == typeof(byte) || t == typeof(sbyte))
            return LLVM.Int8Type();

        if (t == typeof(short) || t == typeof(ushort))
            return LLVM.Int16Type();

        if (t == typeof(int) || t == typeof(uint))
            return LLVM.Int32Type();

        if (t == typeof(long) || t == typeof(ulong))
            return LLVM.Int64Type();

        if (t == typeof(float))
            return LLVM.FloatType();

        if (t == typeof(double))
            return LLVM.DoubleType();

        /*if (t == typeof(Int128)
            || t == typeof(UInt128))
            return LLVM.Int128Type();*/

        if (t == typeof(nint) || t == typeof(nuint))
            return sizeof(nint) switch
            {
                4 => LLVM.Int32Type(),
                8 => LLVM.Int64Type(),
                _ => LLVM.PointerType(LLVM.VoidType(), 0)
            };

        if (t == typeof(void))
            return LLVM.VoidType();

#if !NETSTANDARD
        if (t == typeof(Half))
            return LLVM.HalfType();
#endif

        throw new NotImplementedException(t.FullName);
    }

    internal unsafe delegate void LlvmCreateDelegateBuilderDelegate(LLVMOpaqueBuilder* builder, LLVMOpaqueType* funcType,
        LLVMOpaqueValue* func);

    internal static unsafe void* LlvmCreateFnPtr<TDelegate>(LlvmCreateDelegateBuilderDelegate dlgBuild,
        string? name = null) where TDelegate : Delegate
    {
        var context = NewContext(out var module, out var engine, out var mpm, out var fpm);

        var invoke = typeof(TDelegate).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (invoke == null)
            throw new MissingMethodException("Can't find Invoke method on delegate type.");

        var invokeParams = invoke.GetParameters();
        var paramsLength = invokeParams.Length;
        var paramTypes = new LLVMOpaqueType*[paramsLength];
        for (var i = 0; i < paramsLength; i++)
        {
            var p = invokeParams[i];
            var pt = p.ParameterType;
            paramTypes[i] = GetLlvmType(pt);
        }

        var retType = GetLlvmType(invoke.ReturnType);
        LLVMOpaqueType* funcType;
        fixed (LLVMOpaqueType** pParamTypes = &paramTypes[0])
            funcType = LLVM.FunctionType(retType, pParamTypes, (uint)paramsLength, 0);

        name ??= typeof(TDelegate).Name + "_" + Interlocked.Increment(ref _dlgCounter);

        Debug.WriteLine($"LlvmMcJitContext.LlvmCreateFnPtr generating {name}");

        fixed (byte* nameUtf8 = &Encoding.UTF8.GetBytes(name)[0])
        {
            var fn = LLVM.AddFunction(module, (sbyte*)nameUtf8, funcType);
            LLVMOpaqueBasicBlock* entry;
            fixed (byte* utf8Buf = &Encoding.UTF8.GetBytes($"{name}_EntryPoint")[0])
                entry = LLVM.AppendBasicBlock(fn, (sbyte*)utf8Buf);

            var builder = LLVM.CreateBuilderInContext(context);
            LLVM.PositionBuilderAtEnd(builder, entry);
            dlgBuild(builder, funcType, fn);
            LLVM.DisposeBuilder(builder);
            LLVM.RunFunctionPassManager(fpm, fn);
            LLVM.RunPassManager(mpm, module);

            //if (LLVM.VerifyFunction(fn, LLVMVerifierFailureAction.LLVMReturnStatusAction) != LlvmSuccess)
            //  throw new InvalidOperationException("Function failed to pass verifier.");

            //VerifyFunction(fn);

            LLVM.DumpModule(module);
            VerifyModule(module);

            var pFn = LLVM.GetPointerToGlobal(engine, fn);
            //dlg = Marshal.GetDelegateForFunctionPointer<TDelegate>(pFn);

            //LLVM.FinalizeFunctionPassManager(fpm);

            return pFn;
        }
    }

    private static readonly unsafe LlvmDiagnosticHandlerDelegate LlvmDiagnosticHandlerDelegatePersistRef = LlvmDiagnosticHandler;

    private static readonly IntPtr LlvmDiagnosticHandlerHandle
        = Marshal.GetFunctionPointerForDelegate(LlvmDiagnosticHandlerDelegatePersistRef);

    private static readonly LLVMMCJITCompilerOptions LlvmOptions = LLVMMCJITCompilerOptions.Create();

    private static unsafe LLVMOpaqueContext* NewContext(
        out LLVMOpaqueModule* pModule,
        out LLVMOpaqueExecutionEngine* pEngine,
        out LLVMOpaquePassManager* pModulePM,
        out LLVMOpaquePassManager* pFunctionPM
    )
    {
        pModule = CreateModule(
            $"{nameof(LlvmMcJitContext)}{Interlocked.Increment(ref _modCounter)}",
            out pEngine,
            out pModulePM,
            out pFunctionPM);
        return LLVM.GetModuleContext(pModule);
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate void LlvmDiagnosticHandlerDelegate(LLVMOpaqueDiagnosticInfo* diagInfo, IntPtr userData);

    private static unsafe void LlvmDiagnosticHandler(LLVMOpaqueDiagnosticInfo* diagInfo, IntPtr v)
    {
        var severity = LLVM.GetDiagInfoSeverity(diagInfo);
        var description = GetStringFromLlvmUtf8Error(LLVM.GetDiagInfoDescription(diagInfo), true, false);
        var msg = $"Module {v}: {severity}: {description}";
        switch (severity)
        {
            case >= LLVMDiagnosticSeverity.LLVMDSWarning:
                Trace.WriteLine(msg);
                break;
            case LLVMDiagnosticSeverity.LLVMDSError: throw new(description);
        }

    }

    private static unsafe string GetStringFromLlvmUtf8Error(sbyte* p, bool dispose, bool error)
    {
        if (p == null)
            throw new ArgumentNullException(nameof(p));

        var l = 0;
        while (p[l] != 0)
            ++l;

        var s = new string(p, 0, l, Encoding.UTF8);

        // ReSharper disable once InvertIf
        if (dispose)
        {
            if (error)
                LLVM.DisposeErrorMessage(p);
            else
                LLVM.DisposeMessage(p);
        }

        return s;
    }
}
