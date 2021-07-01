using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities
{
    [PublicAPI]
    public sealed class ShakeStream : Stream
    {
        private Sha3.Context _sha3;

        private bool _finishedInput;

        public ShakeStream(int bits)
            => _sha3.Init(bits / 8);

        public void FinishedInput()
        {
            _sha3.ShakeExtensibleOutputFormatMode();
            _finishedInput = true;
        }

        public override int Read(Span<byte> buffer)
        {
            if (!_finishedInput)
                throw new InvalidOperationException("FinishedInput should be invoked first.");

            _sha3.ShakeOut(buffer);
            return buffer.Length;
        }

        public nuint Read(BigSpan<byte> buffer)
        {
            if (!_finishedInput)
                throw new InvalidOperationException("FinishedInput should be invoked first.");

            _sha3.ShakeOut(buffer);
            return buffer.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Read(byte[] buffer, int offset, int count)
            => checked((int)(uint)Read(new(buffer, checked((uint)offset), checked((uint)count))));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadByte()
        {
            byte x = 0;
            Read(MemoryMarshal.CreateSpan(ref x, 1));
            return x;
        }

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (_finishedInput)
                throw new InvalidOperationException("FinishedInput was already invoked.");

            _sha3.Update(buffer);
        }

        public void Write(ReadOnlyBigSpan<byte> buffer)
        {
            if (_finishedInput)
                throw new InvalidOperationException("FinishedInput was already invoked.");

            _sha3.Update(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(byte[] buffer, int offset, int count)
            => Write(new(buffer, checked((uint)offset), checked((uint)count)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteByte(byte x)
            => Write(MemoryMarshal.CreateReadOnlySpan(ref x, 1));

        public override bool CanRead => _finishedInput;

        public override bool CanWrite => !_finishedInput;

        public override bool CanSeek => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Flush() { }
    }
}
