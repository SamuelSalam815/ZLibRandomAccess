using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ArchiveViewerBackend.TextSearching;

public static class TextSearcherExtensions
{
    public static SearchResult? FindNext<T>(
        this T searcher,
        [StringSyntax(StringSyntaxAttribute.Regex)] string searchPattern) where T : ITextSearcher
    {
        return searcher.FindNext(new Regex(searchPattern));
    }

    public static IEnumerable<SearchResult> FindAll<T>(
        this T searcher,
        [StringSyntax(StringSyntaxAttribute.Regex)] string searchPattern) where T : ITextSearcher
    {
        return searcher.FindAll(new Regex(searchPattern));
    }
}
