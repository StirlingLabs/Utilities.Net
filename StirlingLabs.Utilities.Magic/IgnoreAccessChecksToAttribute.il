.class public auto ansi beforefieldinit
  System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute
    extends [RUNTIME]System.Attribute
{
  .custom instance void [RUNTIME]System.AttributeUsageAttribute::.ctor(valuetype [RUNTIME]System.AttributeTargets)
    = (
      01 00 01 00 00 00 01 00 54 02 0d 41 6c 6c 6f 77 // ........T..Allow
      4d 75 6c 74 69 70 6c 65 01                      // Multiple.
    )
    // int32(1) // 0x00000001
    // property bool 'AllowMultiple' = bool(true)

  .field private initonly string '<AssemblyName>k__BackingField'
    .custom instance void [RUNTIME]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor()
      = (01 00 00 00 )

  .method public hidebysig specialname rtspecialname instance void
    .ctor(
      string assemblyName
    ) cil managed
  {
    .maxstack 8

    IL_0000: ldarg.0      // this
    IL_0001: call         instance void [RUNTIME]System.Attribute::.ctor()

    IL_0006: ldarg.0      // this
    IL_0007: ldarg.1      // assemblyName
    IL_0008: stfld        string System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute::'<AssemblyName>k__BackingField'

    IL_000d: ret

  }

  .method public hidebysig specialname instance string
    get_AssemblyName() cil managed
  {
    .custom instance void [RUNTIME]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor()
      = (01 00 00 00 )
    .maxstack 8

    IL_0000: ldarg.0      // this
    IL_0001: ldfld        string System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute::'<AssemblyName>k__BackingField'
    IL_0006: ret

  }

  .property instance string AssemblyName()
  {
    .get instance string System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute::get_AssemblyName()
  }
}
