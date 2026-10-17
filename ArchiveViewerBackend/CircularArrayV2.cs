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
    /// Yields at most two <see cref="CircularArrayInstruction.Write"/> instructions:
    /// one up to the end of the backing array and, if the append wraps, one from its beginning.
    /// </summary>
    public IEnumerable<CircularArrayInstruction> Append(long itemCount)
    {
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
        var stateAfterFirst = ExpandForward(firstCount);
        yield return new CircularArrayInstruction.Write(
            EndOffset,
            firstCount,
            stateAfterFirst);

        var secondCount = itemCount - firstCount;
        if (secondCount > 0)
        {
            yield return new CircularArrayInstruction.Write(
                stateAfterFirst.EndOffset,
                secondCount,
                stateAfterFirst.ExpandForward(secondCount));
        }
    }

    private CircularArrayState ExpandForward(long count)
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

    private CircularArrayState ExpandBackwardsByOne()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<CircularArrayInstruction> Prepend(long itemCount)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Removes <paramref name="itemCount"/> items from the start of the array.
    /// Needs no I/O, so it returns the new state rather than instructions.
    /// </summary>
    public CircularArrayState DropFromStart(long itemCount)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Removes <paramref name="itemCount"/> items from the end of the array.
    /// Needs no I/O, so it returns the new state rather than instructions.
    /// </summary>
    public CircularArrayState DropFromEnd(long itemCount)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<CircularArrayInstruction> Enumerate()
    {
        throw new NotImplementedException();
    }
}

public class CircularArrayV2<T> : IEnumerable<T>
{
    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
