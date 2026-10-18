namespace ArchiveViewerBackend.CircularArray;

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
        var sourceOffset = 0;
        while (sourceOffset < itemCount)
        {
            var count = Math.Min(itemCount - sourceOffset, BackingArrayCapacity - state.EndOffset);
            var next = state.GetStateAfterAppending(count);
            yield return new CircularArrayWrite(state.EndOffset, count, next, sourceOffset);
            state = next;
            sourceOffset += count;
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
        var newLength = Math.Min(Length + count, BackingArrayCapacity);

        // Items that no longer fit are evicted from the end.
        var overflow = Length + count - newLength;
        var newEndOffset = PositiveModulo.Fix(
            EndOffset - overflow,
            BackingArrayCapacity);

        var newStartOffset = PositiveModulo.Fix(
            StartOffset - count,
            BackingArrayCapacity);

        return this with
        {
            Length = newLength,
            StartOffset = newStartOffset,
            EndOffset = newEndOffset,
        };
    }

    /// <summary>
    /// Places a block of <paramref name="itemCount"/> items before the first item, keeping the
    /// block's order. Yields one <see cref="CircularArrayWrite"/> per contiguous region. Each
    /// <see cref="CircularArrayWrite.NewState"/> is valid, with no unwritten gap.
    /// When the items do not fit, the items at the end are evicted. If the block alone exceeds
    /// the capacity, only its first items are kept.
    /// </summary>
    public IEnumerable<CircularArrayWrite> Prepend(int itemCount)
    {
        ThrowIfSpanOperationPending();

        if (itemCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(itemCount),
                itemCount,
                "Expected item count to not be negative");
        }

        return PrependCore(itemCount);
    }

    // Split from Prepend for the same reason as AppendCore: validate eagerly, iterate lazily.
    private IEnumerable<CircularArrayWrite> PrependCore(int itemCount)
    {
        var keptCount = Math.Min(itemCount, BackingArrayCapacity);
        if (keptCount == 0)
        {
            yield break;
        }

        var finalStartOffset = GetStateAfterPrepending(keptCount).StartOffset;
        var leadingSegmentCount = Math.Min(keptCount, BackingArrayCapacity - finalStartOffset);
        var trailingSegmentCount = keptCount - leadingSegmentCount;

        // Rejected: writing the leading segment first. It would leave an unwritten gap between
        // itself and the current start, so the intermediate state would be invalid.
        var state = this;
        if (trailingSegmentCount > 0)
        {
            state = state.GetStateAfterPrepending(trailingSegmentCount);
            yield return new CircularArrayWrite(0, trailingSegmentCount, state, leadingSegmentCount);
        }

        state = state.GetStateAfterPrepending(leadingSegmentCount);
        yield return new CircularArrayWrite(finalStartOffset, leadingSegmentCount, state, 0);
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
            EndOffset = PositiveModulo.Fix(
                EndOffset - itemCount,
                BackingArrayCapacity),
        };
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
