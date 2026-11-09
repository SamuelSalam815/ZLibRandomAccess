namespace ZLibWrapper;

/// <param name="LineOffset">
/// Number of line feeds before this point. Points are assumed to fall mid-line:
/// start from the previous point (or the start of data) to read the whole line.
/// </param>
public record RecoveryPointOffset(
    long OffsetInUncompressedStream,
    long OffsetInCompressedStream,
    long LineOffset);