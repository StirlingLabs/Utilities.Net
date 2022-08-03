using System;
using System.Runtime.InteropServices;
using LLVMSharp.Interop;

namespace StirlingLabs.Utilities;

internal static class LlvmAtomicOps<T> where T : unmanaged
{
    private const LLVMAtomicOrdering AtomicOrdering = LLVMAtomicOrdering.LLVMAtomicOrderingAcquireRelease;

#if NET6_0_OR_GREATER
    public static readonly unsafe delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T> Load;
#else
    public static readonly unsafe delegate *unmanaged[Cdecl]<ref T, T> Load;
#endif

#if NET6_0_OR_GREATER
    public static readonly unsafe delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, void> Store;
#else
    public static readonly unsafe delegate *unmanaged[Cdecl]<ref T, T, void> Store;
#endif

#if NET6_0_OR_GREATER
    public static readonly unsafe delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, T> Add;
#else
    public static readonly unsafe delegate *unmanaged[Cdecl]<ref T, T, T> Add;
#endif

#if NET6_0_OR_GREATER
    public static readonly unsafe delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, T> Sub;
#else
    public static readonly unsafe delegate *unmanaged[Cdecl]<ref T, T, T> Sub;
#endif

#if NET6_0_OR_GREATER
    public static readonly unsafe delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, T> And;
#else
    public static readonly unsafe delegate *unmanaged[Cdecl]<ref T, T, T> And;
#endif

#if NET6_0_OR_GREATER
    public static readonly unsafe delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, void> StoreAnd;
#else
    public static readonly unsafe delegate *unmanaged[Cdecl]<ref T, T, void> StoreAnd;
#endif

#if NET6_0_OR_GREATER
    public static readonly unsafe delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, T> Or;
#else
    public static readonly unsafe delegate *unmanaged[Cdecl]<ref T, T, T> Or;
#endif

#if NET6_0_OR_GREATER
    public static readonly unsafe delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, void> StoreOr;
#else
    public static readonly unsafe delegate *unmanaged[Cdecl]<ref T, T, void> StoreOr;
#endif

#if NET6_0_OR_GREATER
    public static readonly unsafe delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, T> Xor;
#else
    public static readonly unsafe delegate *unmanaged[Cdecl]<ref T, T, T> Xor;
#endif

#if NET6_0_OR_GREATER
    public static readonly unsafe delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, void> StoreXor;
#else
    public static readonly unsafe delegate *unmanaged[Cdecl]<ref T, T, void> StoreXor;
#endif

#if NET6_0_OR_GREATER
    public static readonly unsafe delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, T> Xchg;
#else
    public static readonly unsafe delegate *unmanaged[Cdecl]<ref T, T, T> Xchg;
#endif

#if NET6_0_OR_GREATER
    public static readonly unsafe delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, T, T> CmpXchg;
#else
    public static readonly unsafe delegate *unmanaged[Cdecl]<ref T, T, T, T> CmpXchg;
#endif

/*
#if NET6_0_OR_GREATER
    public static readonly unsafe delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, in T, in T, bool> CmpXchgLarge;
#else
    public static readonly unsafe delegate *unmanaged[Cdecl]<ref T, in T, in T, bool> CmpXchgLarge;
#endif*/

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate T LoadDelegate(ref T a);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void StoreDelegate(ref T a, T b);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate T BinaryDelegate(ref T a, T b);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate T CmpXchgDelegate(ref T a, T b, T c);

    /*[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool CmpXchgLargeDelegate(ref T a, ref T b, ref T c);*/

