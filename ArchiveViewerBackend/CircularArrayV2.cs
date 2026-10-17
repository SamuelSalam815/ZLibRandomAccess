using System.Collections;

namespace ArchiveViewerBackend;

public sealed record CircularArrayWrite(int OffsetInBackingArray, int Count, CircularArrayState NewState);

public sealed record CircularArrayRead(int OffsetInBackingArray, int Count);

public record CircularArrayState(int BackingArrayCapacity)
{
    public int Length { get; init; } = 0;
    public int StartOffset { get; init; } = 0;
    public int EndOffset { get; init; } = 0;

    /// <summary>
    /// True between <see cref="BeginWriteSpan"/> and <see cref="EndWriteSpan"/>.
    /// While a span operation is pending, only <see cref="EndWriteSpan"/> is allowed.
    /// </summary>
    public bool IsSpanOperationPending { get; init; } = false;

    private void ThrowIfSpanOperationPending()
    {
        if (IsSpanOperationPending)
        {
            throw new InvalidOperationException(
                "A span operation is pending. Report the items written to it first.");
        }
    }

    /// <summary>
    /// Starts a span operation on the next contiguous unused region, which is
    /// where the next appended items would go. The caller writes into that region
    /// and then calls <see cref="EndWriteSpan"/> with the number of items written.
    /// </summary>
    public CircularArrayWrite BeginWriteSpan()
    {
        ThrowIfSpanOperationPending();

        return new CircularArrayWrite(
            EndOffset,
            MaxWriteSpanLength,
            this with { IsSpanOperationPending = true });
    }

    public CircularArrayState EndWriteSpan(int itemCountWritten)
    {
        if (!IsSpanOperationPending)
        {
            throw new InvalidOperationException("No span operation is pending.");
        }

        if (itemCountWritten < 0 || itemCountWritten > MaxWriteSpanLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(itemCountWritten),
                itemCountWritten,
                $"Expected items written to be in range [0, {MaxWriteSpanLength}]");
        }

        return GetStateAfterAppending(itemCountWritten) with
        {
            IsSpanOperationPending = false,
        };
    }

    /// <summary>
    /// Maps an index among the items (0 is the first item) to its offset in the backing array.
    /// </summary>
    public int GetOffsetOfItem(int index)
    {
        ThrowIfSpanOperationPending();

        if (index < 0 || index >= Length)
        {
            throw new IndexOutOfRangeException(
                $"Tried to index {index}, but it is not in the allowed range [0, {Length - 1}]");
        }

        return PositiveModulo.Fix(StartOffset + index, BackingArrayCapacity);
    }

    private int MaxWriteSpanLength => Math.Min(
        BackingArrayCapacity - EndOffset,
        BackingArrayCapacity - Length);

    /// <summary>
    /// Yields one <see cref="CircularArrayWrite"/> per contiguous region, wrapping
    /// around the backing array as often as needed. When <paramref name="itemCount"/> exceeds
    /// the capacity, later writes overwrite earlier ones, so the newest items win.
    /// </summary>
    public IEnumerable<CircularArrayWrite> Append(int itemCount)
    {
        ThrowIfSpanOperationPending();

        if (itemCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(itemCount),
                itemCount,
                "Expected item count to not be negative");
        }

        return AppendCore(itemCount);
    }

    // Split from Append because an iterator body only runs on the first MoveNext.
    // Append validates eagerly so a bad call or a pending span throws at the call site.
    private IEnumerable<CircularArrayWrite> AppendCore(int itemCount)
    {
        var state = this;
        var remaining = itemCount;
        while (remaining > 0)
        {
            var count = Math.Min(remaining, BackingArrayCapacity - state.EndOffset);
            var next = state.GetStateAfterAppending(count);
            yield return new CircularArrayWrite(state.EndOffset, count, next);
            state = next;
            remaining -= count;
        }
    }

    private CircularArrayState GetStateAfterAppending(int count)
    {
        var newLength = Math.Min(Length + count, BackingArrayCapacity);

        // Items that no longer fit push the start offset forward.
        var overflow = Length + count - newLength;
        var newStartOffset = PositiveModulo.Fix(
            StartOffset + overflow,
            BackingArrayCapacity);

        var newEndOffset = PositiveModulo.Fix(
            EndOffset + count,
            BackingArrayCapacity);

        return this with
        {
            Length = newLength,
            StartOffset = newStartOffset,
            EndOffset = newEndOffset,
        };
    }

    private CircularArrayState GetStateAfterPrepending(int count)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<CircularArrayWrite> Prepend(int itemCount)
    {
        ThrowIfSpanOperationPending();

        throw new NotImplementedException();
    }

    /// <summary>
    /// Removes <paramref name="itemCount"/> items from the start of the array.
    /// Needs no I/O, so it returns the new state rather than instructions.
    /// </summary>
    public CircularArrayState DropFromStart(int itemCount)
    {
        ThrowIfSpanOperationPending();

        if (itemCount == 0)
        {
            return this;
        }

        if (Length == 0)
        {
            throw new InvalidOperationException("Cannot drop items from an empty circular array!");
        }

        if (itemCount < 0 || itemCount > Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(itemCount),
                itemCount,
                $"Expected drop count to be in range [0, {Length}]");
        }

        return this with
        {
            Length = Length - itemCount,
            StartOffset = PositiveModulo.Fix(
                StartOffset + itemCount,
                BackingArrayCapacity),
        };
    }

    /// <summary>
    /// Removes <paramref name="itemCount"/> items from the end of the array.
    /// Needs no I/O, so it returns the new state rather than instructions.
    /// </summary>
    public CircularArrayState DropFromEnd(int itemCount)
    {
        ThrowIfSpanOperationPending();

        throw new NotImplementedException();
    }

    /// <summary>
    /// Yields at most two <see cref="CircularArrayRead"/> instructions,
    /// which together cover the items in order: one up to the end of the backing array and,
    /// if the items wrap, one from its beginning.
    /// </summary>
    public IEnumerable<CircularArrayRead> Enumerate()
    {
        ThrowIfSpanOperationPending();

        return EnumerateCore();
    }

    // Split from Enumerate for the same reason as AppendCore: validate eagerly, iterate lazily.
    private IEnumerable<CircularArrayRead> EnumerateCore()
    {
        if (Length == 0)
        {
            yield break;
        }

        var firstCount = Math.Min(Length, BackingArrayCapacity - StartOffset);
        yield return new CircularArrayRead(StartOffset, firstCount);

        var secondCount = Length - firstCount;
        if (secondCount > 0)
        {
            yield return new CircularArrayRead(0, secondCount);
        }
    }
}

public class CircularArrayV2<T>(int capacity) : IEnumerable<T>
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
