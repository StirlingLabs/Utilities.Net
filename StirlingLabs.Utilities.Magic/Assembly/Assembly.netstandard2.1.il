#define RUNTIME "netstandard"
#define PRIVATE "System.Private.CoreLib"

.assembly extern RUNTIME
{
  .publickeytoken = ( cc 7b 13 ff cd 2d dd 51 )
  .ver 2:1:0:0
}

.assembly extern PRIVATE
{
  .publickeytoken = ( 7c ec 85 d7 be a7 79 8e )
  .ver 5:0:0:0
}

.assembly StirlingLabs.Utilities.Magic
{
  .custom instance void [RUNTIME]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void [RUNTIME]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .custom instance void [RUNTIME]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                                   63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.
  .custom instance void [RUNTIME]System.Runtime.CompilerServices.DisablePrivateReflectionAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void [RUNTIME]System.Runtime.Versioning.TargetFrameworkAttribute::.ctor(string) = ( 01 00 19 2E 4E 45 54 53 74 61 6E 64 61 72 64 2C   // ....NETStandard,
                                                                                                           56 65 72 73 69 6F 6E 3D 76 32 2E 31 01 00 54 0E   // Version=v2.1..T.
                                                                                                           14 46 72 61 6D 65 77 6F 72 6B 44 69 73 70 6C 61   // .FrameworkDispla
                                                                                                           79 4E 61 6D 65 00 )                               // yName.
  .custom instance void [RUNTIME]System.Reflection.AssemblyCompanyAttribute::.ctor(string) = ( 01 00 0D 53 74 69 72 6C 69 6E 67 20 4C 61 62 73   // ...Stirling Labs
                                                                                                      00 00 ) 
  .custom instance void [RUNTIME]System.Reflection.AssemblyConfigurationAttribute::.ctor(string) = ( 01 00 07 52 65 6C 65 61 73 65 00 00 )             // ...Release..
  .permissionset reqmin
             = {class 'System.Security.Permissions.SecurityPermissionAttribute, netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' = {property bool 'SkipVerification' = bool(true)}}

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

