namespace ZLibWrapper;

/// <param name="LineOffset">Number of line feeds before this point.</param>
/// <param name="IsAtLineStart">
/// False when the point is mid-line: start from the previous point to read the whole line.
/// </param>
public record RecoveryPointOffset(
    long OffsetInUncompressedStream,
    long OffsetInCompressedStream,
    long LineOffset,
    bool IsAtLineStart);