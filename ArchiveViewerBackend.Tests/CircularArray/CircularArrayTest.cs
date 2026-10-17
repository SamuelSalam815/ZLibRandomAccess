using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

using ArchiveViewerBackend.CircularArray;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(CircularArray<>))]
public class CircularArrayTest
{
    private static CircularArray<int> CreateTestArray() => new(3);

    [TestMethod]
    public void Capacity_ShouldGetPassedThrough()
    {
        new CircularArray<int>(1337).Capacity.ShouldBe(1337);
    }

    [TestMethod]
    public void LengthShouldBeZero_AfterCreation()
    {
        CreateTestArray().Length.ShouldBe(0);
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
    public void AddRange_MoreThanCapacity_KeepsNewestItems()
    {
        var sut = CreateTestArray();
        sut.AddRange([1, 2, 3, 4, 5, 6, 7]);
        sut.ShouldBe([5, 6, 7]);
    }

    [TestMethod]
    public void Drop_ShouldShrinkLength()
    {
        var sut = CreateTestArray();
        sut.AddRange([3, 4, 5]);
        sut.Drop();
        sut.Length.ShouldBe(2);
    }

    [TestMethod]
    public void Drop_ShouldThrowIfCountIsNegative()
    {
        var action = () => CreateTestArray().Drop(-3);
        action.ShouldThrow<Exception>();
    }

    [TestMethod]
    public void DroppingFromAnEmptyArray_ShouldThrow()
    {
        var action = () => CreateTestArray().Drop();
        action.ShouldThrow<InvalidOperationException>();
    }

    [TestMethod]
    public void WritableSpan_ShouldCoverWholeEmptyArray()
    {
        CreateTestArray().GetNextWritableSpan().Length.ShouldBe(3);
    }

    [TestMethod]
    public void ReportItemsWritten_ShouldAppendTheWrittenItems()
    {
        var sut = CreateTestArray();
        new[] { 7, 8 }.CopyTo(sut.GetNextWritableSpan());
        sut.ReportItemsWritten(2);
        sut.Length.ShouldBe(2);
        sut.GetNextWritableSpan().Length.ShouldBe(1);
    }

    [TestMethod]
    public void WritableSpan_ShouldStopAtWrapAround()
    {
        var sut = CreateTestArray();
        sut.AddRange([1, 2, 3]);
        sut.Drop(2);
        sut.GetNextWritableSpan().Length.ShouldBe(2);
    }

    [TestMethod]
    public void OperationsOtherThanReportItemsWritten_ShouldThrowMidSpan()
    {
        var sut = CreateTestArray();
        sut.GetNextWritableSpan();

        ((Action)(() => sut.Add(1))).ShouldThrow<InvalidOperationException>();
        ((Action)(() => sut.AddRange([1]))).ShouldThrow<InvalidOperationException>();
        ((Action)(() => sut.Drop(0))).ShouldThrow<InvalidOperationException>();
        ((Action)(() => sut.GetNextWritableSpan())).ShouldThrow<InvalidOperationException>();
    }

    [TestMethod]
    public void ReportItemsWritten_ShouldEndTheSpanOperation()
    {
        var sut = CreateTestArray();
        sut.GetNextWritableSpan();
        sut.ReportItemsWritten(0);

        ((Action)(() => sut.Add(1))).ShouldNotThrow();
    }

    [TestMethod]
    public void ReportItemsWritten_WithoutASpan_ShouldThrow()
    {
        ((Action)(() => CreateTestArray().ReportItemsWritten(0)))
            .ShouldThrow<InvalidOperationException>();
    }

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
        var sut = CreateTestArray();
        sut.AddRange([6, 2, 8, 1]);
        sut.ShouldBe([2, 8, 1]);
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
    public void Indexer_ShouldReadThroughWrapAround()
    {
        var sut = CreateTestArray();
        sut.AddRange([1, 2, 3, 4]);
        sut[0].ShouldBe(2);
        sut[1].ShouldBe(3);
        sut[2].ShouldBe(4);
    }

    [TestMethod]
    public void Indexer_ShouldThrowBeyondLength()
    {
        var sut = CreateTestArray();
        sut.Add(1);

        ((Action)(() => _ = sut[1])).ShouldThrow<IndexOutOfRangeException>();
        ((Action)(() => _ = sut[-1])).ShouldThrow<IndexOutOfRangeException>();
    }

    [TestMethod]
    public void ReadingMidSpan_ShouldThrow()
    {
        var sut = CreateTestArray();
        sut.Add(1);
        sut.GetNextWritableSpan();

        ((Action)(() => _ = sut[0])).ShouldThrow<InvalidOperationException>();
        ((Action)(() => sut.GetNextUsedSpan())).ShouldThrow<InvalidOperationException>();
        ((Action)(() => sut.ToList())).ShouldThrow<InvalidOperationException>();
    }

    [TestMethod]
    [DataRow(new[]{1,2,3,4}, 2, new[]{ 5,6 }, 0, new []{4,5,6})]
    [DataRow(new[]{1,2,3,4}, 3, new[]{ 5,6 }, 1, new []{5,6})]
    [DataRow(new int[0], 0, new int[0], 3, new int[0])]
    [DataRow(new[]{1}, 0, new int[0], 2, new []{1})]
    [DataRow(new[]{1, 2}, 0, new int[0], 1, new []{1, 2})]
    [DataRow(new[]{1,2,3}, 0, new int[0], 0, new []{1,2,3})]
    [DataRow(new[]{1,2,3,4}, 0, new int[0], 0, new []{2,3,4})]
    [DataRow(new[]{1,2,3,4,5}, 0, new int[0], 0, new []{3,4, 5})]
    [DataRow(new[]{1,2,3,4,5,6}, 0, new int[0], 0, new []{4, 5, 6})]
    [DataRow(new[]{1}, 1, new int[0], 2, new int[0])]
    [DataRow(new[]{1, 2}, 2, new int[0], 1, new int[0])]
    [DataRow(new[]{1,2,3}, 3, new int[0], 3, new int[0])]
    [DataRow(new[]{1,2,3,4}, 3, new int[0], 2, new int[0])]
    [DataRow(new[]{1,2,3,4,5}, 3, new int[0], 1, new int[0])]
    [DataRow(new[]{1,2,3,4,5,6}, 3, new int[0], 3, new int[0])]
    public void WritableSpan_ShouldRepresentSpaceLeftBeforeWrapAround(
        int[] itemsToInsert,
        int itemCountToDrop,
        int[] itemsToWriteToSpan,
        int expectedLengthOfLastWritableSpan,
        int[] expectedFinalArray)
    {
        var sut = CreateTestArray();
        sut.AddRange(itemsToInsert);
        sut.Drop(itemCountToDrop);
        itemsToWriteToSpan.CopyTo(sut.GetNextWritableSpan());
        sut.ReportItemsWritten(itemsToWriteToSpan.Length);

        sut.GetNextWritableSpan().Length.ShouldBe(expectedLengthOfLastWritableSpan);
        sut.ReportItemsWritten(0);
        sut.ShouldBe(expectedFinalArray);
    }

    [TestMethod]
    [DataRow(new int[] {}, 0, new int [] {})]
    [DataRow(new[] {1,}, 0, new [] {1})]
    [DataRow(new[] {1,2}, 0, new [] {1,2})]
    [DataRow(new[] {1,2,3}, 0, new [] {1,2,3})]
    [DataRow(new[] {1,2,3,4}, 0, new [] {2, 3})]
    [DataRow(new[] {1,2,3,4,5}, 0, new [] {3})]
    [DataRow(new[] {1,2,3,4,5,6}, 0, new [] {4,5,6})]
    [DataRow(new[] {1,}, 1, new int[] {})]
    [DataRow(new[] {1,2}, 1, new [] {2})]
    [DataRow(new[] {1,2,3}, 2, new [] {3})]
    [DataRow(new[] {1,2,3,4}, 2, new [] {4})]
    [DataRow(new[] {1,2,3,4,5}, 1, new [] {4, 5})]
    public void UsedSpan_ShouldRepresentItemsBeforeAWrapAround(
        int[] itemsToInsert,
        int dropCount,
        int[] expectedUsedSpan)
    {
        var sut = CreateTestArray();
        sut.AddRange(itemsToInsert);
        sut.Drop(dropCount);
        sut.GetNextUsedSpan().ToArray().ShouldBe(expectedUsedSpan);
    }
}
