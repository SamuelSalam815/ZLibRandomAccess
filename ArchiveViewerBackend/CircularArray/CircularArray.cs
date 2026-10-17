using System.Collections;

namespace ArchiveViewerBackend.CircularArray;

public class CircularArray<T>(int capacity) : IEnumerable<T>
{
    private readonly T[] _backingArray = new T[capacity];
    private CircularArrayState _state = new(capacity);

    public int Capacity => _backingArray.Length;

    public int Length => _state.Length;

    public void Add(T item)
    {
        AddRange([item]);
    }

    public void AddRange(ReadOnlySpan<T> items)
    {
        foreach (var write in _state.Append(items.Length))
        {
            var count = write.Count;
            items[..count].CopyTo(_backingArray.AsSpan(write.OffsetInBackingArray));
            items = items[count..];
            _state = write.NewState;
        }
    }

    public void Drop(int dropCount = 1)
    {
        _state = _state.DropFromStart(dropCount);
    }

    /// <summary>
    /// Exposes the next contiguous unused region for writing only, and starts a span operation.
    /// Write into the span, then call <see cref="ReportItemsWritten"/>.
    /// Every other operation throws until then.
    /// </summary>
    public Span<T> GetNextWritableSpan()
    {
        var write = _state.BeginWriteSpan();
        _state = write.NewState;
        return _backingArray.AsSpan(
            write.OffsetInBackingArray,
            write.Count);
    }

    /// <summary>
    /// Ends the span operation started by <see cref="GetNextWritableSpan"/>.
    /// </summary>
    /// <param name="count">How many items were written to the start of the span.</param>
    public void ReportItemsWritten(int count)
    {
        _state = _state.EndWriteSpan(count);
    }

    public T this[int index] =>
        _backingArray[_state.GetOffsetOfItem(index)];

    /// <summary>
    /// Exposes the first contiguous region of items for reading. If the items wrap around,
    /// the items after the wrap are not included.
    /// </summary>
    public Span<T> GetNextUsedSpan()
    {
        var read = _state.Enumerate().FirstOrDefault();

        return read is null
            ? Span<T>.Empty
            : _backingArray.AsSpan(read.OffsetInBackingArray, read.Count);
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var read in _state.Enumerate())
        {
            for (var i = 0; i < read.Count; i++)
            {
                yield return _backingArray[read.OffsetInBackingArray + i];
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
