#define RUNTIME "System.Runtime"
#define PRIVATE "System.Private.CoreLib"

.assembly extern RUNTIME
{
  .publickeytoken = ( b0 3f 5f 7f 11 d5 0a 3a )
  .ver 5:0:0:0
}

.assembly StirlingLabs.Utilities.Magic
{
  .custom instance void [RUNTIME]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void [RUNTIME]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .custom instance void [RUNTIME]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                                   63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.
  .custom instance void [RUNTIME]System.Runtime.CompilerServices.DisablePrivateReflectionAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void [RUNTIME]System.Runtime.Versioning.TargetFrameworkAttribute::.ctor(string) = ( 01 00 18 2E 4E 45 54 43 6F 72 65 41 70 70 2C 56   // ....NETCoreApp,V
                                                                                                              65 72 73 69 6F 6E 3D 76 35 2E 30 01 00 54 0E 14   // ersion=v5.0..T..
                                                                                                              46 72 61 6D 65 77 6F 72 6B 44 69 73 70 6C 61 79   // FrameworkDisplay
                                                                                                              4E 61 6D 65 00 )                                  // Name.
  .custom instance void [RUNTIME]System.Reflection.AssemblyCompanyAttribute::.ctor(string) = ( 01 00 0D 53 74 69 72 6C 69 6E 67 20 4C 61 62 73   // ...Stirling Labs
                                                                                                      00 00 ) 
  .custom instance void [RUNTIME]System.Reflection.AssemblyConfigurationAttribute::.ctor(string) = ( 01 00 07 52 65 6C 65 61 73 65 00 00 )             // ...Release..
  .permissionset reqmin
             = {class 'System.Security.Permissions.SecurityPermissionAttribute, System.Runtime, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' = {property bool 'SkipVerification' = bool(true)}}

  .custom instance void System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute::.ctor(string) = ( 01 00 08 6D 73 63 6F 72 6C 69 62 00 00 )          // ...mscorlib..
  .custom instance void System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute::.ctor(string) = ( 01 00 0E 53 79 73 74 65 6D 2E 52 75 6E 74 69 6D   // ...System.Runtim
                                                                                                          65 00 00 )                                        // e..
  .custom instance void System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute::.ctor(string) = ( 01 00 0B 6E 65 74 73 74 61 6E 64 61 72 64 00 00 ) // ...netstandard..
  .custom instance void System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute::.ctor(string) = ( 01 00 16 53 79 73 74 65 6D 2E 50 72 69 76 61 74   // ...System.Privat
                                                                                                                                        65 2E 43 6F 72 65 4C 69 62 00 00 )                // e.CoreLib..
  .hash algorithm 0x00008004
}

.module StirlingLabs.Utilities.Magic.dll
.custom instance void [RUNTIME]System.Security.UnverifiableCodeAttribute::.ctor() = ( 01 00 00 00 ) 

