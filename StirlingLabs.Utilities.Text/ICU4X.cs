using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities.Text;

[SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline")]
public static partial class ICU4X
{
    [RegexPattern]
    private const string LocaleRegexStr = /*language=regex*/
        """
        ^(?<grandfathered>
            (?:en-GB-oed
            |i-(?:ami|bnn|default|enochian|hak|klingon|lux|mingo|navajo|pwn|t(?:a[oy]|su))
            |sgn-(?:BE-(?:FR|NL)|CH-DE))
            |(?:art-lojban|cel-gaulish|no-(?:bok|nyn)|zh-(?:guoyu|hakka|min(?:-nan)?|xiang)))
        |(?:
            (?<language>
                (?:[A-Za-z]{2,3}(?:-(?<extlang>[A-Za-z]{3}(?:-[A-Za-z]{3}){0,2}))?)
                |[A-Za-z]{4,8})
            (?:-(?<script>[A-Za-z]{4}))?
            (?:-(?<region>[A-Za-z]{2}|[0-9]{3}))?
            (?:-(?<variant>[A-Za-z0-9]{5,8}|[0-9][A-Za-z0-9]{3}))*
            (?:-(?<extension>[0-9A-WY-Za-wy-z](?:-[A-Za-z0-9]{2,8})+))*
        )
        (?:-(?<privateUse>x(?:-[A-Za-z0-9]{1,8})+))?$
        """;

    private const string UcldrGzFileName = "ucldr.postcard.gz";

#if NET7_0_OR_GREATER
    [GeneratedRegex(LocaleRegexStr, RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace)]
    private static partial Regex GetLocaleRegex();
    private static Regex LocaleRegex => GetLocaleRegex();
#else
    private static readonly Regex LocaleRegex = new Regex(LocaleRegexStr,
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace);
#endif

    internal unsafe struct Ptr<T> where T : unmanaged
    {
        public readonly T* Value;

        public Ptr(T* value) => Value = value;

        public static implicit operator T*(Ptr<T> ptr) => ptr.Value;

        public static implicit operator Ptr<T>(T* ptr) => new(ptr);
    }

    private static readonly ConcurrentDictionary<string, nint> Locales = new();

    private static readonly ConcurrentDictionary<string, nint> CollationLocales = new();

    private static readonly ConcurrentDictionary<(string, CollatorOptionsV1), nint> Collators = new();

    /// <seealso href="https://unicode.org/reports/tr35/tr35.html#Unicode_locale_identifier"/>
    internal static unsafe void* GetLocale(string? locale = null)
        => (void*)Locales.GetOrAdd(locale ?? "", static locale => {
            if (locale is "" or "und")
                return (nint)CreateUndefinedLocale();

            var localeLength = locale.Length;
            Span<byte> bytes = stackalloc byte[localeLength + 1];
            //Encoding.ASCII.GetBytes(locale, bytes);
            for (var i = 0; i < localeLength; i++)
                bytes[i] = (byte)locale[i];
            bytes[localeLength] = 0;
            fixed (byte* pBytes = bytes)
            {
                var result = CreateLocaleFromString(pBytes, (nuint)localeLength);
                if (result.IsOk)
                    return (nint)result.Ok;

                throw new ArgumentException(result.Err.ToString(), nameof(locale));
            }
        });

    internal static unsafe void* GetCollator(in CollatorOptionsV1 options, string? locale = null)
        => (void*)Collators.GetOrAdd((locale is null or "und" ? "" : locale, options), CollatorFactory);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static unsafe nint CollatorFactory((string, CollatorOptionsV1) compositeKey)
    {
        var (localeStr, options) = compositeKey;

        var optionsBackup = options;

        var localeMatch = LocaleRegex.Match(localeStr);
        if (localeMatch.Success)
        {

#if !NETSTANDARD2_0
            var localeChars = (ReadOnlySpan<char>)localeStr;
#else
            var localeChars = localeStr;
#endif
            foreach (Capture extCap in localeMatch.Groups["extension"].Captures)
            {
#if !NETSTANDARD2_0
                var extCapChars = localeChars.Slice(extCap.Index, extCap.Length);
#else
                var extCapChars = localeChars.Substring(extCap.Index, extCap.Length);
#endif

                if (extCapChars.StartsWith("u-", StringComparison.Ordinal))
                {
                    var unicodeExt = extCapChars[2..];

                    if (unicodeExt.StartsWith("ks-", StringComparison.Ordinal))
                    {
                        var ksExt = unicodeExt[3..];
                        if (ksExt.StartsWith("level", StringComparison.Ordinal))
                        {
                            var ksLevel = ksExt[5..];
                            if (ksLevel.Length != 1)
                                continue;

                            var ksLevelChar = ksLevel[0];
                            if (ksLevelChar is < '1' or > '4') continue;
                            var ksLevelInt = ksLevelChar - '0';
                            options = options with { Strength = (CollatorStrength)ksLevelInt };
                        }
                        else if (ksExt.Equals("identic", StringComparison.Ordinal))
                            options = options with { Strength = CollatorStrength.Identical };
                        else if (ksExt.Equals("primary", StringComparison.Ordinal))
                            options = options with { Strength = CollatorStrength.Primary };
                        else if (ksExt.Equals("secondary", StringComparison.Ordinal))
                            options = options with { Strength = CollatorStrength.Secondary };
                        else if (ksExt.Equals("tertiary", StringComparison.Ordinal))
                            options = options with { Strength = CollatorStrength.Tertiary };
                        else if (ksExt.Equals("quaternary", StringComparison.Ordinal))
                            options = options with { Strength = CollatorStrength.Quaternary };
                    }
                    else if (unicodeExt.StartsWith("ka-", StringComparison.Ordinal))
                    {
                        var kaExt = unicodeExt[3..];
                        if (kaExt.Equals("noignore", StringComparison.Ordinal))
                            options = options with { AlternateHandling = CollatorAlternateHandling.NonIgnorable };
                        else if (kaExt.Equals("shifted", StringComparison.Ordinal))
                            options = options with { AlternateHandling = CollatorAlternateHandling.Shifted };
                    }
                    else if (unicodeExt.StartsWith("kc-", StringComparison.Ordinal))
                    {
                        var kcExt = unicodeExt[3..];
                        if (kcExt.Equals("true", StringComparison.Ordinal))
                            options = options with { CaseLevel = CollatorCaseLevel.On };
                        else if (kcExt.Equals("false", StringComparison.Ordinal))
                            options = options with { CaseLevel = CollatorCaseLevel.Off };
                    }
                    else if (unicodeExt.StartsWith("kf-", StringComparison.Ordinal))
                    {
                        var kfExt = unicodeExt[3..];
                        if (kfExt.Equals("upper", StringComparison.Ordinal))
                            options = options with { CaseFirst = CollatorCaseFirst.UpperFirst };
                        else if (kfExt.Equals("lower", StringComparison.Ordinal))
                            options = options with { CaseFirst = CollatorCaseFirst.LowerFirst };
                        else if (kfExt.Equals("false", StringComparison.Ordinal))
                            options = options with { CaseFirst = CollatorCaseFirst.Off };
                    }
                    else if (unicodeExt.StartsWith("kb-", StringComparison.Ordinal))
                    {
                        var kbExt = unicodeExt[3..];
                        if (kbExt.Equals("true", StringComparison.Ordinal))
                            options = options with { BackwardSecondLevel = CollatorBackwardSecondLevel.On };
                        else if (kbExt.Equals("false", StringComparison.Ordinal))
                            options = options with { BackwardSecondLevel = CollatorBackwardSecondLevel.Off };
                    }
                    else if (unicodeExt.StartsWith("kn-", StringComparison.Ordinal))
                    {
                        var knExt = unicodeExt[3..];
                        if (knExt.Equals("true", StringComparison.Ordinal))
                            options = options with { Numeric = CollatorNumeric.On };
                        else if (knExt.Equals("false", StringComparison.Ordinal))
                            options = options with { Numeric = CollatorNumeric.Off };
                    }
                    else if (unicodeExt.StartsWith("kv-", StringComparison.Ordinal))
                    {
                        var kvExt = unicodeExt[3..];
                        if (kvExt.Equals("space", StringComparison.Ordinal))
                            options = options with { MaxVariable = CollatorMaxVariable.Space };
                        else if (kvExt.Equals("punct", StringComparison.Ordinal))
                            options = options with { MaxVariable = CollatorMaxVariable.Punctuation };
                        else if (kvExt.Equals("symbol", StringComparison.Ordinal))
                            options = options with { MaxVariable = CollatorMaxVariable.Symbol };
                        else if (kvExt.Equals("currency", StringComparison.Ordinal))
                            options = options with { MaxVariable = CollatorMaxVariable.Currency };
                    }
                }
            }

            var optionsSize = Unsafe.SizeOf<CollatorOptionsV1>();
            var optionsSpan = new ReadOnlySpan<byte>(Unsafe.AsPointer(ref options), optionsSize);
            var optionsBackupSpan = new ReadOnlySpan<byte>(Unsafe.AsPointer(ref optionsBackup), optionsSize);
            if (!optionsSpan.SequenceEqual(optionsBackupSpan))
                return (nint)GetCollator(options, localeStr);
        }

        if (!CollationLocales.TryGetValue(localeStr, out var locale))
            locale = (nint)GetLocale(localeStr);

        if (!UcldrDataProvider.IsOk)
            throw new NotImplementedException("Failed to initialize data provider from ucldr.gz!");
        var provider = UcldrDataProvider.Ok;
        var result = CreateCollator(provider, (void*)locale, options);
        if (result.IsOk)
        {
            var collLocale = CollationLocales.GetOrAdd(localeStr, locale);
            if (collLocale == locale)
                return (nint)result.Ok;

            //DestroyLocale((void*)locale);
            DestroyCollator(result.Ok);
            locale = collLocale;
            var writable = new Writable(stackalloc byte[8]);
            LocaleToString((void*)locale, &writable);
            var collLocaleStr = Encoding.UTF8.GetString(writable.Buffer, (int)writable.Length);
            if (Collators.TryGetValue((collLocaleStr, options), out var coll))
            {
                result.Ok = (void*)coll;
                result.IsOk = true;
            }
            else
                result = CreateCollator(provider, (void*)locale, options);
            Debug.Assert(result.IsOk);
            if (result.IsOk)
                return (nint)result.Ok;
            throw new NotImplementedException("Can't create collator!");
        }
        else
        {
            if (Fallbacker is null)
                throw new NotSupportedException("No locale fallback mechanism is configured.");

            var iterator = FallbackForLocale(Fallbacker, (void*)locale);
            if (iterator is null)
                throw new NotImplementedException("Can't find a valid collator!");
            StepLocaleFallbackIterator(iterator);
            var fallbackLocale = GetLocaleFallback(iterator);
            var writable = new Writable(8);
            LocaleToString(fallbackLocale, &writable);
            var fallbackLocaleStr = Encoding.UTF8.GetString(writable.Buffer, (int)writable.Length);
            if (fallbackLocaleStr is "und")
                fallbackLocaleStr = "";
            writable.Free();
            DestroyLocale(fallbackLocale);
            DestroyLocaleFallbackIterator(iterator);
            var coll = (nint)GetCollator(options, fallbackLocaleStr);
            if (CollationLocales.TryGetValue(fallbackLocaleStr, out var collLocale))
                CollationLocales.TryAdd(localeStr, collLocale);
            return coll;
        }
    }

    private static readonly (nint Pointer, int Length) Ucldr = LoadUcldr();
    private static unsafe (nint Pointer, int Length) LoadUcldr()
    {

        var gzFileInfo = new FileInfo(UcldrGzFileName);

        if (!gzFileInfo.Exists)
            throw new FileNotFoundException(UcldrGzFileName + " missing!");

        using var mmf =
            MemoryMappedFile.CreateFromFile(gzFileInfo.FullName, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
        using var accessor = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
        var gzUcldrLength = gzFileInfo.Length;
        byte* pGzUcldr = null;
        accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref pGzUcldr);
        //var gzUcldr = new ReadOnlySpan<byte>(pGzUcldr, (int)length);
        using var ums = new UnmanagedMemoryStream(pGzUcldr, gzUcldrLength, gzUcldrLength, FileAccess.Read);
        using var gzUcldr = new GZipStream(ums, CompressionMode.Decompress);
        using var ms = new MemoryStream();
        gzUcldr.CopyTo(ms);
        ms.Position = 0;
        var bytesLength = (int)ms.Length;
        var pBytes = (byte*)NativeMemory.NewUnsafe((nuint)bytesLength);
        var bytes = new Span<byte>(pBytes, bytesLength);
        if (ms.TryGetBuffer(out var buffer))
            buffer.AsSpan().CopyTo(bytes);
#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        else
            // ReSharper disable once MustUseReturnValue
            ms.Read(bytes);
#else
        else
        {
            // ReSharper disable once MustUseReturnValue
            using var umsBytes = new UnmanagedMemoryStream(pBytes, bytes.Length, bytes.Length, FileAccess.Write);
            ms.WriteTo(umsBytes);
        }
#endif
        return ((nint)pBytes, bytes.Length);
    }

    internal static unsafe CreateResult UcldrDataProvider;

    private static unsafe void* Fallbacker;

    static unsafe ICU4X()
    {
#if ICU4X_LOGGING
        try
        {
            InitializeLogger();
        }
        catch
        {
            // ignored; debug build with release packages has no logger
        }
#endif

        var blobLen = (nuint)Ucldr.Length;
        UcldrDataProvider = CreateDataProviderFromMemory((byte*)Ucldr.Pointer, blobLen);

        var unconfiguredFallbacker = CreateLocaleFallbacker(UcldrDataProvider.Ok);
        if (!unconfiguredFallbacker.IsOk)
            return;

        var fallbacker = ConfigureFallbacker(unconfiguredFallbacker.Ok, new()
        {
            Priority = FallbackPriority.Language,
            ExtensionKey = default //new("u"u8) 
        });

        if (fallbacker.IsOk)
            Fallbacker = fallbacker.Ok;
    }

    public enum Error
    {
        UnknownError = 0,
        WritableError = 1,
        OutOfBoundsError = 2,
        DataMissingDataKeyError = 256,
        DataMissingVariantError = 257,
        DataMissingLocaleError = 258,
        DataNeedsVariantError = 259,
        DataNeedsLocaleError = 260,
        DataExtraneousLocaleError = 261,
        DataFilteredResourceError = 262,
        DataMismatchedTypeError = 263,
        DataMissingPayloadError = 264,
        DataInvalidStateError = 265,
        DataCustomError = 266,
        DataIoError = 267,
        DataUnavailableBufferFormatError = 268,
        DataMismatchedAnyBufferError = 269,
        LocaleUndefinedSubtagError = 512,
        LocaleParserLanguageError = 513,
        LocaleParserSubtagError = 514,
        LocaleParserExtensionError = 515,
        DataStructValidityError = 768,
        PropertyUnknownScriptIdError = 1024,
        PropertyUnknownGeneralCategoryGroupError = 1025,
        FixedDecimalLimitError = 1280,
        FixedDecimalSyntaxError = 1281,
        PluralsParserError = 1536,
        CalendarParseError = 1792,
        CalendarOverflowError = 1793,
        CalendarUnderflowError = 1794,
        CalendarOutOfRangeError = 1795,
        CalendarUnknownEraError = 1796,
        CalendarUnknownMonthCodeError = 1797,
        CalendarMissingInputError = 1798,
        CalendarUnknownKindError = 1799,
        CalendarMissingError = 1800,
        DateTimePatternError = 2048,
        DateTimeMissingInputFieldError = 2049,
        DateTimeSkeletonError = 2050,
        DateTimeUnsupportedFieldError = 2051,
        DateTimeUnsupportedOptionsError = 2052,
        DateTimeMissingWeekdaySymbolError = 2053,
        DateTimeMissingMonthSymbolError = 2054,
        DateTimeFixedDecimalError = 2055,
        DateTimeMismatchedCalendarError = 2056,
        TinyStrTooLargeError = 2304,
        TinyStrContainsNullError = 2305,
        TinyStrNonAsciiError = 2306,
        TimeZoneOffsetOutOfBoundsError = 2560,
        TimeZoneInvalidOffsetError = 2561,
        TimeZoneMissingInputError = 2562,
        NormalizerFutureExtensionError = 2816,
        NormalizerValidationError = 2817,
    }


    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct CreateResult
    {
        public void* Ok;
        public Error Err => (Error)(int)(nint)Ok;
        private byte _IsOk;

        public bool IsOk
        {
            get => _IsOk != 0;
            set => _IsOk = (byte)(value ? 0 : 1);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct ErrorResult
    {
        public Error Err;
        private byte _IsOk;

        public bool IsOk
        {
            get => _IsOk != 0;
            set => _IsOk = (byte)(value ? 0 : 1);
        }
    }

//[StructLayout(LayoutKind.Sequential)]
    internal struct CollatorOptionsV1
    {
        public CollatorStrength Strength;
        public CollatorAlternateHandling AlternateHandling;
        public CollatorCaseFirst CaseFirst;
        public CollatorMaxVariable MaxVariable;
        public CollatorCaseLevel CaseLevel;
        public CollatorNumeric Numeric;
        public CollatorBackwardSecondLevel BackwardSecondLevel;
    }

    internal enum Ordering
    {
        Less = -1,
        Equal = 0,
        Greater = 1
    }

    internal enum CollatorStrength
    {
        Auto = 0,
        Primary = 1,
        Secondary = 2,
        Tertiary = 3,
        Quaternary = 4,
        Identical = 5
    }

    internal enum CollatorAlternateHandling
    {
        Auto = 0,
        NonIgnorable = 1,
        Shifted = 2
    }

    internal enum CollatorCaseFirst
    {
        Auto = 0,
        Off = 1,
        LowerFirst = 2,
        UpperFirst = 3
    }

    internal enum CollatorMaxVariable
    {
        Space = 0,
        Punctuation = 1,
        Symbol = 2,
        Currency = 3
    }

    internal enum CollatorCaseLevel
    {
        Auto = 0,
        Off = 1,
        On = 2
    }

    internal enum CollatorNumeric
    {
        Auto = 0,
        Off = 1,
        On = 2
    }

    internal enum CollatorBackwardSecondLevel
    {
        Auto = 0,
        Off = 1,
        On = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Writable
    {
        public nint Context;
        public unsafe byte* Buffer;
        public nuint Length;
        public nuint Capacity;

#if NET6_0_OR_GREATER
        public readonly unsafe delegate * unmanaged[Cdecl, SuppressGCTransition]<Writable*, void> Flush;
        public readonly unsafe delegate * unmanaged[Cdecl, SuppressGCTransition]<Writable*, nuint, void> Grow;
#elif NET5_0_OR_GREATER
        public readonly unsafe delegate * unmanaged[Cdecl]<Writable*, void> Flush;
        public readonly unsafe delegate * unmanaged[Cdecl]<Writable*, nuint, void> Grow;
#else
        public readonly nint Flush;
        public readonly nint Grow;
#endif

        public unsafe Writable(byte* ptr, nuint size)
        {
            Context = 0;
            Buffer = ptr;
            Length = 0;
            Capacity = size;
        }

        public unsafe Writable(Span<byte> pinnedData)
        {
            Context = 0;
            Buffer = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(pinnedData));
            Length = 0;
            Capacity = (nuint)pinnedData.Length;
        }

        public unsafe Writable(byte[] pinnableData)
        {
            var h = GCHandle.Alloc(pinnableData, GCHandleType.Pinned);
            Context = GCHandle.ToIntPtr(h);
#if NETSTANDARD
            Buffer = (byte*)Unsafe.AsPointer(ref pinnableData[0]);
#else
            Buffer = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(pinnableData));
#endif
            Length = 0;
            Capacity = (nuint)pinnableData.Length;
        }

        public unsafe Writable() : this(12) { }

        public unsafe Writable(nuint size)
        {
            if (size < 12) size = 12;
            var a = new byte[size];
            var h = GCHandle.Alloc(a, GCHandleType.Pinned);
            Context = GCHandle.ToIntPtr(h);
#if NETSTANDARD
            Buffer = (byte*)Unsafe.AsPointer(ref a[0]);
#else
            Buffer = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(a));
#endif
            Length = 0;
            Capacity = size;
#if NETSTANDARD
            Flush = FlushImplFnPtr;
            Grow = GrowImplFnPtr;
#else
            Flush = &FlushImpl;
            Grow = &GrowImpl;
#endif
        }

#if NET6_0_OR_GREATER
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition) })]
#elif NET5_0_OR_GREATER
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
        private static unsafe void FlushImpl(Writable* writable) { }

