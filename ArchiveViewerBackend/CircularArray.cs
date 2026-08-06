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

    public void Drop(int count = 1)
    {
        if (count == 0)
        {
            return;
        }

        if (Length == 0)
        {
            throw new InvalidOperationException("Cannot drop items from an empty circular array!");
        }

        if (count < 0 || count > Length)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, $"Expected drop count to be in range [0, {Length}]");
        }

        _start = (_start + count) % _buffer.Length;
        _itemCount -= count;
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
    /// The first of two span representations for this circular array.
    /// Contains all the elements before the start pointer must wrap around.
    /// </summary>
    public Span<T> GetWritableSpan()
    {
        var startOfSpan = _start + _itemCount;
        var contiguousLength = _buffer.Length - _start;
        var remainingCapacity = _buffer.Length - _itemCount;
        var spanLength = Math.Min(contiguousLength, remainingCapacity);
        return _buffer.AsSpan(startOfSpan, spanLength);
    }

    /// <summary>
    /// The second of two span representations for this circular array.
    /// Contains all the elements after the start pointer has wrapped around.
    /// </summary>
    /// <returns></returns>
    public Span<T> GetSecondSpan()
    {
        return _buffer.AsSpan(0, _itemCount - GetWritableSpan().Length);
    }

    public void SimulateAdd(int itemCount)
    {
        for (var i = 0; i < itemCount; i++)
        {
            SimulateAddOnce();
        }
    }

    private void SimulateAddOnce()
    {
        _end = (_end + 1) % _buffer.Length;
        if (_itemCount < Capacity)
        {
            _itemCount++;
        }
        else
        {
            _start = (_start + 1) % _buffer.Length;
        }
    }
}
