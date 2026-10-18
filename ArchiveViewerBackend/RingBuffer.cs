using System.Collections;

namespace ArchiveViewerBackend;

/// <summary>
/// A fixed-capacity buffer of items that mutates itself in place. Blocks are added at either end
/// and items are dropped from either end. When an add exceeds the capacity, items are evicted from the
/// opposite end.
/// </summary>
/// <remarks>
/// Items are stored in one backing array, so the logical contents are at most two contiguous slices.
/// Slices are views of the backing array and are valid only until the next mutation.
/// </remarks>
public sealed class RingBuffer<T>(int capacity) : IEnumerable<T>
{
    private readonly T[] _array = new T[capacity];
    private int _start;

    public int Capacity => _array.Length;

    public int Length { get; private set; }

    /// <summary>The slice that starts at the first item. It is empty only when the buffer is empty.</summary>
    public ReadOnlyMemory<T> FirstSegment => _array.AsMemory(_start, Math.Min(Length, Capacity - _start));

    /// <summary>The slice that continues after <see cref="FirstSegment"/>. It is empty unless the items wrap.</summary>
    public ReadOnlyMemory<T> SecondSegment => _array.AsMemory(0, Length - FirstSegment.Length);

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

    public void DropFromStart(int count)
    {
        if (count == 0)
        {
            return;
        }

        ThrowIfInvalidDropCount(count);
        _start = (_start + count) % Capacity;
        Length -= count;
    }

    public void DropFromEnd(int count)
    {
        if (count == 0)
        {
            return;
        }

        ThrowIfInvalidDropCount(count);
        Length -= count;
    }

    public IEnumerator<T> GetEnumerator()
    {
        var first = FirstSegment;
        for (var i = 0; i < first.Length; i++)
        {
            yield return first.Span[i];
        }

        var second = SecondSegment;
        for (var i = 0; i < second.Length; i++)
        {
            yield return second.Span[i];
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

    private void ThrowIfInvalidDropCount(int count)
    {
        if (Length == 0)
        {
            throw new InvalidOperationException("Cannot drop from an empty buffer");
        }

        if (count < 0 || count > Length)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, $"Expected drop count to be in range [0, {Length}]");
        }
    }
}
