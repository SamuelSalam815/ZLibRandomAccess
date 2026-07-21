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
    private const int InputBufferSize = 1024;

    protected readonly byte* InputBuffer;
    protected readonly z_stream_s* ZLibStream;
    protected readonly Stream CompressedStream;

    private readonly bool _leaveOpen;
    private bool _isDisposed;


    protected Span<byte> InputBufferSpan => new(InputBuffer, InputBufferSize);

    public ZLibInflateStream(Stream compressedStream, bool leaveOpen = false, ZWindowBits windowBits = ZWindowBits.Default)
    {
        CompressedStream = compressedStream;
        _leaveOpen = leaveOpen;
        ZLibStream = (z_stream_s*)Marshal.AllocHGlobal(sizeof(z_stream_s));
        *ZLibStream = new z_stream_s
        {
            zfree = null,
            zalloc = null,
            opaque = null
        };
        ZLibLowLevelBindings
            .inflateInit2(ZLibStream, windowBits)
            .GuardAgainstFatalErrors(ZLibStream);
        InputBuffer = (byte*)Marshal.AllocHGlobal(InputBufferSize);
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

        fixed (byte* outputPtr = new Span<byte>(buffer, offset, count))
        {
            return Read(outputPtr, count, ZFlushValue.Z_NO_FLUSH);
        }
    }

    protected int Read(byte* outputBufferPointer, int outputBufferSize, ZFlushValue flushValue)
    {
        checked
        {
            ZLibStream->next_out = outputBufferPointer;
            ZLibStream->avail_out = (uint)outputBufferSize;
            while (true)
            {
                var returnCode = ZLibLowLevelBindings.inflate(ZLibStream, flushValue)
                    .GuardAgainstFatalErrors(ZLibStream);
                var nextAction = ZLibPumpLogic.GetNextAction(ZLibStream, returnCode);

                if (HandleZLibReadAction(nextAction))
                {
                    return GetNumBytesReadFromZlib(outputBufferSize);
                }
            }
        }
    }

    /// <summary>
    /// Handles various values of the <see cref="ZLibPumpAction"/>
    /// </summary>
    /// <param name="nextAction"></param>
    /// <returns>True if the read operation has completed.</returns>
    /// <exception cref="UnreachableException"></exception>
    protected virtual bool HandleZLibReadAction(ZLibPumpAction nextAction)
    {
        switch (nextAction)
        {
            case ZLibPumpAction.RequestMoreInputSpace:
                var numBytesInInputBuffer = CompressedStream.Read(InputBufferSpan);
                if (numBytesInInputBuffer == 0)
                {
                    return true;
                }

                ZLibStream->next_in = InputBuffer;
                ZLibStream->avail_in = (uint)numBytesInInputBuffer;
                break;
            case ZLibPumpAction.CompleteStream:
            case ZLibPumpAction.RequestMoreOutputSpace:
                return true;
            case ZLibPumpAction.Continue:
                break;
            case ZLibPumpAction.FailToDecide:
            default:
                throw new UnreachableException($"Enum value ({nextAction}) was not explicitly handled! This indicates a logical error!");
        }

        return false;
    }

    private int GetNumBytesReadFromZlib(int outputBufferSize)
    {
        checked
        {
            return outputBufferSize - (int)ZLibStream->avail_out;
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

    public override bool CanRead => CompressedStream.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => CompressedStream.Length;

    public override long Position
    {
        get => CompressedStream.Position;
        set => throw new NotSupportedException("Setting stream position is not supported!");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            if (!_leaveOpen)
            {
                CompressedStream.Dispose();
            }

            ZLibLowLevelBindings
                .inflateEnd(ZLibStream)
                .GuardAgainstFatalErrors(ZLibStream);
            Marshal.FreeHGlobal((IntPtr)ZLibStream);
            _isDisposed = true;
        }
        base.Dispose(disposing);
    }
}
