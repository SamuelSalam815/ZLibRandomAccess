namespace ArchiveViewerBackend.CircularArray;

/// <param name="OffsetInBackingArray">Where in the backing array to write.</param>
/// <param name="Count">How many items to write.</param>
/// <param name="NewState">The state once this write is done.</param>
/// <param name="SourceOffset">Index in the caller's block of the first item to write.</param>
public sealed record CircularArrayWrite(
    int OffsetInBackingArray,
    int Count,
    CircularArrayState NewState,
    int SourceOffset = 0);
