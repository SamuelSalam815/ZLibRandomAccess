using System.Text;
using ZLibWrapper;

namespace ArchiveViewerBackend;

/// <summary>
/// Presents a window of lines from a large text stream for a front end to render. It keeps more lines
/// buffered than it renders, so scrolling within the buffer does no I/O. Scrolling outside the buffer
/// reloads it, seeking to the last recovery point before the wanted line.
/// </summary>
public sealed class TextPresenter
{
    private readonly Stream _stream;
    private readonly IReadOnlyList<RecoveryPointOffset> _recoveryPoints;
    private readonly Encoding _encoding;
    private readonly int _renderedLineCount;
    private readonly int _bufferedLineCount;
    private readonly int _bufferCapacityInChars;
    private LineAwareReader _reader;

    /// <param name="stream">A seekable stream of decompressed text, such as <see cref="BackwardSeekableStream"/>.</param>
    /// <param name="recoveryPoints">Recovery points in ascending order. They are assumed to fall mid-line.</param>
    /// <param name="renderedLineCount">The number of lines <see cref="Render"/> returns.</param>
    /// <param name="bufferedLineCount">The number of lines to keep buffered. At least <paramref name="renderedLineCount"/>.</param>
    /// <param name="bufferCapacityInChars">The most characters to buffer. A line longer than this throws <see cref="LineTooLongException"/>.</param>
    public TextPresenter(
        Stream stream,
        IReadOnlyList<RecoveryPointOffset> recoveryPoints,
        Encoding encoding,
        int renderedLineCount,
        int bufferedLineCount,
        int bufferCapacityInChars)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(bufferedLineCount, renderedLineCount);
        _stream = stream;
        _recoveryPoints = recoveryPoints;
        _encoding = encoding;
        _renderedLineCount = renderedLineCount;
        _bufferedLineCount = bufferedLineCount;
        _bufferCapacityInChars = bufferCapacityInChars;
        _reader = Reload(0);
    }

    /// <summary>The index of the first line <see cref="Render"/> returns.</summary>
    public long FirstRenderedLine { get; private set; }

    /// <summary>Makes <paramref name="firstLine"/> the first rendered line, reloading the buffer if needed.</summary>
    public void ScrollTo(long firstLine)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(firstLine);
        var isBuffered = firstLine >= _reader.FirstLineNumber
            && (firstLine + _renderedLineCount <= _reader.FirstLineNumber + _reader.LineCount || _reader.IsAtEndOfStream);
        if (!isBuffered)
        {
            // Put the wanted lines in the middle of the buffer so that scrolling either way stays buffered.
            LoadBufferStartingAt(Math.Max(0, firstLine - (_bufferedLineCount - _renderedLineCount) / 2));
        }

        FirstRenderedLine = firstLine;
    }

    /// <summary>The rendered lines without line endings. There are fewer near the end of the stream.</summary>
    public IReadOnlyList<string> Render()
    {
        var lines = new List<string>(_renderedLineCount);
        var firstBufferedIndex = FirstRenderedLine - _reader.FirstLineNumber;
        var text = _reader.Text;
        long lineIndex = 0;
        var lineStart = 0;
        for (var i = 0; i < text.Length && lines.Count < _renderedLineCount; i++)
        {
            if (text[i] != '\n')
            {
                continue;
            }

            if (lineIndex >= firstBufferedIndex)
            {
                lines.Add(LineText(text, lineStart, i - lineStart));
            }

            lineIndex++;
            lineStart = i + 1;
        }

        var hasUnfinishedLastLine = _reader.IsAtEndOfStream && lineStart < text.Length;
        if (hasUnfinishedLastLine && lineIndex >= firstBufferedIndex && lines.Count < _renderedLineCount)
        {
            lines.Add(LineText(text, lineStart, text.Length - lineStart));
        }

        return lines;
    }

    private static string LineText(ReadOnlySpanPair<char> text, int start, int length)
    {
        if (length > 0 && text[start + length - 1] == '\r')
        {
            length--;
        }

        var chars = new char[length];
        text.Slice(start, length).CopyTo(chars);
        return new string(chars);
    }

    private void LoadBufferStartingAt(long start)
    {
        var isInsideOrJustAfterBuffer = start >= _reader.FirstLineNumber
            && start <= _reader.FirstLineNumber + _reader.LineCount;
        if (isInsideOrJustAfterBuffer)
        {
            _reader.DropLines((int)(start - _reader.FirstLineNumber));
            _reader.Fill();
            return;
        }

        _reader = Reload(start);
    }

    /// <summary>Seeks to the last recovery point whose following whole line is at or before <paramref name="start"/>.</summary>
    private LineAwareReader Reload(long start)
    {
        // A recovery point is mid-line, so the first whole line after it is LineOffset + 1.
        var pointIndex = -1;
        for (var i = 0; i < _recoveryPoints.Count && _recoveryPoints[i].LineOffset + 1 <= start; i++)
        {
            pointIndex = i;
        }

        LineAwareReader reader;
        if (pointIndex < 0)
        {
            _stream.Position = 0;
            reader = new LineAwareReader(_stream, _encoding, _bufferedLineCount, _bufferCapacityInChars);
        }
        else
        {
            var point = _recoveryPoints[pointIndex];
            _stream.Position = point.OffsetInUncompressedStream;
            reader = new LineAwareReader(
                _stream,
                _encoding,
                _bufferedLineCount,
                _bufferCapacityInChars,
                lineAtStart: point.LineOffset,
                startsMidLine: true);
        }

        reader.Fill();
        while (reader.FirstLineNumber < start && reader.LineCount > 0)
        {
            reader.DropLines((int)Math.Min(start - reader.FirstLineNumber, reader.LineCount));
            reader.Fill();
        }

        return reader;
    }
}
