using ZLibBindings.Constants;

namespace ZLibWrapper;

/// <summary>
/// A <see cref="ZLibDeflateStream"/> where <see cref="ZLibDeflateStreamWithRecoveryPoints.Flush"/> creates a
/// a point of recovery in the compressed stream.
/// </summary>
/// <remarks>
/// This kind of flush severely degrades the compression ratio if used frequently, so it should only be used when needed
/// and never with a stream that will automatically call flush.
/// </remarks>
internal class ZLibDeflateStreamWithRecoveryPoints : ZLibDeflateStream
{
    public ZLibDeflateStreamWithRecoveryPoints(Stream stream, bool leaveOpen = false, ZLibDeflateConfiguration? configuration = null) : base(stream, leaveOpen, configuration)
    {
    }

    public override void Flush()
    {
        Flush(ZFlushValue.Z_FULL_FLUSH);
    }
}
