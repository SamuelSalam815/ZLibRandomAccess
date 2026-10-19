namespace ArchiveViewerBackend;

/// <summary>
/// A fixed-capacity buffer that holds its items in one contiguous run. Callers write at the end through
/// <see cref="GetWritableSpan"/> and drop items from the start with <see cref="DropFirst"/>.
/// </summary>
/// <remarks>
/// A ring buffer was rejected. It stores its items in at most two pieces, and its writable space is cut at
/// the end of the array in the same way. The callers here decode into that space with a <see cref="System.Text.Decoder"/>,
/// which needs one contiguous output span and throws instead of splitting a character (for example a
/// surrogate pair) between two spans. After items are dropped from the front, a ring can offer a span of only
/// one or two cells even though most of the buffer is free. This buffer keeps its items in one run and moves
/// them to the start of the array when a caller needs more contiguous room.
/// </remarks>
public sealed class CompactingBuffer<T>(int capacity)
{
    private readonly T[] _array = new T[capacity];
    private int _start;

    public int Capacity { get; } = capacity;

    public int Length { get; private set; }

    /// <summary>The items, valid until the next mutation.</summary>
    public ReadOnlySpan<T> Contents => _array.AsSpan(_start, Length);

    /// <summary>The number of free cells after the last item.</summary>
    private int TailLength => Capacity - _start - Length;

    /// <summary>
    /// Returns the free space after the last item. Write into it, then call <see cref="ReportWritten"/>.
    /// If the free space after the last item is shorter than <paramref name="minimumLength"/>, the items are first
    /// moved to the start of the array, so the span is never shorter than that.
    /// </summary>
    /// <param name="minimumLength">The least length needed. When omitted, all the remaining capacity.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="minimumLength"/> is negative or more than the remaining capacity.
    /// </exception>
    public Span<T> GetWritableSpan(int? minimumLength = null)
    {
        var remainingCapacity = Capacity - Length;
        var wantedLength = minimumLength ?? remainingCapacity;
        if (wantedLength < 0 || wantedLength > remainingCapacity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minimumLength),
                wantedLength,
                $"Expected minimum length to be in range [0, {remainingCapacity}]");
        }

        if (TailLength < wantedLength)
        {
            // Span.CopyTo handles overlapping source and destination.
            Contents.CopyTo(_array);
            _start = 0;
        }

        return _array.AsSpan(_start + Length, TailLength);
    }

    /// <summary>Adds the first <paramref name="count"/> items written into the span from <see cref="GetWritableSpan"/>.</summary>
    public void ReportWritten(int count)
    {
        if (count < 0 || count > TailLength)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, $"Expected written count to be in range [0, {TailLength}]");
        }

        Length += count;
    }

    public void DropFirst(int count)
    {
        if (count < 0 || count > Length)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, $"Expected drop count to be in range [0, {Length}]");
        }

        _start += count;
        Length -= count;
        if (Length == 0)
        {
            // An empty buffer can offer its whole capacity without moving anything.
            _start = 0;
        }
    }

    public void Clear()
    {
        _start = 0;
        Length = 0;
    }
}
