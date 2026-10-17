using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

using ArchiveViewerBackend.CircularArray;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(CircularArrayState))]
public class CircularArrayStateTest
{
    private readonly CircularArrayState _size10CircularArray = new(10);

    private static CircularArrayWrite Write(int offset, int count, CircularArrayState state)
    {
        return new CircularArrayWrite(offset, count, state);
    }

    private static CircularArrayRead Read(int offset, int count)
    {
        return new CircularArrayRead(offset, count);
    }

    [TestMethod]
    public void LengthShouldBeZero_AfterCreation()
    {
        _size10CircularArray.Length.ShouldBe(0);
    }

    [TestMethod]
    public void AppendIncrementsEndOffset()
    {
        _size10CircularArray.Append(3)
            .ShouldBe(
            [
                Write(
                    0,
                    3,
                    _size10CircularArray with { EndOffset = 3, Length = 3 }),
            ]);
    }

    [TestMethod]
    public void AppendIncreasesLength()
    {
        _size10CircularArray.Append(3).LastState().Length.ShouldBe(3);
    }

    [TestMethod]
    public void AppendWrapsAround_IntoTwoWrites()
    {
        var startingState = new CircularArrayState(3);
        var afterTwo = startingState.Append(2).LastState();

        afterTwo
            .Append(3)
            .ShouldBe(
                [
                    Write(
                        2,
                        1,
                        startingState with { EndOffset = 0, Length = 3 }),
                    Write(
                        0,
                        2,
                        startingState with
                        {
                            EndOffset = 2, StartOffset = 2, Length = 3
                        }),
                ]
            );
    }

    [TestMethod]
    public void AppendToFullArray_MovesStartOffset()
    {
        var full = _size10CircularArray.Append(10).LastState();

        full.Append(5)
            .ShouldBe(
            [
                Write(
                    0,
                    5,
                    _size10CircularArray with
                    {
                        EndOffset = 5, StartOffset = 5, Length = 10
                    }),
            ]);
    }

    [TestMethod]
    public void AppendWrapsAround_OnFullArray()
    {
        var startingState = new CircularArrayState(3);
        var overfilled = startingState.OverfillBy(2);

        overfilled
            .Append(2)
            .ShouldBe(
            [
                Write(
                    2,
                    1,
                    startingState with
                    {
                        EndOffset = 0, StartOffset = 0, Length = 3
                    }),
                Write(
                    0,
                    1,
                    startingState with
                    {
                        EndOffset = 1, StartOffset = 1, Length = 3
                    }),
            ]);
    }

    [TestMethod]
    public void AppendOfCapacity_IsSingleWrite()
    {
        _size10CircularArray
            .Append(10)
            .ShouldBe(
            [
                Write(
                    0,
                    10,
                    _size10CircularArray with { EndOffset = 0, Length = 10 }),
            ]);
    }

    [TestMethod]
    public void AppendDoesNotIncreaseLengthBeyondCapacity()
    {
        var full = _size10CircularArray.Append(10).LastState();

        full.Append(5).LastState()
            .Length.ShouldBe(_size10CircularArray.BackingArrayCapacity);
    }

    [TestMethod]
    public void AppendZero_YieldsNoInstructions()
    {
        _size10CircularArray.Append(0).ShouldBeEmpty();
    }

