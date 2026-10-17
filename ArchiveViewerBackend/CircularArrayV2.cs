using System.Collections;

namespace ArchiveViewerBackend;

public abstract record CircularArrayInstruction
{
    private CircularArrayInstruction()
    {
    }

    public sealed record Write(long OffsetInBackingArray, long Count, CircularArrayState NewState) : CircularArrayInstruction;
    public sealed record Read(long OffsetInBackingArray, long Count) : CircularArrayInstruction;
}

public record CircularArrayState(long BackingArrayCapacity)
{
    public long Length { get; init; } = 0;
    public long StartOffset { get; init; } = 0;
    public long EndOffset { get; init; } = 0;

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
    public CircularArrayInstruction.Write BeginWriteSpan()
    {
        ThrowIfSpanOperationPending();

        return new CircularArrayInstruction.Write(
            EndOffset,
            MaxWriteSpanLength,
            this with { IsSpanOperationPending = true });
    }

    public CircularArrayState EndWriteSpan(long itemsWritten)
    {
        if (!IsSpanOperationPending)
        {
            throw new InvalidOperationException("No span operation is pending.");
        }

        if (itemsWritten < 0 || itemsWritten > MaxWriteSpanLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(itemsWritten),
                itemsWritten,
                $"Expected items written to be in range [0, {MaxWriteSpanLength}]");
        }

        return GetStateAfterAppending(itemsWritten) with
        {
            IsSpanOperationPending = false,
        };
    }

    private long MaxWriteSpanLength => Math.Min(
        BackingArrayCapacity - EndOffset,
        BackingArrayCapacity - Length);

    /// <summary>
    /// Yields at most two <see cref="CircularArrayInstruction.Write"/> instructions:
    /// one up to the end of the backing array and, if the append wraps, one from its beginning.
    /// </summary>
    public IEnumerable<CircularArrayInstruction> Append(long itemCount)
    {
        ThrowIfSpanOperationPending();

        if (itemCount < 0 || itemCount > BackingArrayCapacity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(itemCount),
                itemCount,
                $"Expected item count to be in range [0, {BackingArrayCapacity}]");
        }

        return AppendCore(itemCount);
    }

    private IEnumerable<CircularArrayInstruction> AppendCore(long itemCount)
    {
        if (itemCount == 0)
        {
            yield break;
        }

        var firstCount = Math.Min(itemCount, BackingArrayCapacity - EndOffset);
        var stateAfterFirst = GetStateAfterAppending(firstCount);
        yield return new CircularArrayInstruction.Write(
            EndOffset,
            firstCount,
            stateAfterFirst);

        var secondCount = itemCount - firstCount;
        if (secondCount <= 0)
        {
            yield break;
        }

        yield return new CircularArrayInstruction.Write(
            stateAfterFirst.EndOffset,
            secondCount,
            stateAfterFirst.GetStateAfterAppending(secondCount));
    }

    private CircularArrayState GetStateAfterAppending(long count)
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

    private CircularArrayState GetStateAfterPrepending(long count)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<CircularArrayInstruction> Prepend(long itemCount)
    {
        ThrowIfSpanOperationPending();

        throw new NotImplementedException();
    }

    /// <summary>
    /// Removes <paramref name="itemCount"/> items from the start of the array.
    /// Needs no I/O, so it returns the new state rather than instructions.
    /// </summary>
    public CircularArrayState DropFromStart(long itemCount)
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
    public CircularArrayState DropFromEnd(long itemCount)
    {
        ThrowIfSpanOperationPending();

        throw new NotImplementedException();
    }

    /// <summary>
    /// Yields at most two <see cref="CircularArrayInstruction.Read"/> instructions,
    /// which together cover the items in order: one up to the end of the backing array and,
    /// if the items wrap, one from its beginning.
    /// </summary>
    public IEnumerable<CircularArrayInstruction> Enumerate()
    {
        ThrowIfSpanOperationPending();

        return EnumerateCore();
    }

    private IEnumerable<CircularArrayInstruction> EnumerateCore()
    {
        if (Length == 0)
        {
            yield break;
        }

        var firstCount = Math.Min(Length, BackingArrayCapacity - StartOffset);
        yield return new CircularArrayInstruction.Read(StartOffset, firstCount);

        var secondCount = Length - firstCount;
        if (secondCount > 0)
        {
            yield return new CircularArrayInstruction.Read(0, secondCount);
        }
    }
}

public class CircularArrayV2<T> : IEnumerable<T>
{
    private readonly T[] _backingArray;
    private CircularArrayState _state;

    public CircularArrayV2(int capacity)
    {
        _backingArray = new T[capacity];
        _state = new CircularArrayState(capacity);
    }

    public int Capacity => _backingArray.Length;

    public int Length => (int)_state.Length;

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