#if NET6_0_OR_GREATER
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition) })]
#elif NET5_0_OR_GREATER
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
        private static unsafe void GrowImpl(Writable* writable, nuint size)
        {
            if (size <= writable->Capacity)
                return;

            var a = new byte[size];
            var h = GCHandle.Alloc(a, GCHandleType.Pinned);
            var newCtx = GCHandle.ToIntPtr(h);
            var oldCtx = writable->Context;
            if (oldCtx != 0)
            {
                var oldH = GCHandle.FromIntPtr(oldCtx);
                oldH.Free();
            }
            writable->Context = newCtx;
#if NETSTANDARD
            var newData = (byte*)Unsafe.AsPointer(ref a[0]);
#else
            var newData = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(a));
#endif
            Unsafe.CopyBlockUnaligned(newData, writable->Buffer, (uint)writable->Length);
            NativeMemory.Free(writable->Buffer);
            writable->Buffer = newData;
            writable->Capacity = size;
        }

#if NETSTANDARD
        private static readonly unsafe IntPtr FlushImplFnPtr = Marshal.GetFunctionPointerForDelegate(FlushImpl);
        private static readonly unsafe IntPtr GrowImplFnPtr = Marshal.GetFunctionPointerForDelegate(GrowImpl);
#endif
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct Utf8StringView
    {
        public unsafe byte* Data;
        public nuint Length;


        public unsafe Utf8StringView(byte* data, nuint length)
        {
            Data = data;
            Length = length;
        }
        public unsafe Utf8StringView(ReadOnlySpan<byte> span)
            : this((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)), (nuint)span.Length) { }
    }

    internal enum FallbackPriority
    {
        Language = 0,
        Region = 1,
        Collation = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FallbackConfig
    {
        public FallbackPriority Priority;
        public Utf8StringView ExtensionKey;
    }

    internal static unsafe void Free(ref this Writable writable)
    {
        var oldCtx = writable.Context;
        if (oldCtx == 0)
            return;
        var oldH = GCHandle.FromIntPtr(oldCtx);
        oldH.Free();
    }

    /// <summary>
    /// <c>DiplomatWriteable diplomat_simple_writable(char* buf, size_t buf_size);</c>
    /// </summary>
    [DllImport("icu_capi_cdylib", EntryPoint = "diplomat_simple_writable")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.AssemblyDirectory)]
    internal static extern unsafe Writable CreateDiplomatWritable(byte* buf, nuint bufSize);

    /// <summary>
    /// <c>diplomat_result_box_ICU4XDataProvider_ICU4XError ICU4XDataProvider_create_fs(const char* path_data, size_t path_len);</c>
    /// </summary>
    [DllImport("icu_capi_cdylib", EntryPoint = "ICU4XDataProvider_create_fs")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.AssemblyDirectory)]
    internal static extern unsafe CreateResult CreateDataProviderFromFilePath(byte* pathData, nuint pathLen);

    /// <summary>
    /// <c>diplomat_result_box_ICU4XDataProvider_ICU4XError ICU4XDataProvider_create_from_byte_slice(const uint8_t* blob_data, size_t blob_len);</c>
    /// </summary>
    [DllImport("icu_capi_cdylib", EntryPoint = "ICU4XDataProvider_create_from_byte_slice")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.AssemblyDirectory)]
    internal static extern unsafe CreateResult CreateDataProviderFromMemory(byte* blobData, nuint blobLen);


    /// <summary>
    /// <c>void ICU4XDataProvider_destroy(ICU4XDataProvider* self);</c>
    /// </summary>
    [DllImport("icu_capi_cdylib", EntryPoint = "ICU4XDataProvider_destroy")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.AssemblyDirectory)]
    internal static extern unsafe void DestroyDataProvider(void* self);

    /// <summary>
    /// <c>diplomat_result_box_ICU4XLocale_ICU4XError ICU4XLocale_create_from_string(const char* name_data, size_t name_len);</c>
    /// </summary>
    [DllImport("icu_capi_cdylib", EntryPoint = "ICU4XLocale_create_from_string")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.AssemblyDirectory)]
    internal static extern unsafe CreateResult CreateLocaleFromString(byte* nameData, nuint nameLen);

    /// <summary>
    /// <c>diplomat_result_void_ICU4XError ICU4XLocale_to_string(const ICU4XLocale* self, DiplomatWriteable* write);</c>
    /// </summary>
    [DllImport("icu_capi_cdylib", EntryPoint = "ICU4XLocale_to_string")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.AssemblyDirectory)]
    internal static extern unsafe ErrorResult LocaleToString(void* self, Writable* write);

    /// <summary>
    /// <c>ICU4XLocale* ICU4XLocale_create_und();</c>
    /// </summary>
    [DllImport("icu_capi_cdylib", EntryPoint = "ICU4XLocale_create_und")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.AssemblyDirectory)]
    internal static extern unsafe void* CreateUndefinedLocale();

    /// <summary>
    /// <c>diplomat_result_box_ICU4XCollator_ICU4XError ICU4XCollator_create_v1(const ICU4XDataProvider* provider, const ICU4XLocale* locale, ICU4XCollatorOptionsV1 options);</c>
    /// </summary>
    [DllImport("icu_capi_cdylib", EntryPoint = "ICU4XCollator_create_v1")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.AssemblyDirectory)]
    internal static extern unsafe CreateResult CreateCollator(void* provider, void* locale, CollatorOptionsV1 options);

    /// <summary>
    /// <c>ICU4XOrdering ICU4XCollator_compare(const ICU4XCollator* self, const char* left_data, size_t left_len, const char* right_data, size_t right_len);</c>
    /// </summary>
    [DllImport("icu_capi_cdylib", EntryPoint = "ICU4XCollator_compare")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.AssemblyDirectory)]
    internal static extern unsafe Ordering CompareUtf8(void* self, byte* leftData, nuint leftLen, byte* rightData, nuint rightLen);

    /// <summary>
    /// <c>ICU4XOrdering ICU4XCollator_compare_valid_utf8(const ICU4XCollator* self, const char* left_data, size_t left_len, const char* right_data, size_t right_len);</c>
    /// </summary>
    [DllImport("icu_capi_cdylib", EntryPoint = "ICU4XCollator_compare_valid_utf8")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.AssemblyDirectory)]
    internal static extern unsafe Ordering CompareValidUtf8(void* self, byte* leftData, nuint leftLen, byte* rightData, nuint rightLen);

    /// <summary>
    /// <c>ICU4XOrdering ICU4XCollator_compare_utf16(const ICU4XCollator* self, const uint16_t* left_data, size_t left_len, const uint16_t* right_data, size_t right_len);</c>
    /// </summary>
    [DllImport("icu_capi_cdylib", EntryPoint = "ICU4XCollator_compare_utf16")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.AssemblyDirectory)]
    internal static extern unsafe Ordering CompareUtf16(void* self, char* leftData, nuint leftLen, char* rightData, nuint rightLen);

    /// <summary>
    /// <c>void ICU4XCollator_destroy(ICU4XCollator* self);</c>
    /// </summary>
    [DllImport("icu_capi_cdylib", EntryPoint = "ICU4XCollator_destroy")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.AssemblyDirectory)]
    internal static extern unsafe void DestroyCollator(void* self);
    /// <summary>
    /// <c>void ICU4XLocale_destroy(ICU4XLocale* self);</c>
    /// </summary>
    [DllImport("icu_capi_cdylib", EntryPoint = "ICU4XLocale_destroy")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.AssemblyDirectory)]
    internal static extern unsafe void DestroyLocale(void* self);

