using System;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(CircularArrayV2<>))]
public class CircularArrayV2Test
{
    private static CircularArrayV2<int> CreateTestArray() => new(3);

    [TestMethod]
    public void Capacity_ShouldGetPassedThrough()
    {
        new CircularArrayV2<int>(1337).Capacity.ShouldBe(1337);
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
        sut.Length.ShouldBe(3);
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
}
