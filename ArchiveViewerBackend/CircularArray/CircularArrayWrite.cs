namespace ArchiveViewerBackend.CircularArray;

public sealed record CircularArrayWrite(int OffsetInBackingArray, int Count, CircularArrayState NewState);
