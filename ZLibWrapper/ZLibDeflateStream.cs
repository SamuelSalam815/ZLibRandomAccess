using System.Diagnostics;
using System.Runtime.InteropServices;
using ZLibBindings;
using ZLibBindings.Constants;
using ZLibBindings.State;
using ZLibWrapper.Extensions;

namespace ZLibWrapper;

internal unsafe class ZLibDeflateStream : Stream
{
    private const int BufferSize = 1024 * 4;
    private readonly z_stream_s* _zLibStream;
    private readonly Stream _stream;
    private readonly bool _leaveOpen;
    private bool _isDisposed;

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
        ZLibLowLevelBindings.deflateInit(
            _zLibStream,
            ZCompressionLevel.Z_DEFAULT_COMPRESSION).GuardAgainstFatalErrors(_zLibStream);
    }

    public override void Flush()
    {
        Flush(ZFlushValue.Z_SYNC_FLUSH);
    }

    private void Flush(ZFlushValue flushValue)
    {
        var outputBuffer = stackalloc byte[BufferSize];
        ProcessZLibStream(null, 0, outputBuffer, BufferSize, flushValue);
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
        var outputBuffer = stackalloc byte[BufferSize];
        fixed (byte* inputBuffer = &buffer[offset])
        {
            ProcessZLibStream(inputBuffer, count, outputBuffer, BufferSize, ZFlushValue.Z_NO_FLUSH);
        }
    }

    private void ProcessZLibStream(byte* inputBufferPointer, int inputBufferSize, byte* outputBufferPointer, int outputBufferSize, ZFlushValue flushValue)
    {
        _zLibStream->next_in = inputBufferPointer;
        _zLibStream->avail_in = (uint)inputBufferSize;
        _zLibStream->next_out = outputBufferPointer;
        _zLibStream->avail_out = (uint)outputBufferSize;

        while (true)
        {
            var returnCode = ZLibLowLevelBindings
                .deflate(_zLibStream, flushValue)
                .GuardAgainstFatalErrors(_zLibStream);
            var nextAction = ZLibDeflateLogic.GetNextAction(_zLibStream, returnCode, flushValue);
            var outputBytes = new Span<byte>(outputBufferPointer, outputBufferSize - (int)_zLibStream->avail_out);

            switch (nextAction)
            {
                case ZLibWriteAction.CompleteInput:
                    _stream.Write(outputBytes);
                    return;
                case ZLibWriteAction.RequestMoreOutputSpace:
                    _stream.Write(outputBytes);
                    _zLibStream->next_out = outputBufferPointer;
                    _zLibStream->avail_out = (uint)outputBufferSize;
                    break;
                case ZLibWriteAction.Continue:
                    break;
                case ZLibWriteAction.FailToDecide:
                default:
                    throw new UnreachableException($"Enum value ({nextAction}) was not explicitly handled! This indicates a logical error!");
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
        if (!disposing || _isDisposed)
        {
            return;
        }

        Flush(ZFlushValue.Z_FINISH);
        ZLibLowLevelBindings
            .deflateEnd(_zLibStream)
            .GuardAgainstFatalErrors(_zLibStream);
        Marshal.FreeHGlobal((IntPtr)_zLibStream);

        if (!_leaveOpen)
        {
            _stream.Dispose();
        }
        _isDisposed = true;
    }
}
