using System.Diagnostics;
using System.Runtime.InteropServices;
using ZLibBindings;
using ZLibBindings.Constants;
using ZLibBindings.State;
using ZLibWrapper.Extensions;

namespace ZLibWrapper;

internal unsafe class ZLibDeflateStream : Stream
{
    private readonly z_stream_s* _zLibStream;
    private readonly Stream _stream;
    private readonly bool _leaveOpen;

    public ZLibDeflateStream(Stream stream, bool leaveOpen = false)
    {
        _stream = stream;
        _leaveOpen = leaveOpen;
        _zLibStream = (z_stream_s*)Marshal.AllocHGlobal(sizeof(z_stream_s));
        *_zLibStream = new z_stream_s
        {
            zfree = null,
            zalloc = null,
            opaque = null
        };
        ZLibLowLevelBindings.deflateInit(_zLibStream, ZCompressionLevel.Z_DEFAULT_COMPRESSION).GuardAgainstFatalErrors(_zLibStream);
    }

    public override void Flush()
    {
        throw new NotSupportedException("Flush is not supported!");
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new InvalidOperationException("Reading from a write-only stream is not valid");
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new InvalidOperationException("Seeking in this stream is not a valid operation");
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException("Setting stream length is not supported!");
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        const int bufferSize = 1024 * 4;
        var outputBuffer = stackalloc byte[bufferSize];
        fixed (byte* inputBuffer = &buffer[offset])
        {
            ProcessZLibStream(count, inputBuffer, outputBuffer, bufferSize);
        }
    }

    private void ProcessZLibStream(int count, byte* inputBuffer, byte* outputBuffer, int bufferSize)
    {
        _zLibStream->next_in = inputBuffer;
        _zLibStream->avail_in = (uint)count;
        _zLibStream->next_out = outputBuffer;
        _zLibStream->avail_out = (uint)bufferSize;
        while (true)
        {
            var returnCode = ZLibLowLevelBindings.deflate(_zLibStream, ZFlushValue.Z_NO_FLUSH);
            var numBytesReadyToWrite = (int)(bufferSize - _zLibStream->avail_out);
            var writtenBytes = new ReadOnlySpan<byte>(outputBuffer, numBytesReadyToWrite);
            switch (ZLibDeflateLogic.DecideNextAction(*_zLibStream, returnCode))
            {
                case ZLibDeflateAction.FailedToDecide:
                    throw new ZLibException("Failed to interpret the required action for the given return code in the current state.", returnCode);
                case ZLibDeflateAction.FatalError:
                    throw new ZLibException($"Encountered fatal error '{returnCode.ToString()}' : {(*_zLibStream).GetErrorMessage()}!", returnCode);
                case ZLibDeflateAction.InputBufferConsumed:
                    _stream.Write(writtenBytes);
                    return;
                case ZLibDeflateAction.OutputBufferConsumed:
                    _stream.Write(writtenBytes);
                    _zLibStream->next_out = outputBuffer;
                    _zLibStream->avail_out = (uint)bufferSize;
                    break;
                case ZLibDeflateAction.CallDeflateAgain:
                    break;
                default:
                    throw new UnreachableException("Unknown ZLibDeflateAction! Logical error!");
            }
        }
    }

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => _stream.CanWrite;

    public override long Length => _stream.Length;

    public override long Position
    {
        get => _stream.Position;
        set => throw new NotSupportedException("Setting stream position is not supported!");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_leaveOpen)
            {
                _stream.Dispose();
            }

            ZLibLowLevelBindings
                .deflateEnd(_zLibStream)
                .GuardAgainstFatalErrors(_zLibStream);
            Marshal.FreeHGlobal((IntPtr)_zLibStream);
        }
        base.Dispose(disposing);
    }
}
