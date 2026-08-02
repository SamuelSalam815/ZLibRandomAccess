using System.Text.RegularExpressions;

namespace ArchiveViewerBackend;

public interface ITextSearcher
{
    public SearchResult? FindNext(Regex searchPattern);
    public IEnumerable<SearchResult> FindAll(Regex searchPattern);
}

public static class ITextSearcherExtensions
{
    public static SearchResult? FindNext<T>(this T searcher,string searchPattern) where T : ITextSearcher
    {
        return searcher.FindNext(new Regex(searchPattern));
    }

    public static IEnumerable<SearchResult> FindAll<T>(this T searcher, string searchPattern) where T : ITextSearcher
    {
        return searcher.FindAll(new Regex(searchPattern));
    }
}
