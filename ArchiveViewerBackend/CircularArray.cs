using System.Collections;

namespace ArchiveViewerBackend;

public class CircularArray<T> : IEnumerable<T>
{
    private readonly T[] _buffer;

    private int _start;
    private int _end;
    private int _itemCount;

    public CircularArray(int capacity)
    {
        _buffer = new T[capacity];
        _start = 0;
        _end = 0;
        _itemCount = 0;
    }

    public int Length => _itemCount;

    public int Capacity => _buffer.Length;

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < Length; i++)
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(T item)
    {
        _buffer[_end] = item;
        SimulateAdd(1);
    }

    public void Drop(int dropCount = 1)
    {
        if (dropCount == 0)
        {
            return;
        }

        if (Length == 0)
        {
            throw new InvalidOperationException("Cannot drop items from an empty circular array!");
        }

        if (dropCount < 0 || dropCount > Length)
        {
            throw new ArgumentOutOfRangeException(nameof(dropCount), dropCount, $"Expected drop count to be in range [0, {Length}]");
        }

        _start = (_start + dropCount) % _buffer.Length;
        _itemCount -= dropCount;
    }

    public void AddRange(ReadOnlySpan<T> items)
    {
        foreach (var item in items)
        {
            Add(item);
        }
    }

    public T this[int t]
    {
        get {
            if (t < 0 || t >= _buffer.Length)
            {
                throw new IndexOutOfRangeException($"Tried to index {t}, but it is is not in the allowed range [{0}, {_buffer.Length - 1}]");
            }
            return _buffer[(_start + t) % _buffer.Length];
        }
    }

    /// <summary>
    /// Represents the next contiguous region of this array that is not being used.
    /// These locations are the ones that will be written to next if there were further calls to <see cref="Add"/>.
    /// When used with <see cref="SimulateAdd"/>, this methos allows one to directly write to the span
    /// and then report how many bytes were written.
    /// </summary>
    public Span<T> GetNextUnusedSpan()
    {
        var startOfSpan = (_start + _itemCount) % _buffer.Length;
        var contiguousLength = _buffer.Length - startOfSpan;
        var remainingCapacity = _buffer.Length - _itemCount;
        var spanLength = Math.Min(contiguousLength, remainingCapacity);
        return _buffer.AsSpan(startOfSpan, spanLength);
    }

    /// <summary>
    /// Represents the next contiguous region of this array that is being used.
    /// Allows one to read from this array as if it were a span. If the collection wraps around before listing
    /// all current items, then the items after wrapping around will not be included.
    /// </summary>
    public Span<T> GetNextUsedSpan()
    {
        var spanLength = Math.Min(_itemCount, _buffer.Length - _start);
        return _buffer.AsSpan(_start, spanLength);
    }

    public void SimulateAdd(int additionalItemCount)
    {
        _end = (_end + additionalItemCount) % _buffer.Length;
        if (_itemCount + additionalItemCount <= Capacity)
        {
            _itemCount += additionalItemCount;
        }
        else
        {
            var overflowAmount = _itemCount + additionalItemCount - Capacity;
            _start = (_start + overflowAmount) % _buffer.Length;
            _itemCount = Capacity;
        }
    }
}
