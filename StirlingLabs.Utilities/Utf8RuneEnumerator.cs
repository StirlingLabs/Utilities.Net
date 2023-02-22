using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace StirlingLabs.Utilities;

public ref struct Utf8RuneEnumerator {

  public ReadOnlySpan<byte> Remaining;

  public Rune Current;

  public Utf8RuneEnumerator(ReadOnlySpan<byte> buffer) {
    Remaining = buffer;
    Current = new();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Utf8RuneEnumerator GetEnumerator() => this;

  public bool MoveNext() {
    if (Remaining.IsEmpty) {
      Current = new();
      return false;
    }

    if (Rune.DecodeFromUtf8(Remaining, out Current, out var bytesConsumed) == OperationStatus.InvalidData) {
      Remaining = new();
      throw new InvalidOperationException("Invalid data encountered in UTF8 string.");
    }

    if (bytesConsumed == 0) {
      Remaining = new();
      return false;
    }

    Remaining = Remaining.Slice(bytesConsumed);
    return true;
  }

}