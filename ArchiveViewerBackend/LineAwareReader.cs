using System.Text;

namespace ArchiveViewerBackend;

/// <summary>
/// Reads text forward from a stream into a buffer of characters and keeps count of the lines in it.
/// <see cref="Fill"/> reads until the buffer holds the target number of lines, and <see cref="DropLines"/>
/// removes lines from the start so that the caller can scroll forward.
/// </summary>
public sealed class LineAwareReader
{
    private const int ChunkSizeInBytes = 4096;

    private readonly Stream _source;
    private readonly Encoding _encoding;
    private readonly Decoder _decoder;
    private readonly int _targetLineCount;
    private readonly CompactingBuffer<char> _text;
    private readonly byte[] _bytes = new byte[ChunkSizeInBytes];
    private int _lineFeedCount;
    private bool _isSkippingPartialLine;

    /// <param name="source">A stream positioned at the first byte to read.</param>
    /// <param name="targetLineCount">The number of lines <see cref="Fill"/> reads up to.</param>
    /// <param name="capacityInChars">The most characters the buffer holds. Must be at least 2.</param>
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
        ArgumentOutOfRangeException.ThrowIfLessThan(capacityInChars, 2);
        _source = source;
        _encoding = encoding;
        _decoder = encoding.GetDecoder();
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
            var bytesToRead = BytesThatCertainlyFit(_text.Capacity - _text.Length);
            if (bytesToRead == 0)
            {
                throw new LineTooLongException(_text.Capacity);
            }

            var bytesRead = _source.Read(_bytes, 0, bytesToRead);
            if (bytesRead == 0)
            {
                IsAtEndOfStream = true;
                return;
            }

            AppendDecoded(bytesRead);
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

    /// <summary>
    /// The decoder throws if its output span is too small, so the reader reads only as many bytes as are
    /// certain to decode into the free space. That can be fewer than the free space in characters: n bytes can
    /// decode to more than n characters, because the decoder holds a partly received character between reads
    /// and the next bytes can complete it. For example, 3 bytes of an emoji arrived last time and 1 byte
    /// arrives now; that 1 byte produces 2 characters (UTF-8 reports <c>n + 1</c>). The headroom comes from the
    /// encoding's own <see cref="Encoding.GetMaxCharCount"/>, so it is right for every encoding.
    /// </summary>
    private int BytesThatCertainlyFit(int freeChars)
    {
        var byteCount = Math.Min(_bytes.Length, freeChars);
        while (byteCount > 0 && _encoding.GetMaxCharCount(byteCount) > freeChars)
        {
            byteCount--;
        }

        return byteCount;
    }

    private void AppendDecoded(int byteCount)
    {
        var writable = _text.GetWritableSpan(_encoding.GetMaxCharCount(byteCount));
        var charCount = _decoder.GetChars(_bytes.AsSpan(0, byteCount), writable, flush: false);
        if (_isSkippingPartialLine)
        {
            var lineFeedIndex = writable[..charCount].IndexOf('\n');
            if (lineFeedIndex < 0)
            {
                // The whole chunk is the rest of the partial line. Nothing is reported written, so it is discarded.
                return;
            }

            var wholeLines = writable[(lineFeedIndex + 1)..charCount];
            wholeLines.CopyTo(writable);
            charCount = wholeLines.Length;
            _isSkippingPartialLine = false;
            FirstLineNumber++;
        }

        ReadOnlySpan<char> decoded = writable[..charCount];
        _lineFeedCount += decoded.Count('\n');
        _text.ReportWritten(charCount);
    }
}
