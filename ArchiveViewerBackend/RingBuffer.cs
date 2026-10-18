using System.Collections;

namespace ArchiveViewerBackend;

/// <summary>
/// A fixed-capacity buffer of items that mutates itself in place. Blocks are added at either end
/// and items are dropped from either end. When an add exceeds the capacity, items are evicted from the
/// opposite end.
/// </summary>
/// <remarks>
/// Items are stored in one backing array, so the logical contents are at most two contiguous slices.
/// <see cref="Contents"/> is a view of the backing array and is valid only until the next mutation.
/// </remarks>
public sealed class RingBuffer<T>(int capacity) : IEnumerable<T>
{
    private readonly T[] _array = new T[capacity];
    private int _start;

    public int Capacity { get; } = capacity;

    public int Length { get; private set; }

    /// <summary>
    /// The items as a view of the backing array, valid only until the next mutation. The second span is
    /// empty unless the items wrap.
    /// </summary>
    public ReadOnlySpanPair<T> Contents => new(
        _array.AsSpan(_start, FirstSegmentLength),
        _array.AsSpan(0, Length - FirstSegmentLength));

    private int FirstSegmentLength => Math.Min(Length, Capacity - _start);

    private int WritableSpanLength => Math.Min(Capacity - Length, Capacity - (_start + Length) % Capacity);

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Length)
            {
                throw new IndexOutOfRangeException($"Expected index to be in range [0, {Length - 1}] but was {index}");
            }

            return _array[(_start + index) % Capacity];
        }
    }

    /// <summary>
    /// Adds <paramref name="block"/> after the last item. Items that do not fit are evicted from the start.
    /// A block larger than the capacity keeps only its last <see cref="Capacity"/> items.
    /// </summary>
    public void Append(ReadOnlySpan<T> block)
    {
        if (block.Length > Capacity)
        {
            block = block[^Capacity..];
        }

        var overflow = Math.Max(0, Length + block.Length - Capacity);
        _start = (_start + overflow) % Capacity;
        Length -= overflow;

        var writeOffset = (_start + Length) % Capacity;
        CopyWrapping(block, writeOffset);
        Length += block.Length;
    }

    /// <summary>
    /// Adds <paramref name="block"/> before the first item. Items that do not fit are evicted from the end.
    /// A block larger than the capacity keeps only its first <see cref="Capacity"/> items.
    /// </summary>
    public void Prepend(ReadOnlySpan<T> block)
    {
        if (block.Length > Capacity)
        {
            block = block[..Capacity];
        }

        var overflow = Math.Max(0, Length + block.Length - Capacity);
        Length -= overflow;

        _start = PositiveModulo.Fix(_start - block.Length, Capacity);
        CopyWrapping(block, _start);
        Length += block.Length;
    }

    public void DropFirst(int count)
    {
        if (count == 0)
        {
            return;
        }

        ThrowIfCannotDrop(count);
        _start = (_start + count) % Capacity;
        Length -= count;
    }

    public void DropLast(int count)
    {
        if (count == 0)
        {
            return;
        }

        ThrowIfCannotDrop(count);
        Length -= count;
    }

    public void Clear()
    {
        _start = 0;
        Length = 0;
    }

    /// <summary>
    /// Returns the free space after the last item that can be written without wrapping. It is empty only
    /// when the buffer is full. Write into it, then call <see cref="ReportWritten"/>.
    /// </summary>
    public Span<T> GetWritableSpan() => _array.AsSpan((_start + Length) % Capacity, WritableSpanLength);

    /// <summary>Adds the first <paramref name="count"/> items written into the span from <see cref="GetWritableSpan"/>.</summary>
    public void ReportWritten(int count)
    {
        if (count < 0 || count > WritableSpanLength)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, $"Expected written count to be in range [0, {WritableSpanLength}]");
        }

        Length += count;
    }

    // An iterator cannot hold spans across a yield, so this indexes the backing array
    // using the same first/second split as Contents.
    public IEnumerator<T> GetEnumerator()
    {
        var firstSegmentLength = FirstSegmentLength;
        for (var i = 0; i < firstSegmentLength; i++)
        {
            yield return _array[_start + i];
        }

        for (var i = 0; i < Length - firstSegmentLength; i++)
        {
            yield return _array[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Copies <paramref name="block"/> to the backing array at <paramref name="offset"/>, continuing at 0 past the end.</summary>
    private void CopyWrapping(ReadOnlySpan<T> block, int offset)
    {
        var firstCount = Math.Min(block.Length, Capacity - offset);
        block[..firstCount].CopyTo(_array.AsSpan(offset));
        block[firstCount..].CopyTo(_array);
    }

    private void ThrowIfCannotDrop(int count)
    {
        if (count < 0 || count > Length)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, $"Expected drop count to be in range [0, {Length}]");
        }
    }
}
