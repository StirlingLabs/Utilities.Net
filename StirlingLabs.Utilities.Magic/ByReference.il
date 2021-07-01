.class public sealed sequential ansi beforefieldinit
  StirlingLabs.Utilities.ByReference`1<T>
    extends [System.Runtime]System.ValueType
{
  .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor()
    = (01 00 00 00 )
  .custom instance void [System.Runtime]System.ObsoleteAttribute::.ctor(string, bool)
    = (
      01 00 52 54 79 70 65 73 20 77 69 74 68 20 65 6d // ..RTypes with em
      62 65 64 64 65 64 20 72 65 66 65 72 65 6e 63 65 // bedded reference
      73 20 61 72 65 20 6e 6f 74 20 73 75 70 70 6f 72 // s are not suppor
      74 65 64 20 69 6e 20 74 68 69 73 20 76 65 72 73 // ted in this vers
      69 6f 6e 20 6f 66 20 79 6f 75 72 20 63 6f 6d 70 // ion of your comp
      69 6c 65 72 2e 01 00 00                         // iler....
    )
  .custom instance void [System.Runtime]System.Runtime.CompilerServices.IsReadOnlyAttribute::.ctor()
    = (01 00 00 00 )
  .custom instance void [System.Runtime]System.Runtime.CompilerServices.IsByRefLikeAttribute::.ctor()
    = (01 00 00 00 )
    // string('Types with embedded references are not supported in this version of your compiler.')
    // bool(true)

  .field private initonly !0* _value
  //.field private initonly valuetype [System.Runtime]System.ByReference`1<!0> _value
  //.field private initonly !0& _value // ByRef field not supported yet

  .method public hidebysig specialname rtspecialname instance void
    .ctor(
      !0& 'value'
    ) cil managed
  {
    .custom instance void System.Runtime.CompilerServices.IntrinsicAttribute::.ctor()
      = (01 00 00 00 )
    .maxstack 8
        ldarg.0
        ldarg.1
        //conv.i
        stfld !0* valuetype StirlingLabs.Utilities.ByReference`1<!0>::_value
        ret
  }

  .method public hidebysig specialname instance !0&
    get_Value() cil managed
  {
    .custom instance void System.Runtime.CompilerServices.IntrinsicAttribute::.ctor()
      = (01 00 00 00 )
    .maxstack 8
        ldarg.0
        ldfld !0* valuetype StirlingLabs.Utilities.ByReference`1<!0>::_value
        ret
  }

  .property instance !0& Value()
  {
    .get instance !0& StirlingLabs.Utilities.ByReference`1::get_Value()
  }
}
