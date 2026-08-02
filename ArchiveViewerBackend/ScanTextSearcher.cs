using System.Text;
using System.Text.RegularExpressions;

namespace ArchiveViewerBackend;

/// <summary>
/// This search strategy simply scans the given text reader, line by line, for the provided search pattern
/// </summary>
public class ScanTextSearcher(Stream stream, Encoding encoding, bool leaveOpen = false) : ITextSearcher, IDisposable
{
    private readonly TextReader _reader = new StreamReader(stream, encoding, leaveOpen: leaveOpen);
    private bool _isDisposed;
    private long _numCharactersRead;

    public ScanTextSearcher(Stream stream, bool leaveOpen = false) : this(stream, Encoding.UTF8, leaveOpen)
    {
    }

    public SearchResult? FindNext(Regex searchPattern)
    {
        var startOfLineOffset = _numCharactersRead;
        for(var line = _reader.ReadLine(); line != null; line = _reader.ReadLine())
        {
            _numCharactersRead += line.Length;
            var match = searchPattern.Match(line);
            if (!match.Success)
            {
                startOfLineOffset = _numCharactersRead;
                continue;
            }

            var prefix = line[..match.Index];
            encoding.GetByteCount(prefix);
            var matchOffset = startOfLineOffset + match.Index;
            return new SearchResult(matchOffset, match.Value);
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
