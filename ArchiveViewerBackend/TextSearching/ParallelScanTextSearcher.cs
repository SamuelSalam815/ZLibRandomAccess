using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace ArchiveViewerBackend.TextSearching;

public class ParallelScanTextSearcher : ITextSearcher
{
    private readonly ImmutableList<WithByteOffset<ScanTextSearcher>> _textSearchers;

    private readonly Dictionary<SearchResult, int> _recordOfMatches = [];
    private readonly SemaphoreSlim _matchSemaphore = new(1);

    private int _textSearcherIndex;

    private bool _isDisposed;

    public ParallelScanTextSearcher(IEnumerable<WithByteOffset<Stream>> parallelStreams, Encoding encoding)
    {
        _textSearchers = parallelStreams.Select(stream =>
                new ScanTextSearcher(stream.Payload, encoding).WithByteOffset(stream.ByteOffset)
            )
            .ToImmutableList();
    }

    public SearchResult? FindNext(Regex searchPattern)
    {
        while (true)
        {
            if (_textSearcherIndex >= _textSearchers.Count)
            {
                return null;
            }

            if (FindNext(_textSearcherIndex, searchPattern) is { } found)
            {
                return found;
            }

            _textSearcherIndex++;
        }
    }

    private SearchResult? FindNext(int currentSearcherIndex, Regex searchPattern)
    {
        while (true)
        {
            var currentSearcher = _textSearchers[currentSearcherIndex];

            var maybeMatch = currentSearcher.Payload.FindNext(searchPattern);
            if (maybeMatch is not { } found)
            {
                return null;
            }

            found = found with { ByteOffset = found.ByteOffset + currentSearcher.ByteOffset };

            _matchSemaphore.Wait();
            try
            {
                if (!_recordOfMatches.TryGetValue(found, out var otherSearcherIndex))
                {
                    _recordOfMatches.Add(found, _textSearcherIndex);
                    return found;
                }

                if (otherSearcherIndex > currentSearcherIndex)
                {
                    return null;
                }

                _recordOfMatches[found] = _textSearcherIndex;
            }
            finally
            {
                _matchSemaphore.Release();
            }
        }
    }

    private IEnumerable<SearchResult> FindAll(
        int searchIndex,
        Regex searchPattern)
    {
        while (FindNext(searchIndex, searchPattern) is {} found)
        {
            yield return found;
        }
    }

    public IEnumerable<SearchResult> FindAll(Regex searchPattern)
    {
        var result = Enumerable
            .Sequence(_textSearcherIndex, _textSearchers.Count - 1, 1)
            .AsParallel()
            .SelectMany(searchIndex => FindAll(searchIndex, searchPattern))
            .Distinct()
            .OrderBy(result => result.ByteOffset);

        _textSearcherIndex = _textSearchers.Count;

        return result;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || _isDisposed)
        {
            return;
        }

        _matchSemaphore.Dispose();
        foreach (var textSearcher in _textSearchers)
        {
            textSearcher.Payload.Dispose();
        }

        _isDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
