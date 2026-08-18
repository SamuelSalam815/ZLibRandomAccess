using System.Text.RegularExpressions;

namespace ArchiveViewerBackend.TextSearching;

public interface ITextSearcher : IDisposable
{
    public SearchResult? FindNext(Regex searchPattern);

    public IEnumerable<SearchResult> FindAll(Regex searchPattern);
}
