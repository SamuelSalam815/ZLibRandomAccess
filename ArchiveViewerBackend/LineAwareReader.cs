using System.Text;

namespace ArchiveViewerBackend;

/// <summary>
/// Reads text forward from a stream into a buffer of characters and keeps count of the lines in it.
/// <see cref="Fill"/> reads until the buffer holds the target number of lines, and <see cref="DropLines"/>
/// removes lines from the start so that the caller can scroll forward.
/// </summary>
public sealed class LineAwareReader
{
    private readonly StreamReader _reader;
    private readonly int _targetLineCount;
    private readonly CompactingBuffer<char> _text;
    private int _lineFeedCount;
    private bool _isSkippingPartialLine;

    /// <param name="source">
    /// A stream positioned at the first byte to read. It is read ahead, so do not rely on its position afterwards.
    /// </param>
    /// <param name="targetLineCount">The number of lines <see cref="Fill"/> reads up to.</param>
    /// <param name="capacityInChars">The most characters the buffer holds. Must be at least 1.</param>
    /// <param name="lineAtStart">The index of the line that <paramref name="source"/> is positioned in.</param>
    /// <param name="startsMidLine">
    /// True when <paramref name="source"/> is positioned inside a line, such as at a recovery point. The rest
    /// of that line is discarded, so the buffer starts at the next whole line.
    /// </param>
    public LineAwareReader(
        Stream source,
        Encoding encoding,
        int targetLineCount,
        int capacityInChars,
        long lineAtStart = 0,
        bool startsMidLine = false)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacityInChars, 1);
        // The source stays open: it belongs to the caller. A byte order mark would be data in the middle of a file.
        _reader = new StreamReader(source, encoding, detectEncodingFromByteOrderMarks: false, bufferSize: -1, leaveOpen: true);
        _targetLineCount = targetLineCount;
        _text = new CompactingBuffer<char>(capacityInChars);
        FirstLineNumber = lineAtStart;
        _isSkippingPartialLine = startsMidLine;
    }

    /// <summary>The index of the first line in <see cref="Text"/>.</summary>
    public long FirstLineNumber { get; private set; }

    public bool IsAtEndOfStream { get; private set; }

    /// <summary>The buffered text, valid until the next call to <see cref="Fill"/> or <see cref="DropLines"/>.</summary>
    public ReadOnlySpan<char> Text => _text.Contents;

    /// <summary>The number of lines in <see cref="Text"/>. After the end of the stream, an unfinished last line counts.</summary>
    public int LineCount => _lineFeedCount + (IsAtEndOfStream && _text.Length > 0 && Text[^1] != '\n' ? 1 : 0);

    /// <summary>Reads until <see cref="LineCount"/> reaches the target or the stream ends.</summary>
    /// <exception cref="LineTooLongException">The buffer is full and still holds too few lines.</exception>
    public void Fill()
    {
        while (LineCount < _targetLineCount && !IsAtEndOfStream)
        {
            var writable = _text.GetWritableSpan();
            if (writable.IsEmpty)
            {
                throw new LineTooLongException(_text.Capacity);
            }

            var charCount = _reader.Read(writable);
            if (charCount == 0)
            {
                IsAtEndOfStream = true;
                return;
            }

            Commit(writable, charCount);
        }
    }

    /// <summary>Removes the first <paramref name="count"/> lines from <see cref="Text"/>.</summary>
    public void DropLines(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, LineCount);

        var charsToDrop = 0;
        var lineFeedsDropped = 0;
        if (count > 0)
        {
            foreach (var character in Text)
            {
                charsToDrop++;
                if (character == '\n' && ++lineFeedsDropped == count)
                {
                    break;
                }
            }
        }

        _text.DropFirst(charsToDrop);
        _lineFeedCount -= lineFeedsDropped;
        FirstLineNumber += count;
    }

    /// <summary>Adds the characters just read into <paramref name="writable"/> to the buffer.</summary>
    private void Commit(Span<char> writable, int charCount)
    {
        if (_isSkippingPartialLine)
        {
            var lineFeedIndex = writable[..charCount].IndexOf('\n');
            if (lineFeedIndex < 0)
            {
                // The whole read is the rest of the partial line. Nothing is reported written, so it is discarded.
                return;
            }

            var wholeLines = writable[(lineFeedIndex + 1)..charCount];
            wholeLines.CopyTo(writable);
            charCount = wholeLines.Length;
            _isSkippingPartialLine = false;
            FirstLineNumber++;
        }

        ReadOnlySpan<char> read = writable[..charCount];
        _lineFeedCount += read.Count('\n');
        _text.ReportWritten(charCount);
    }
}
