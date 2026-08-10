using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(CircularArray<>))]
public class CircularArrayTest
{
    private static CircularArray<int> CreateTestArray() => new(3);

    [TestMethod]
    public void AddingItems_MutatesTheArray()
    {
        var sut = new CircularArray<int>(5) { 3 };
        sut.First().ShouldBe(3);
    }

    [TestMethod]
    public void AddingItems_AllowsThemToBeIteratedOver()
    {
        var expectedItems = new[] { 3, 2, 8 };
        var sut = CreateTestArray();
        sut.AddRange(expectedItems);
        sut.ShouldBe(expectedItems);
    }

    [TestMethod]
    public void ExceedingBufferCapacity_ShouldOverwriteItems()
    {
        var itemsToAdd = new[] { 6, 2, 8, 1 };
        var sut = CreateTestArray();
        sut.AddRange(itemsToAdd);
        sut.ShouldBe([2, 8, 1]);
    }

    [TestMethod]
    public void Capacity_ShouldGetPassedThrough()
    {
        new CircularArray<int>(1337).Capacity.ShouldBe(1337);
    }

    [TestMethod]
    public void Add_ShouldIncreaseLength()
    {
        var sut = CreateTestArray();
        for (var i = 0; i < sut.Capacity; i++)
        {
            sut.Length.ShouldBe(i);
            sut.Add(i);
            sut.Length.ShouldBe(i + 1);
        }
    }

    [TestMethod]
    public void Add_ShouldNotIncreaseLengthAboveCapacity()
    {
        var sut = CreateTestArray();
        for (var i = 0; i < sut.Capacity * 2; i++)
        {
            sut.Add(i);
        }

        sut.Length.ShouldBe(sut.Capacity);
    }

    [TestMethod]
    public void Drop_ShouldThrowIfCountIsNegative()
    {
        var action = () => CreateTestArray().Drop(-3);
        action.ShouldThrow<Exception>();
    }

    [TestMethod]
    public void CanDropZeroItems_FromEmptyArray()
    {
        var action = () => CreateTestArray().Drop(0);
        action.ShouldNotThrow();
    }

    [TestMethod]
    [DataRow(0, 1)]
    [DataRow(1, 2)]
    [DataRow(2, 3)]
    [DataRow(3, 4)]
    public void Drop_ShouldThrowIfDropCountExceedsLength(int itemCountToAdd, int dropCount)
    {
        var sut = CreateTestArray();
        for (var i = 0; i < itemCountToAdd; i++)
        {
            sut.Add(i);
        }

        var action = () => sut.Drop(dropCount);
        action.ShouldThrow<Exception>();
    }

    [TestMethod]
    public void DroppingFromAnEmptyArray_ShouldThrow()
    {
        var action = () => CreateTestArray().Drop();
        action.ShouldThrow<InvalidOperationException>();
    }

    [TestMethod]
    public void DroppingAnItem_ShouldRemoveIt()
    {
        var sut = CreateTestArray();
        sut.AddRange([3, 4, 5]);
        sut.Drop();
        sut.ShouldBe([4, 5]);
    }

    [TestMethod]
    public void AddingAndDroppingItemsWithinCapacity_ShouldBehaveLikeInfiniteArray()
    {
        var sut = CreateTestArray();
        sut.AddRange([1, 2, 3]);
        sut.Drop(2);
        sut.AddRange([4, 5]);
        sut.Drop(2);
        sut.AddRange([6, 7]);
        sut.ShouldBe([5, 6, 7]);
    }

    [TestMethod]
    [DataRow(new[]{1,2,3,4}, 2, new[]{ 5,6 }, 0, new []{4,5,6})]
    // TODO: add more tests
    public void UnusedSpan_ShouldRepresentSpaceLeftBeforeWrapAround(
        int[] itemsToInsert,
        int itemCountToDrop,
        int[] itemsToWriteToSpan,
        int expectedLengthOfLastWritableSpan,
        int[] expectedFinalArray)
    {
        var sut = CreateTestArray();
        sut.AddRange(itemsToInsert);
        sut.Drop(itemCountToDrop);
        itemsToWriteToSpan.CopyTo(sut.GetNextUnusedSpan());
        sut.SimulateAdd(itemsToWriteToSpan.Length);
        sut.GetNextUnusedSpan().Length.ShouldBe(expectedLengthOfLastWritableSpan);
        sut.ShouldBe(expectedFinalArray);
    }

    [TestMethod]
    [DataRow(new int[] {}, new int [] {})]
    [DataRow(new[] {1,}, new [] {1})]
    [DataRow(new[] {1,2}, new [] {1,2})]
    [DataRow(new[] {1,2,3}, new [] {1,2,3})]
    [DataRow(new[] {1,2,3,4}, new [] {2, 3})]
    [DataRow(new[] {1,2,3,4,5}, new [] {3})]
    [DataRow(new[] {1,2,3,4,5,6}, new [] {4,5,6})]
    public void UsedSpan_ShouldRepresentItemsBeforeAWrapAround(
        int[] itemsToInsert,
        int[] expectedUsedSpan)
    {
        var sut = CreateTestArray();
        sut.AddRange(itemsToInsert);
        sut.GetNextUsedSpan().ToArray().ShouldBe(expectedUsedSpan);
    }

    [TestMethod]
    [DataRow(new[] { 5, }, 0, new[] { 5 })]
    [DataRow(new[] { 5, }, 1, new[] { 5, 0 })]
    [DataRow(new[] { 5, }, 2, new[] { 5, 0, 0 })]
    [DataRow(new[] { 5, }, 3, new[] { 0, 0, 5 })]
    [DataRow(new[] { 5, }, 4, new[] { 0, 5, 0 })]
    [DataRow(new[] { 5, }, 5, new[] { 5, 0, 0 })]
    [DataRow(new[] { 5, 8 }, 0, new[] { 5, 8 })]
    [DataRow(new[] { 5, 8 }, 1, new[] { 5, 8, 0 })]
    [DataRow(new[] { 5, 8 }, 2, new[] { 8, 0, 5 })]
    [DataRow(new[] { 5, 8 }, 3, new[] { 0, 5, 8})]
    [DataRow(new[] { 5, 8 }, 4, new[] { 5, 8, 0})]
    [DataRow(new[] { 1, 2, 3 }, 0, new[] { 1, 2, 3 })]
    [DataRow(new[] { 1, 2, 3 }, 1, new[] { 2, 3, 1 })]
    [DataRow(new[] { 1, 2, 3 }, 2, new[] { 3, 1, 2 })]
    [DataRow(new[] { 1, 2, 3 }, 3, new[] { 1, 2, 3 })]
    public void SimulateAdd_ShouldActAsIfAnItemHasBeenAddedWithoutChangingArrayContents(
        int[] itemsToInsert,
        int cycleCount,
        int[] expectedArray)
    {
        var sut = CreateTestArray();
        sut.AddRange(itemsToInsert);
        sut.SimulateAdd(cycleCount);
        sut.ShouldBe(expectedArray);
    }


}
