using System.Collections;

namespace ArchiveViewerBackend;

public abstract record CircularArrayInstruction
{
    private CircularArrayInstruction()
    {
    }

    public sealed record Write(long OffsetInBackingArray, CircularArrayState NewState) : CircularArrayInstruction;
    public sealed record Read(long OffsetInBackingArray, long Count) : CircularArrayInstruction;
}

public record CircularArrayState(long BackingArrayCapacity)
{
    public long Length { get; init; } = 0;
    public long StartOffset { get; init; } = 0;
    public long EndOffset { get; init; } = 0;

    public IEnumerable<CircularArrayInstruction> Append(long itemCount)
    {
        return Append(itemCount, this);
    }

    private static IEnumerable<CircularArrayInstruction> Append(
        long itemCount,
        CircularArrayState currentState)
    {
        for (var i = 0; i < itemCount; i++)
        {
            var newState = currentState.ExpandForwardByOne();

            yield return new CircularArrayInstruction.Write(
                currentState.EndOffset,
                newState);

            currentState = newState;
        }
    }

    private CircularArrayState ExpandForwardByOne()
    {
        var newLength =
            Length + 1 <= BackingArrayCapacity ? Length + 1 : Length;

        var newStartOffset = Length == BackingArrayCapacity
                ? PositiveModulo.Fix(StartOffset + 1, BackingArrayCapacity)
                : StartOffset;

        var newEndOffset = PositiveModulo.Fix(
            EndOffset + 1,
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
