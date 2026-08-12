using System.Text.RegularExpressions;

namespace ArchiveViewerBackend;

public interface ITextSearcher
{
    public SearchResult? FindNext(Regex searchPattern);

    public IEnumerable<SearchResult> FindAll(Regex searchPattern);
}
