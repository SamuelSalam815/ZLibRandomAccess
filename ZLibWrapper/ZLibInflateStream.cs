using System.Diagnostics;
using System.Runtime.InteropServices;
using ZLibBindings;
using ZLibBindings.Constants;
using ZLibBindings.State;
using ZLibWrapper.Extensions;
using ZLibWrapper.Logic;

namespace ZLibWrapper;

internal unsafe class ZLibInflateStream : Stream
{
    private readonly z_stream_s* _zLibStream;
    private readonly Stream _compressedStream;
    private readonly bool _leaveOpen;
    private readonly byte* _inputBuffer;
    private const int InputBufferSize = 1024;
    private int _numBytesInInputBuffer;
    private bool _isDisposed;


    private Span<byte> InputBufferSpan => new(_inputBuffer, InputBufferSize);

    public ZLibInflateStream(Stream compressedStream, bool leaveOpen = false, ZWindowBits windowBits = ZWindowBits.Default)
    {
        _compressedStream = compressedStream;
        _leaveOpen = leaveOpen;
        _zLibStream = (z_stream_s*)Marshal.AllocHGlobal(sizeof(z_stream_s));
        *_zLibStream = new z_stream_s
        {
            zfree = null,
            zalloc = null,
            opaque = null
        };
        ZLibLowLevelBindings
            .inflateInit2(_zLibStream, windowBits)
            .GuardAgainstFatalErrors(_zLibStream);
        _inputBuffer = (byte*)Marshal.AllocHGlobal(InputBufferSize);
        _numBytesInInputBuffer = 0;
    }

    public override void Flush()
    {
        throw new NotSupportedException("Flush is not supported!");
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (offset + count > buffer.Length)
        {
            throw new ArgumentException(
                $"Not enough space in buffer after offset={offset} to read the maximum count={count} of bytes");
        }
        var outputSpan = new Span<byte>(buffer, offset, count);
        fixed (byte* outputPtr = outputSpan)
        {
            return Read(outputPtr, count);
        }
    }

    private int Read(byte* outputBufferPointer, int outputBufferSize)
    {
        checked
        {
            _zLibStream->next_out = outputBufferPointer;
            _zLibStream->avail_out = (uint)outputBufferSize;
            while (true)
            {
                var zNoFlush = ZFlushValue.Z_NO_FLUSH;
                var returnCode = ZLibLowLevelBindings.inflate(_zLibStream, zNoFlush)
                    .GuardAgainstFatalErrors(_zLibStream);
                var nextAction = ZLibPumpLogic.GetNextAction(_zLibStream, returnCode);

                switch (nextAction)
                {
                    case ZLibPumpAction.RequestMoreInputSpace:
                        _numBytesInInputBuffer = _compressedStream.Read(InputBufferSpan);
                        if (_numBytesInInputBuffer == 0)
                        {
                            return GetNumBytesReadFromZlib(outputBufferSize);
                        }

                        _zLibStream->next_in = _inputBuffer;
                        _zLibStream->avail_in = (uint)_numBytesInInputBuffer;
                        break;
                    case ZLibPumpAction.CompleteStream:
                    case ZLibPumpAction.RequestMoreOutputSpace:
                        return GetNumBytesReadFromZlib(outputBufferSize);
                    case ZLibPumpAction.Continue:
                        break;
                    case ZLibPumpAction.FailToDecide:
                    default:
                        throw new UnreachableException($"Enum value ({nextAction}) was not explicitly handled! This indicates a logical error!");
                }
            }
        }
    }

    private int GetNumBytesReadFromZlib(int outputBufferSize)
    {
        checked
        {
            return outputBufferSize - (int)_zLibStream->avail_out;
        }
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException("Seeking in this stream is not supported!");
    }

    public override void SetLength(long value)
    {
        throw new InvalidOperationException("Setting stream length of a read-only stream is not a valid operation!");
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new InvalidOperationException("Cannot write to a read-only stream!");
    }

    public override bool CanRead => _compressedStream.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => _compressedStream.Length;

    public override long Position
    {
        get => _compressedStream.Position;
        set => throw new NotSupportedException("Setting stream position is not supported!");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            if (!_leaveOpen)
            {
                _compressedStream.Dispose();
            }

            ZLibLowLevelBindings
                .inflateEnd(_zLibStream)
                .GuardAgainstFatalErrors(_zLibStream);
            Marshal.FreeHGlobal((IntPtr)_zLibStream);
            _isDisposed = true;
        }
        base.Dispose(disposing);
    }
}
