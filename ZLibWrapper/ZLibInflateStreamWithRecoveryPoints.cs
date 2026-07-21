using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using ZLibBindings;
using ZLibBindings.Constants;
using ZLibBindings.State;
using ZLibWrapper.Extensions;
using ZLibWrapper.Logic;

namespace ZLibWrapper;

internal unsafe class ZLibInflateStreamWithRecoveryPoints : ZLibInflateStream
{
    private readonly ZWindowBits _windowBits;

    public ZLibInflateStreamWithRecoveryPoints(
        Stream compressedStream,
        bool leaveOpen = false,
        ZWindowBits windowBits = ZWindowBits.Default) : base(compressedStream, leaveOpen, windowBits)
    {
        _windowBits = windowBits;
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
            return Read(outputPtr, count, ZFlushValue.Z_BLOCK);
        }
    }

    protected override bool HandleZLibReadAction(ZLibPumpAction nextAction)
    {
        var isFinalBlockProcessed = ZLibStream->_zDataType.HasFlag(ZDataType.Z_IS_FINAL_BLOCK) &&
                            ZLibStream->_zDataType.HasFlag(ZDataType.Z_END_OF_BLOCK);
        if (nextAction is ZLibPumpAction.Continue && isFinalBlockProcessed)
        {
            // The only data left after processing the end of the final block should be a check sum.
            // Skip this check sum because if we skipped some data by jumping to a recovery point,
            // the checksum for the partial data read and the checksum for the entire file will not be the same.
            ZLibStream->avail_in = 0;
            CompressedStream.Seek(4, SeekOrigin.Current);
        }

        return base.HandleZLibReadAction(nextAction);
    }

    private void ReadHeader()
    {
        CompressedStream.Seek(0, SeekOrigin.Begin);
        byte placeHolder;
        ZLibStream->next_out = &placeHolder;
        ZLibStream->avail_out = 1;
        ZLibStream->next_in = InputBuffer;
        ZLibStream->avail_in = 0;
        while (true)
        {
            var returnCode = ZLibLowLevelBindings.inflate(ZLibStream, ZFlushValue.Z_BLOCK)
                .GuardAgainstFatalErrors(ZLibStream);
            var nextAction = ZLibPumpLogic.GetNextAction(ZLibStream, returnCode);

            switch (nextAction)
            {
                case ZLibPumpAction.RequestMoreInputSpace:
                    var numBytesInInputBuffer = CompressedStream.Read(InputBufferSpan);
                    if (numBytesInInputBuffer == 0)
                    {
                        throw new InvalidOperationException("Reached end of underlying stream before completing the header!");
                    }

                    ZLibStream->next_in = InputBuffer;
                    ZLibStream->avail_in = (uint)numBytesInInputBuffer;
                    break;
                case ZLibPumpAction.RequestMoreOutputSpace:
                    throw new UnreachableException(
                        "It was assumed output cannot be produced while reading a header. This assumption is wrong. This indicates a logical error!");
                case ZLibPumpAction.CompleteStream:
                case ZLibPumpAction.Continue:
                    ZLibStream->avail_in = 0;
                    return;
                case ZLibPumpAction.FailToDecide:
                default:
                    throw new UnreachableException($"Enum value ({nextAction}) was not explicitly handled! This indicates a logical error!");
            }
        }
    }

    private void ResetZLibStream()
    {
        ZLibLowLevelBindings.inflateEnd(ZLibStream).GuardAgainstFatalErrors(ZLibStream);
        *ZLibStream = new z_stream_s
        {
            zalloc = null,
            zfree = null,
            opaque = null,
        };
        ZLibLowLevelBindings.inflateInit2(ZLibStream, _windowBits).GuardAgainstFatalErrors(ZLibStream);
    }

    public void JumpTo(long byteOffset)
    {
        ResetZLibStream();
        var jumpLogic = RecoveryPointNavigationLogic.CreateFrom(_windowBits);
        while (true)
        {
            jumpLogic = jumpLogic.GetJumpAction(out var jumpAction);

            switch (jumpAction)
            {
                case RecoveryPointNavigationAction.JumpToRecoveryPoint:
                    CompressedStream.Seek(byteOffset, SeekOrigin.Begin);
                    return;
                case RecoveryPointNavigationAction.ReadHeader:
                    ReadHeader();
                    break;
                default:
                    throw new UnreachableException($"Enum value ({jumpAction}) was not explicitly handled! This indicates a logical error!");
            }
        }
    }
}