    [TestMethod]
    public void AppendNegative_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(
            () => _size10CircularArray.Append(-1));
    }

    [TestMethod]
    public void AppendMoreThanCapacity_WrapsAroundRepeatedly()
    {
        var instructions = _size10CircularArray.Append(25).ToList();

        instructions.ShouldBe(
        [
            Write(0, 10, _size10CircularArray with { EndOffset = 0, Length = 10 }),
            Write(0, 10, _size10CircularArray with { EndOffset = 0, Length = 10 }),
            Write(
                0,
                5,
                _size10CircularArray with { EndOffset = 5, StartOffset = 5, Length = 10 }),
        ]);
    }

    [TestMethod]
    public void DropZeroItems_FromEmptyArray_DoesNotThrow()
    {
        _size10CircularArray.DropFromStart(0).ShouldBe(_size10CircularArray);
    }

    [TestMethod]
    public void DropFromEmptyArray_Throws()
    {
        Should.Throw<InvalidOperationException>(
            () => _size10CircularArray.DropFromStart(1));
    }

    [TestMethod]
    public void DropFromStart_OutOfRange_Throws()
    {
        var threeItems = _size10CircularArray.Append(3).LastState();

        Should.Throw<ArgumentOutOfRangeException>(
            () => threeItems.DropFromStart(-1));
        Should.Throw<ArgumentOutOfRangeException>(
            () => threeItems.DropFromStart(4));
    }

    [TestMethod]
    public void DropFromStart_MovesStartOffsetAndShrinksLength()
    {
        var threeItems = _size10CircularArray.Append(3).LastState();

        threeItems
            .DropFromStart(2)
            .ShouldBe(
                _size10CircularArray with
                {
                    StartOffset = 2, EndOffset = 3, Length = 1
                });
    }

    [TestMethod]
    public void DropFromStart_WrapsStartOffset()
    {
        var startingState = new CircularArrayState(3);
        var overfilled = startingState.OverfillBy(2);

        overfilled
            .DropFromStart(2)
            .ShouldBe(
                startingState with
                {
                    StartOffset = 1, EndOffset = 2, Length = 1
                });
    }

    [TestMethod]
    public void DropEverything_LeavesStartAndEndOffsetEqual()
    {
        var threeItems = _size10CircularArray.Append(3).LastState();

        var dropped = threeItems.DropFromStart(3);

        dropped.Length.ShouldBe(0);
        dropped.StartOffset.ShouldBe(dropped.EndOffset);
    }

    [TestMethod]
    public void DropZeroItemsFromEnd_FromEmptyArray_DoesNotThrow()
    {
        _size10CircularArray.DropFromEnd(0).ShouldBe(_size10CircularArray);
    }

    [TestMethod]
    public void DropFromEnd_OnEmptyArray_Throws()
    {
        Should.Throw<InvalidOperationException>(
            () => _size10CircularArray.DropFromEnd(1));
    }

    [TestMethod]
    public void DropFromEnd_OutOfRange_Throws()
    {
        var threeItems = _size10CircularArray.Append(3).LastState();

        Should.Throw<ArgumentOutOfRangeException>(
            () => threeItems.DropFromEnd(-1));
        Should.Throw<ArgumentOutOfRangeException>(
            () => threeItems.DropFromEnd(4));
    }

    [TestMethod]
    public void DropFromEnd_MovesEndOffsetAndShrinksLength()
    {
        var threeItems = _size10CircularArray.Append(3).LastState();

        threeItems
            .DropFromEnd(2)
            .ShouldBe(
                _size10CircularArray with
                {
                    StartOffset = 0, EndOffset = 1, Length = 1
                });
    }

    [TestMethod]
    public void DropFromEnd_WrapsEndOffset()
    {
        var startingState = new CircularArrayState(3);

        startingState.OverfillBy(2)
            .DropFromEnd(1)
            .ShouldBe(
                startingState with
                {
                    StartOffset = 2, EndOffset = 1, Length = 2
                });
    }

    [TestMethod]
    public void DropEverythingFromEnd_LeavesStartAndEndOffsetEqual()
    {
        var wrapped = new CircularArrayState(3).OverfillBy(2);

        var dropped = wrapped.DropFromEnd(3);

        dropped.Length.ShouldBe(0);
        dropped.EndOffset.ShouldBe(dropped.StartOffset);
    }

    [TestMethod]
    public void DropFromEnd_WhileSpanPending_Throws()
    {
        var pending = _size10CircularArray.BeginWriteSpan().NewState;

        Should.Throw<InvalidOperationException>(
            () => pending.DropFromEnd(0));
    }

    [TestMethod]
    public void Prepend_OnEmptyArray_WritesBeforeTheStart()
    {
        _size10CircularArray.Prepend(3)
            .ShouldBe(
            [
                Write(
                    7,
                    3,
                    _size10CircularArray with { StartOffset = 7, EndOffset = 0, Length = 3 }),
            ]);
    }

    [TestMethod]
    public void Prepend_WrappingBelowZero_YieldsTwoWritesInBlockOrder()
    {
        var twoItemsFromOffset1 = _size10CircularArray.Append(3).LastState().DropFromStart(1);
        var expectedState = _size10CircularArray with { StartOffset = 8, EndOffset = 3, Length = 5 };

        twoItemsFromOffset1.Prepend(3)
            .ShouldBe(
            [
                Write(8, 2, expectedState),
                Write(0, 1, expectedState),
            ]);
    }

    [TestMethod]
    public void Prepend_OnFullArray_EvictsItemsFromTheEnd()
    {
        var startingState = new CircularArrayState(3);
        var full = startingState.Append(3).LastState();

        full.Prepend(1)
            .ShouldBe(
            [
                Write(2, 1, startingState with { StartOffset = 2, EndOffset = 2, Length = 3 }),
            ]);
    }

    [TestMethod]
    public void Prepend_MoreThanCapacity_KeepsOnlyTheFirstItemsOfTheBlock()
    {
        var startingState = new CircularArrayState(3);

        startingState.Prepend(5)
            .ShouldBe(
            [
                Write(0, 3, startingState with { StartOffset = 0, EndOffset = 0, Length = 3 }),
            ]);
    }

    [TestMethod]
    public void PrependZero_YieldsNoInstructions()
    {
        _size10CircularArray.Prepend(0).ShouldBeEmpty();
    }

    [TestMethod]
    public void Prepend_NegativeCount_ThrowsEagerly()
    {
        Should.Throw<ArgumentOutOfRangeException>(
            () => _size10CircularArray.Prepend(-1));
    }

    [TestMethod]
    public void Prepend_WhileSpanPending_Throws()
    {
        var pending = _size10CircularArray.BeginWriteSpan().NewState;

        Should.Throw<InvalidOperationException>(
            () => pending.Prepend(1));
    }

    [TestMethod]
    public void BeginWriteSpan_OnEmptyArray_CoversWholeArray()
    {
        _size10CircularArray
            .BeginWriteSpan()
            .ShouldBe(
                Write(
                    0,
                    10,
                    _size10CircularArray with { IsSpanOperationPending = true }));
    }

    [TestMethod]
    public void BeginWriteSpan_StopsAtWrapPoint()
    {
        var startingState = new CircularArrayState(3);
        var twoItems = startingState.Append(2).LastState();

        twoItems
            .BeginWriteSpan()
            .ShouldBe(Write(2, 1, twoItems with { IsSpanOperationPending = true }));
    }

    [TestMethod]
    public void BeginWriteSpan_OnFullArray_IsEmpty()
    {
        var full = _size10CircularArray.Append(10).LastState();

        full.BeginWriteSpan().Count.ShouldBe(0);
    }

    [TestMethod]
    public void EndWriteSpan_AppendsReportedItemsAndClearsFlag()
    {
        _size10CircularArray
            .BeginWriteSpan()
            .NewState
            .EndWriteSpan(4)
            .ShouldBe(_size10CircularArray with { EndOffset = 4, Length = 4 });
    }

    [TestMethod]
    public void EndWriteSpan_CanFillTheArray()
    {
        _size10CircularArray
            .BeginWriteSpan()
            .NewState
            .EndWriteSpan(10)
            .ShouldBe(_size10CircularArray with { EndOffset = 0, Length = 10 });
    }

    [TestMethod]
    public void EndWriteSpan_WithZeroItems_OnFullArray_ClearsFlag()
    {
        var full = _size10CircularArray.Append(10).LastState();

        full.BeginWriteSpan().NewState.EndWriteSpan(0).ShouldBe(full);
    }

    [TestMethod]
    public void EndWriteSpan_OutOfRange_Throws()
    {
        var pending = new CircularArrayState(3).OverfillBy(2)
            .DropFromStart(1)
            .BeginWriteSpan()
            .NewState;

        // One item fits before the wrap point, so 2 is already too many.
        Should.Throw<ArgumentOutOfRangeException>(() => pending.EndWriteSpan(-1));
        Should.Throw<ArgumentOutOfRangeException>(() => pending.EndWriteSpan(2));
    }

    [TestMethod]
    public void EndWriteSpan_WithNothingPending_Throws()
    {
        Should.Throw<InvalidOperationException>(
            () => _size10CircularArray.EndWriteSpan(0));
    }

    [TestMethod]
    public void OperationsOtherThanEndWriteSpan_Throw_WhileSpanOperationPending()
    {
        var pending = _size10CircularArray.BeginWriteSpan().NewState;

        Should.Throw<InvalidOperationException>(() => pending.BeginWriteSpan());
        Should.Throw<InvalidOperationException>(() => pending.Append(1));
        Should.Throw<InvalidOperationException>(() => pending.DropFromStart(0));
        Should.Throw<InvalidOperationException>(() => pending.Enumerate());
    }

    [TestMethod]
    public void Enumerate_EmptyArray_YieldsNothing()
    {
        _size10CircularArray.Enumerate().ShouldBeEmpty();
    }

    [TestMethod]
    public void Enumerate_YieldsSingleRead_WhenItemsDoNotWrap()
    {
        _size10CircularArray.Append(3).LastState()
            .DropFromStart(1)
            .Enumerate()
            .ShouldBe([Read(1, 2)]);
    }

    [TestMethod]
    public void Enumerate_YieldsTwoReads_WhenItemsWrap()
    {
        new CircularArrayState(3).OverfillBy(2)
            .Enumerate()
            .ShouldBe([Read(2, 1), Read(0, 2)]);
    }

    [TestMethod]
    public void Enumerate_FullArrayStartingAtZero_YieldsSingleRead()
    {
        _size10CircularArray.Append(10).LastState()
            .Enumerate()
            .ShouldBe([Read(0, 10)]);
    }

    [TestMethod]
    public void GetOffsetOfItem_MapsIndexThroughStartOffset()
    {
        var wrapped = new CircularArrayState(3).OverfillBy(2);

        wrapped.GetOffsetOfItem(0).ShouldBe(2);
        wrapped.GetOffsetOfItem(1).ShouldBe(0);
        wrapped.GetOffsetOfItem(2).ShouldBe(1);
    }

    [TestMethod]
    public void GetOffsetOfItem_OutsideLength_Throws()
    {
        var threeItems = _size10CircularArray.Append(3).LastState();

        Should.Throw<IndexOutOfRangeException>(() => threeItems.GetOffsetOfItem(-1));
        Should.Throw<IndexOutOfRangeException>(() => threeItems.GetOffsetOfItem(3));
    }

    [TestMethod]
    public void GetOffsetOfItem_Throws_WhileSpanOperationPending()
    {
        var pending = _size10CircularArray.BeginWriteSpan().NewState;

        Should.Throw<InvalidOperationException>(() => pending.GetOffsetOfItem(0));
    }
}

internal static class CircularArrayStateTestExtensions
{
    public static CircularArrayState LastState(this IEnumerable<CircularArrayWrite> writes)
    {
        return writes.Last().NewState;
    }

    /// <summary>
    /// Fills the array, then appends <paramref name="itemCount"/> more items,
    /// so the oldest items are overwritten and the offsets wrap around.
    /// </summary>
    public static CircularArrayState OverfillBy(this CircularArrayState state, int itemCount)
    {
        return state.Append(state.BackingArrayCapacity + itemCount).LastState();
    }
}
