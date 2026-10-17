using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(CircularArrayState))]
public class CircularArrayStateTest
{
    private readonly CircularArrayState _size10CircularArray = new(10);

    private static CircularArrayInstruction.Write Write(long offset, long count, CircularArrayState state)
    {
        return new CircularArrayInstruction.Write(offset, count, state);
    }

    private static CircularArrayInstruction.Read Read(long offset, long count)
    {
        return new CircularArrayInstruction.Read(offset, count);
    }

    private static CircularArrayState StateAfter(IEnumerable<CircularArrayInstruction> instructions)
    {
        return instructions
            .OfType<CircularArrayInstruction.Write>()
            .Last()
            .NewState;
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
        StateAfter(_size10CircularArray.Append(3)).Length.ShouldBe(3);
    }

    [TestMethod]
    public void AppendWrapsAround_IntoTwoWrites()
    {
        var startingState = new CircularArrayState(3);
        var afterTwo = StateAfter(startingState.Append(2));

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
        var full = StateAfter(_size10CircularArray.Append(10));

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
        var fullAtEnd = StateAfter(
            StateAfter(startingState.Append(3)).Append(2));

        fullAtEnd
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
        var full = StateAfter(_size10CircularArray.Append(10));

        StateAfter(full.Append(5))
            .Length.ShouldBe(_size10CircularArray.BackingArrayCapacity);
    }

    [TestMethod]
    public void AppendZero_YieldsNoInstructions()
    {
        _size10CircularArray.Append(0).ShouldBeEmpty();
    }

    [TestMethod]
    public void AppendOutOfRange_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(
            () => _size10CircularArray.Append(-1));
        Should.Throw<ArgumentOutOfRangeException>(
            () => _size10CircularArray.Append(11));
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
        var threeItems = StateAfter(_size10CircularArray.Append(3));

        Should.Throw<ArgumentOutOfRangeException>(
            () => threeItems.DropFromStart(-1));
        Should.Throw<ArgumentOutOfRangeException>(
            () => threeItems.DropFromStart(4));
    }

    [TestMethod]
    public void DropFromStart_MovesStartOffsetAndShrinksLength()
    {
        var threeItems = StateAfter(_size10CircularArray.Append(3));

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
        var fullAtEnd = StateAfter(
            StateAfter(startingState.Append(3)).Append(2));

        fullAtEnd
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
        var threeItems = StateAfter(_size10CircularArray.Append(3));

        var dropped = threeItems.DropFromStart(3);

        dropped.Length.ShouldBe(0);
        dropped.StartOffset.ShouldBe(dropped.EndOffset);
    }
}
