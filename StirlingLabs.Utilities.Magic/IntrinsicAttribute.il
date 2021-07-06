.class public sealed auto ansi beforefieldinit
  System.Runtime.CompilerServices.IntrinsicAttribute
    extends [RUNTIME]System.Attribute
{
  .custom instance void [RUNTIME]System.AttributeUsageAttribute::.ctor(
    valuetype [RUNTIME]System.AttributeTargets
  ) = (
      01 00 6c 01 00 00 01 00 54 02 09 49 6e 68 65 72 // ..l.....T..Inher
      69 74 65 64 00                                  // ited.
  )
    // int32(364) // 0x0000016c
    // property bool 'Inherited' = bool(false)

  .method public hidebysig specialname rtspecialname instance void
    .ctor() cil managed
  {
    .maxstack 8
        ldarg.0
        call instance void [RUNTIME]System.Attribute::.ctor()
        ret
  }
}
