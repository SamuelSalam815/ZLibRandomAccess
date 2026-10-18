using System.Buffers;
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
    private readonly RingBuffer<char> _text;
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
        _text = new RingBuffer<char>(capacityInChars);
        FirstLineNumber = lineAtStart;
        _isSkippingPartialLine = startsMidLine;
    }

    /// <summary>The index of the first line in <see cref="Text"/>.</summary>
    public long FirstLineNumber { get; private set; }

    public bool IsAtEndOfStream { get; private set; }

    /// <summary>The buffered text, valid until the next call to <see cref="Fill"/> or <see cref="DropLines"/>.</summary>
    public ReadOnlySpanPair<char> Text => _text.Contents;

    /// <summary>The number of lines in <see cref="Text"/>. After the end of the stream, an unfinished last line counts.</summary>
    public int LineCount => _lineFeedCount + (IsAtEndOfStream && _text.Length > 0 && Text[^1] != '\n' ? 1 : 0);

    /// <summary>Reads until <see cref="LineCount"/> reaches the target or the stream ends.</summary>
    /// <exception cref="LineTooLongException">The buffer is full and still holds too few lines.</exception>
    public void Fill()
    {
        while (LineCount < _targetLineCount && !IsAtEndOfStream)
        {
            // n bytes decode to at most n + 1 characters, so reading one byte fewer than the free space cannot overflow.
            var bytesToRead = Math.Min(_bytes.Length, _text.Capacity - _text.Length - 1);
            if (bytesToRead <= 0)
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

    private void AppendDecoded(int byteCount)
    {
        var chars = ArrayPool<char>.Shared.Rent(_encoding.GetMaxCharCount(byteCount));
        try
        {
            var charCount = _decoder.GetChars(_bytes, 0, byteCount, chars, 0);
            ReadOnlySpan<char> decoded = chars.AsSpan(0, charCount);
            if (_isSkippingPartialLine)
            {
                var lineFeedIndex = decoded.IndexOf('\n');
                if (lineFeedIndex < 0)
                {
                    return;
                }

                decoded = decoded[(lineFeedIndex + 1)..];
                _isSkippingPartialLine = false;
                FirstLineNumber++;
            }

            _text.Append(decoded);
            _lineFeedCount += decoded.Count('\n');
        }
        finally
        {
            ArrayPool<char>.Shared.Return(chars);
        }
    }
}
