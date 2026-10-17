using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using ArchiveViewerBackend;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(CircularArrayState))]
public class CircularArrayStateTest
{
    private readonly CircularArrayState _size10CircularArray = new(10);

    private static CircularArrayInstruction.Write Write(long offset, CircularArrayState state)
    {
        return new CircularArrayInstruction.Write(offset, state);
    }

    private static CircularArrayInstruction.Read Read(long offset, long count)
    {
        return new CircularArrayInstruction.Read(offset, count);
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
                    _size10CircularArray with { EndOffset = 1, Length = 1 }),
                Write(
                    1,
                    _size10CircularArray with { EndOffset = 2, Length = 2 }),
                Write(
                    2,
                    _size10CircularArray with { EndOffset = 3, Length = 3 }),
            ]);
    }

    [TestMethod]
    public void AppendIncreasesLength()
    {
        _size10CircularArray
            .Append(3)
            .Last()
            .ShouldBeOfType<CircularArrayInstruction.Write>()
            .NewState
            .Length.ShouldBe(3);
    }

    [TestMethod]
    public void AppendWrapsAround()
    {
        var startingState = new CircularArrayState(3);
        startingState
            .Append(7)
            .ShouldMatch(
                [
                    Write(0, startingState with { EndOffset = 1, Length = 1 }),
                    Write(1, startingState with { EndOffset = 2, Length = 2 }),
                    Write(2, startingState with { EndOffset = 0, Length = 3 }),
                    Write(
                        0,
                        startingState with
                        {
                            EndOffset = 1, StartOffset = 1, Length = 3
                        }),
                    Write(
                        1,
                        startingState with
                        {
                            EndOffset = 2, StartOffset = 2, Length = 3
                        }),
                    Write(
                        2,
                        startingState with
                        {
                            EndOffset = 0, StartOffset = 0, Length = 3
                        }),
                    Write(
                        0,
                        startingState with
                        {
                            EndOffset = 1, StartOffset = 1, Length = 3
                        }),
                ]
            );
    }

    [TestMethod]
    public void AppendDoesNotIncreaseLengthBeyondCapacity()
    {
        _size10CircularArray
            .Append(_size10CircularArray.BackingArrayCapacity + 5)
            .Last()
            .ShouldBeOfType<CircularArrayInstruction.Write>()
            .NewState
            .Length.ShouldBe(_size10CircularArray.BackingArrayCapacity);
    }
}
