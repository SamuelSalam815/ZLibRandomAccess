namespace ArchiveViewerBackend.TextSearching;

public record struct SearchResult(long ByteOffset, string MatchText);