#if ICU4X_LOGGING
/// <summary>
/// <c>bool ICU4XLogger_init_simple_logger()</c>
/// </summary>
[DllImport("icu_capi_cdylib", EntryPoint = "ICU4XLogger_init_simple_logger")]
[DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.AssemblyDirectory)]
internal static extern unsafe bool InitializeLogger();
#endif

    /// <summary>
    /// <c>diplomat_result_box_ICU4XLocaleFallbacker_ICU4XError ICU4XLocaleFallbacker_create(const ICU4XDataProvider* provider);</c>
    /// </summary>
    [DllImport("icu_capi_cdylib", EntryPoint = "ICU4XLocaleFallbacker_create")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.AssemblyDirectory)]
    internal static extern unsafe CreateResult CreateLocaleFallbacker(void* provider);

    /// <summary>
    /// <c>diplomat_result_box_ICU4XLocaleFallbackerWithConfig_ICU4XError ICU4XLocaleFallbacker_for_config(const ICU4XLocaleFallbacker* self, ICU4XLocaleFallbackConfig config);</c>
    /// </summary>
    [DllImport("icu_capi_cdylib", EntryPoint = "ICU4XLocaleFallbacker_for_config")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.AssemblyDirectory)]
    internal static extern unsafe CreateResult ConfigureFallbacker(void* self, FallbackConfig config);


    /// <summary>
    /// <c>ICU4XLocaleFallbackIterator* ICU4XLocaleFallbackerWithConfig_fallback_for_locale(const ICU4XLocaleFallbackerWithConfig* self, const ICU4XLocale* locale);</c>
    /// </summary>
    [DllImport("icu_capi_cdylib", EntryPoint = "ICU4XLocaleFallbackerWithConfig_fallback_for_locale")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.AssemblyDirectory)]
    internal static extern unsafe void* FallbackForLocale(void* self, void* locale);

    /// <summary>
    /// <c>ICU4XLocale* ICU4XLocaleFallbackIterator_get(const ICU4XLocaleFallbackIterator* self);</c>
    /// </summary>
    [DllImport("icu_capi_cdylib", EntryPoint = "ICU4XLocaleFallbackIterator_get")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.AssemblyDirectory)]
    internal static extern unsafe void* GetLocaleFallback(void* iterator);

    /// <summary>
    /// <c>void ICU4XLocaleFallbackIterator_step(ICU4XLocaleFallbackIterator* self);</c>
    /// </summary>
    [DllImport("icu_capi_cdylib", EntryPoint = "ICU4XLocaleFallbackIterator_step")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.AssemblyDirectory)]
    internal static extern unsafe void StepLocaleFallbackIterator(void* iterator);

    /// <summary>
    /// <c>void ICU4XLocaleFallbackIterator_destroy(ICU4XLocaleFallbackIterator* self);</c>
    /// </summary>
    [DllImport("icu_capi_cdylib", EntryPoint = "ICU4XLocaleFallbackIterator_destroy")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.AssemblyDirectory)]
    internal static extern unsafe void DestroyLocaleFallbackIterator(void* iterator);

    public static unsafe int Compare(string a, string b, StringComparison mode = StringComparison.CurrentCulture)
    {
        void* collator = null;
        switch (mode)
        {
            case StringComparison.CurrentCulture:
                collator = GetCollator(new()
                {
                    Strength = CollatorStrength.Secondary,
                    Numeric = CollatorNumeric.Auto,
                    AlternateHandling = CollatorAlternateHandling.Auto,
                    BackwardSecondLevel = CollatorBackwardSecondLevel.Auto,
                    CaseFirst = CollatorCaseFirst.Auto,
                    CaseLevel = CollatorCaseLevel.On,
                    MaxVariable = CollatorMaxVariable.Space
                }, CultureInfo.CurrentCulture.IetfLanguageTag);
                break;
            case StringComparison.CurrentCultureIgnoreCase:
                collator = GetCollator(new()
                {
                    Strength = CollatorStrength.Secondary,
                    Numeric = CollatorNumeric.Auto,
                    AlternateHandling = CollatorAlternateHandling.Auto,
                    BackwardSecondLevel = CollatorBackwardSecondLevel.Auto,
                    CaseFirst = CollatorCaseFirst.Auto,
                    CaseLevel = CollatorCaseLevel.Off,
                    MaxVariable = CollatorMaxVariable.Space
                }, CultureInfo.CurrentCulture.IetfLanguageTag);
                break;
            case StringComparison.InvariantCulture:
                collator = GetCollator(new()
                {
                    Strength = CollatorStrength.Secondary,
                    Numeric = CollatorNumeric.Auto,
                    AlternateHandling = CollatorAlternateHandling.Auto,
                    BackwardSecondLevel = CollatorBackwardSecondLevel.Auto,
                    CaseFirst = CollatorCaseFirst.Auto,
                    CaseLevel = CollatorCaseLevel.On,
                    MaxVariable = CollatorMaxVariable.Space
                }, CultureInfo.InvariantCulture.IetfLanguageTag);
                break;
            case StringComparison.InvariantCultureIgnoreCase:
                collator = GetCollator(new()
                {
                    Strength = CollatorStrength.Secondary,
                    Numeric = CollatorNumeric.Auto,
                    AlternateHandling = CollatorAlternateHandling.Auto,
                    BackwardSecondLevel = CollatorBackwardSecondLevel.Auto,
                    CaseFirst = CollatorCaseFirst.Auto,
                    CaseLevel = CollatorCaseLevel.Off,
                    MaxVariable = CollatorMaxVariable.Space
                }, CultureInfo.InvariantCulture.IetfLanguageTag);
                break;
            case StringComparison.Ordinal:
                return string.CompareOrdinal(a, b);
            case StringComparison.OrdinalIgnoreCase:
                return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);

        }
        if (collator is null)
            throw new InvalidOperationException("Can't get collator for current culture!");

        fixed (char* pA = a)
        fixed (char* pB = b)
            return (int)CompareUtf16(collator, pA, (nuint)a.Length, pB, (nuint)b.Length);
    }


    public static unsafe int Compare(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b, StringComparison mode = StringComparison.CurrentCulture)
    {
        void* collator = null;
        switch (mode)
        {
            case StringComparison.CurrentCulture:
                collator = GetCollator(new()
                {
                    Strength = CollatorStrength.Secondary,
                    Numeric = CollatorNumeric.Auto,
                    AlternateHandling = CollatorAlternateHandling.Auto,
                    BackwardSecondLevel = CollatorBackwardSecondLevel.Auto,
                    CaseFirst = CollatorCaseFirst.Auto,
                    CaseLevel = CollatorCaseLevel.On,
                    MaxVariable = CollatorMaxVariable.Space
                }, CultureInfo.CurrentCulture.IetfLanguageTag);
                break;
            case StringComparison.CurrentCultureIgnoreCase:
                collator = GetCollator(new()
                {
                    Strength = CollatorStrength.Secondary,
                    Numeric = CollatorNumeric.Auto,
                    AlternateHandling = CollatorAlternateHandling.Auto,
                    BackwardSecondLevel = CollatorBackwardSecondLevel.Auto,
                    CaseFirst = CollatorCaseFirst.Auto,
                    CaseLevel = CollatorCaseLevel.Off,
                    MaxVariable = CollatorMaxVariable.Space
                }, CultureInfo.CurrentCulture.IetfLanguageTag);
                break;
            case StringComparison.InvariantCulture:
                collator = GetCollator(new()
                {
                    Strength = CollatorStrength.Secondary,
                    Numeric = CollatorNumeric.Auto,
                    AlternateHandling = CollatorAlternateHandling.Auto,
                    BackwardSecondLevel = CollatorBackwardSecondLevel.Auto,
                    CaseFirst = CollatorCaseFirst.Auto,
                    CaseLevel = CollatorCaseLevel.On,
                    MaxVariable = CollatorMaxVariable.Space
                }, CultureInfo.InvariantCulture.IetfLanguageTag);
                break;
            case StringComparison.InvariantCultureIgnoreCase:
                collator = GetCollator(new()
                {
                    Strength = CollatorStrength.Secondary,
                    Numeric = CollatorNumeric.Auto,
                    AlternateHandling = CollatorAlternateHandling.Auto,
                    BackwardSecondLevel = CollatorBackwardSecondLevel.Auto,
                    CaseFirst = CollatorCaseFirst.Auto,
                    CaseLevel = CollatorCaseLevel.Off,
                    MaxVariable = CollatorMaxVariable.Space
                }, CultureInfo.InvariantCulture.IetfLanguageTag);
                break;
            case StringComparison.Ordinal:
                return a.SequenceCompareTo(b);
            case StringComparison.OrdinalIgnoreCase: {
                return CompareOrdinalIgnoreCase(a, b);
            }

        }
        if (collator is null)
            throw new InvalidOperationException("Can't get collator for current culture!");

        fixed (byte* pA = a)
        fixed (byte* pB = b)
            return (int)CompareUtf8(collator, pA, (nuint)a.Length, pB, (nuint)b.Length);
    }
    private static int CompareOrdinalIgnoreCase(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
    {
        var l = Math.Min(a.Length, b.Length);
        ref var aStart = ref Unsafe.AsRef(in a[0]);
        ref var bStart = ref Unsafe.AsRef(in b[0]);
        const int capDiff = 'A' - 'a';
        for (var i = 0; i < l; ++i)
        {
            ref var rA = ref Unsafe.Add(ref aStart, i);
            ref var rB = ref Unsafe.Add(ref bStart, i);

            var chA = rA;
            var chB = rB;

            if (chA == chB)
                continue;

            if (chA is >= (byte)'a' and <= (byte)'z')
                chA = (byte)(chA - capDiff);
            if (chB is >= (byte)'a' and <= (byte)'z')
                chB = (byte)(chB - capDiff);
            if (chA == chB)
                continue;

            return chA > chB ? 1 : -1;
        }

        return 0;
    }

    public static unsafe int Compare(string a, string b, bool caseSensitive, CultureInfo? culture = null)
        => Compare(a, b, culture?.IetfLanguageTag, caseSensitive);

    public static unsafe int Compare(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b, bool caseSensitive, CultureInfo? culture = null)
        => Compare(a, b, culture?.IetfLanguageTag, caseSensitive);

    public static unsafe int Compare(string a, string b, string? locale, bool caseInsensitive = false)
    {
        var collator = GetCollator(new()
        {
            Strength = CollatorStrength.Auto,
            Numeric = CollatorNumeric.Auto,
            AlternateHandling = CollatorAlternateHandling.Auto,
            BackwardSecondLevel = CollatorBackwardSecondLevel.Auto,
            CaseFirst = CollatorCaseFirst.Auto,
            CaseLevel = caseInsensitive ? CollatorCaseLevel.Off : CollatorCaseLevel.On,
            MaxVariable = CollatorMaxVariable.Space
        }, locale);

        if (collator is null)
            throw new InvalidOperationException("Can't get collator for current culture!");

        fixed (char* pA = a)
        fixed (char* pB = b)
            return (int)CompareUtf16(collator, pA, (nuint)a.Length, pB, (nuint)b.Length);
    }

    public static unsafe int Compare(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b, string? locale, bool caseInsensitive = false)
    {
        var collator = GetCollator(new()
        {
            Strength = CollatorStrength.Auto,
            Numeric = CollatorNumeric.Auto,
            AlternateHandling = CollatorAlternateHandling.Auto,
            BackwardSecondLevel = CollatorBackwardSecondLevel.Auto,
            CaseFirst = CollatorCaseFirst.Auto,
            CaseLevel = caseInsensitive ? CollatorCaseLevel.Off : CollatorCaseLevel.On,
            MaxVariable = CollatorMaxVariable.Space
        }, locale);

        if (collator is null)
            throw new InvalidOperationException("Can't get collator for current culture!");

        fixed (byte* pA = a)
        fixed (byte* pB = b)
            return (int)CompareUtf8(collator, pA, (nuint)a.Length, pB, (nuint)b.Length);
    }
}
