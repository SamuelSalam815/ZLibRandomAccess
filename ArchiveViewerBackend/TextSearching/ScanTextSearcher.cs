using System.Text;
using System.Text.RegularExpressions;
using ArchiveViewerBackend.LineSearch;

namespace ArchiveViewerBackend.TextSearching;

/// <summary>
/// This search strategy simply scans the given text reader, line by line, for the provided search pattern
/// </summary>
public class ScanTextSearcher(Stream stream, Encoding encoding, bool leaveOpen = false) : ITextSearcher
{
    private readonly StreamReaderWithByteCount _reader = new(stream, encoding, leaveOpen: leaveOpen);
    private bool _isDisposed;
    private LineSearchLogic? _currentLineSearchLogic;

    private int _currentLineByteOffset;
    private int _nextLineByteOffset;

    public int NumberOfBytesRead => _reader.NumberOfBytesRead;

    public SearchResult? FindNext(Regex searchPattern)
    {
        if (_currentLineSearchLogic is null)
        {
            _currentLineByteOffset = _nextLineByteOffset;
            var nextLine = _reader.ReadLine();
            _nextLineByteOffset = _reader.NumberOfBytesRead;
            if (nextLine is null)
            {
                return null;
            }

            _currentLineSearchLogic = new LineSearchLogic(new EncodedStringBuilder(nextLine, encoding), searchPattern);
        }

        _currentLineSearchLogic = _currentLineSearchLogic.SetSearchPattern(searchPattern);
        LineSearchResult? match = null;
        while (match is null)
        {
            _currentLineSearchLogic = _currentLineSearchLogic.NextMatch(out match);
            if (match is {} found)
            {
                return new SearchResult(_currentLineByteOffset + found.ByteOffset, found.MatchText);
            }

            // todo code duplication
            _currentLineByteOffset = _nextLineByteOffset;
            var nextLine = _reader.ReadLine();
            _nextLineByteOffset = _reader.NumberOfBytesRead;
            if (nextLine is null)
            {
                return null;
            }

            _currentLineSearchLogic = new LineSearchLogic(new EncodedStringBuilder(nextLine, encoding), searchPattern);
        }

        return null;
    }

    public IEnumerable<SearchResult> FindAll(Regex searchPattern)
    {
        while (FindNext(searchPattern) is { } match)
        {
            yield return match;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || _isDisposed)
        {
            return;
        }

        _reader.Dispose();
        _isDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
