using System.Text;
using System.Text.RegularExpressions;
using ArchiveViewerBackend.LineSearch;

namespace ArchiveViewerBackend;

/// <summary>
/// This search strategy simply scans the given text reader, line by line, for the provided search pattern
/// </summary>
public class ScanTextSearcher(Stream stream, Encoding encoding, bool leaveOpen = false) : ITextSearcher, IDisposable
{

    // todo: we need a specialized text reader stream that exposes the number of bytes read
    //  from the underlying stream, because currently readline does not let us know whether it is
    //   \r, \n\r, or \n is the line separator - so we cannot know the true byte offset we have read into the stream
    //    also because of buffering we cannot look at the position of the underlying stream as an indicator - since more than one line may get buffered.
    private readonly TextReader _reader = new StreamReader(stream, encoding, leaveOpen: leaveOpen);
    private bool _isDisposed;
    private int _numBytesRead;
    private LineSearchLogic? _currentLineSearchLogic;

    public ScanTextSearcher(Stream stream, bool leaveOpen = false) : this(stream, Encoding.UTF8, leaveOpen)
    {
    }

    public SearchResult? FindNext(Regex searchPattern)
    {
        if (_currentLineSearchLogic is null)
        {
            var nextLine = _reader.ReadLine();
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
            if (match is null)
            {
                _numBytesRead += _currentLineSearchLogic.EncodedString.String.Length;
                    // todo code duplication
                var nextLine = _reader.ReadLine();
                if (nextLine is null)
                {
                    return null;
                }

                _currentLineSearchLogic = new LineSearchLogic(new EncodedStringBuilder(nextLine, encoding), searchPattern);

            }
            else
            {
                return new SearchResult(_numBytesRead + match.Value.ByteOffset, match.Value.MatchText);
            }

        }

        return null;
    }

    public IEnumerable<SearchResult> FindAll(Regex searchPattern)
    {
        throw new NotImplementedException();
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