    static unsafe LlvmAtomicOps()
    {

        if (sizeof(T) <= 8)
        {
            Load =
#if NET6_0_OR_GREATER
                (delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T>)
#else
                (delegate *unmanaged[Cdecl]<ref T, T>)
#endif
                LlvmMcJitContext.LlvmCreateFnPtr<LoadDelegate>((builder, funcType, fn) => {
                    LLVM.BuildRet(builder, LLVM.BuildAtomicRMW(builder,
                        LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpOr,
                        LLVM.GetParam(fn, 0),
                        LLVM.ConstInt(LlvmMcJitContext.GetLlvmType(typeof(T)), 0uL, default),
                        LLVMAtomicOrdering.LLVMAtomicOrderingAcquire,
                        0
                    ));
                }, typeof(T).Name + nameof(Load));

            Store =
#if NET6_0_OR_GREATER
                (delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, void>)
#else
                (delegate *unmanaged[Cdecl]<ref T, T, void>)
#endif
                LlvmMcJitContext.LlvmCreateFnPtr<StoreDelegate>((builder, funcType, fn) => {
                    LLVM.BuildAtomicRMW(builder,
                        LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpXchg,
                        LLVM.GetParam(fn, 0),
                        LLVM.GetParam(fn, 1),
                        LLVMAtomicOrdering.LLVMAtomicOrderingRelease,
                        0
                    );
                    LLVM.BuildRetVoid(builder);
                }, typeof(T).Name + nameof(Store));

            Add =
#if NET6_0_OR_GREATER
                (delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, T>)
#else
                (delegate *unmanaged[Cdecl]<ref T, T, T>)
#endif
                LlvmMcJitContext.LlvmCreateFnPtr<BinaryDelegate>((builder, funcType, fn) => {
                    LLVM.BuildRet(builder, LLVM.BuildAtomicRMW(builder,
                        LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpAdd,
                        LLVM.GetParam(fn, 0),
                        LLVM.GetParam(fn, 1),
                        AtomicOrdering,
                        0
                    ));
                }, typeof(T).Name + nameof(Add));

            Sub =
#if NET6_0_OR_GREATER
                (delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, T>)
#else
                (delegate *unmanaged[Cdecl]<ref T, T, T>)
#endif
                LlvmMcJitContext.LlvmCreateFnPtr<BinaryDelegate>((builder, funcType, fn) => {
                    LLVM.BuildRet(builder, LLVM.BuildAtomicRMW(builder,
                        LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpSub,
                        LLVM.GetParam(fn, 0),
                        LLVM.GetParam(fn, 1),
                        AtomicOrdering,
                        0
                    ));
                }, typeof(T).Name + nameof(Sub));

            And =
#if NET6_0_OR_GREATER
                (delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, T>)
#else
                (delegate *unmanaged[Cdecl]<ref T, T, T>)
#endif
                LlvmMcJitContext.LlvmCreateFnPtr<BinaryDelegate>((builder, funcType, fn) => {
                    LLVM.BuildRet(builder, LLVM.BuildAtomicRMW(builder,
                        LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpAnd,
                        LLVM.GetParam(fn, 0),
                        LLVM.GetParam(fn, 1),
                        AtomicOrdering,
                        0
                    ));
                }, typeof(T).Name + nameof(And));

            StoreAnd =
#if NET6_0_OR_GREATER
                (delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, void>)
#else
                (delegate *unmanaged[Cdecl]<ref T, T, void>)
#endif
                LlvmMcJitContext.LlvmCreateFnPtr<StoreDelegate>((builder, funcType, fn) => {
                    LLVM.BuildAtomicRMW(builder,
                        LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpAnd,
                        LLVM.GetParam(fn, 0),
                        LLVM.GetParam(fn, 1),
                        LLVMAtomicOrdering.LLVMAtomicOrderingRelease,
                        0
                    );
                    LLVM.BuildRetVoid(builder);
                }, typeof(T).Name + nameof(StoreAnd));

            Or =
#if NET6_0_OR_GREATER
                (delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, T>)
#else
                (delegate *unmanaged[Cdecl]<ref T, T, T>)
#endif
                LlvmMcJitContext.LlvmCreateFnPtr<BinaryDelegate>((builder, funcType, fn) => {
                    LLVM.BuildRet(builder, LLVM.BuildAtomicRMW(builder,
                        LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpOr,
                        LLVM.GetParam(fn, 0),
                        LLVM.GetParam(fn, 1),
                        AtomicOrdering,
                        0
                    ));
                }, typeof(T).Name + nameof(Or));

            StoreOr =
#if NET6_0_OR_GREATER
                (delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, void>)
#else
                (delegate *unmanaged[Cdecl]<ref T, T, void>)
#endif
                LlvmMcJitContext.LlvmCreateFnPtr<StoreDelegate>((builder, funcType, fn) => {
                    LLVM.BuildAtomicRMW(builder,
                        LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpOr,
                        LLVM.GetParam(fn, 0),
                        LLVM.GetParam(fn, 1),
                        LLVMAtomicOrdering.LLVMAtomicOrderingRelease,
                        0
                    );
                    LLVM.BuildRetVoid(builder);
                }, typeof(T).Name + nameof(StoreOr));

            Xor =
#if NET6_0_OR_GREATER
                (delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, T>)
#else
                (delegate *unmanaged[Cdecl]<ref T, T, T>)
#endif
                LlvmMcJitContext.LlvmCreateFnPtr<BinaryDelegate>((builder, funcType, fn) => {
                    LLVM.BuildRet(builder, LLVM.BuildAtomicRMW(builder,
                        LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpXor,
                        LLVM.GetParam(fn, 0),
                        LLVM.GetParam(fn, 1),
                        AtomicOrdering,
                        0
                    ));
                }, typeof(T).Name + nameof(Xor));

            StoreXor =
#if NET6_0_OR_GREATER
                (delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, void>)
#else
                (delegate *unmanaged[Cdecl]<ref T, T, void>)
#endif
                LlvmMcJitContext.LlvmCreateFnPtr<StoreDelegate>((builder, funcType, fn) => {
                    LLVM.BuildAtomicRMW(builder,
                        LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpXor,
                        LLVM.GetParam(fn, 0),
                        LLVM.GetParam(fn, 1),
                        LLVMAtomicOrdering.LLVMAtomicOrderingRelease,
                        0
                    );
                    LLVM.BuildRetVoid(builder);
                }, typeof(T).Name + nameof(StoreXor));

            Xchg =
#if NET6_0_OR_GREATER
                (delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, T>)
#else
                (delegate *unmanaged[Cdecl]<ref T, T, T>)
#endif
                LlvmMcJitContext.LlvmCreateFnPtr<BinaryDelegate>((builder, funcType, fn) => {
                    LLVM.BuildRet(builder, LLVM.BuildAtomicRMW(builder,
                        LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpXchg,
                        LLVM.GetParam(fn, 0),
                        LLVM.GetParam(fn, 1),
                        AtomicOrdering,
                        0
                    ));
                }, typeof(T).Name + nameof(Xchg));

            CmpXchg =
#if NET6_0_OR_GREATER
                (delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, T, T, T>)
#else
                (delegate *unmanaged[Cdecl]<ref T, T, T, T>)
#endif
                LlvmMcJitContext.LlvmCreateFnPtr<CmpXchgDelegate>((builder, funcType, fn) => {
                    var result = LLVM.BuildAtomicCmpXchg(builder,
                        LLVM.GetParam(fn, 0),
                        LLVM.GetParam(fn, 1),
                        LLVM.GetParam(fn, 2),
                        LLVMAtomicOrdering.LLVMAtomicOrderingSequentiallyConsistent,
                        LLVMAtomicOrdering.LLVMAtomicOrderingSequentiallyConsistent,
                        0
                    );
                    //LLVM.BuildStore(builder, LLVM.BuildExtractValue(builder, result, 1, Utf8String.Create("b").Pointer), LLVM.GetParam(fn, 3));
                    LLVM.BuildRet(builder, LLVM.BuildExtractValue(builder, result, 0, Utf8String.Create("r").Pointer));
                }, typeof(T).Name + nameof(CmpXchg));
        }
        else
        {
            throw new NotImplementedException();
            /*CmpXchgLarge =
#if NET6_0_OR_GREATER
                (delegate *unmanaged[Cdecl, SuppressGCTransition]<ref T, in T, in T, bool>)
#else
                (delegate *unmanaged[Cdecl]<ref T, in T, in T, bool>)
#endif
                LlvmMcJitContext.LlvmCreateFnPtr<CmpXchgLargeDelegate>((builder, funcType, fn) => {
                    var result = LLVM.BuildAtomicCmpXchg(builder,
                        LLVM.GetParam(fn, 0),
                        LLVM.BuildLoad2(builder, LLVM.Int128Type(), LLVM.GetParam(fn, 1), Utf8String.Create("c").Pointer),
                        LLVM.BuildLoad2(builder, LLVM.Int128Type(), LLVM.GetParam(fn, 2), Utf8String.Create("n").Pointer),
                        LLVMAtomicOrdering.LLVMAtomicOrderingAcquireRelease,
                        LLVMAtomicOrdering.LLVMAtomicOrderingAcquireRelease,
                        0
                    );
                    var b = LLVM.BuildExtractValue(builder, result, 1, Utf8String.Create("b").Pointer);
                    LLVM.BuildRet(builder, LLVM.BuildZExt(builder, b, LLVM.Int8Type(), Utf8String.Create("z").Pointer));
                }, typeof(T).Name + nameof(CmpXchgLarge));*/
        }
    }
}
