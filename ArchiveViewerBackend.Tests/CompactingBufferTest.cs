using System;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(CompactingBuffer<>))]
public class CompactingBufferTest
{
    private static CompactingBuffer<char> BufferWith(int capacity, string initial)
    {
        var buffer = new CompactingBuffer<char>(capacity);
        Write(buffer, initial);
        return buffer;
    }

    private static void Write(CompactingBuffer<char> buffer, string text)
    {
        text.AsSpan().CopyTo(buffer.GetWritableSpan(text.Length));
        buffer.ReportWritten(text.Length);
    }

    private static string TextOf(CompactingBuffer<char> buffer) => new(buffer.Contents);

    [TestMethod]
    public void Contents_AfterWrites_ReturnsItemsInOrder()
    {
        var sut = BufferWith(10, "abc");

        Write(sut, "de");

        TextOf(sut).ShouldBe("abcde");
        sut.Length.ShouldBe(5);
    }

    [TestMethod]
    public void GetWritableSpan_WithoutMinimum_ReturnsTheWholeRemainingCapacity()
    {
        var sut = BufferWith(10, "abcdef");
        sut.DropFirst(4);

        var writable = sut.GetWritableSpan();

        writable.Length.ShouldBe(8);
        TextOf(sut).ShouldBe("ef");
    }

    [TestMethod]
    public void GetWritableSpan_WhenTheTailIsLongEnough_DoesNotMoveItems()
    {
        var sut = BufferWith(10, "abcdef");
        sut.DropFirst(2);

        // Free space is 6 cells in total but only 4 of them are after the last item.
        sut.GetWritableSpan(4).Length.ShouldBe(4);
    }

    [TestMethod]
    public void GetWritableSpan_WhenTheTailIsTooShort_MovesItemsAndKeepsThem()
    {
        var sut = BufferWith(10, "abcdef");
        sut.DropFirst(2);

        var writable = sut.GetWritableSpan(5);

        writable.Length.ShouldBe(6);
        TextOf(sut).ShouldBe("cdef");
    }

    [TestMethod]
    public void GetWritableSpan_AfterDroppingEverything_ReturnsTheWholeCapacity()
    {
        var sut = BufferWith(10, "abcdefgh");
        sut.DropFirst(8);

        sut.GetWritableSpan().Length.ShouldBe(10);
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(5)]
    public void GetWritableSpan_MinimumOutOfRange_Throws(int minimumLength)
    {
        var sut = BufferWith(10, "abcdef");

        Should.Throw<ArgumentOutOfRangeException>(() => sut.GetWritableSpan(minimumLength));
    }

    [TestMethod]
    public void GetWritableSpan_MinimumEqualToRemainingCapacity_ReturnsIt()
    {
        var sut = BufferWith(10, "abcdef");

        sut.GetWritableSpan(4).Length.ShouldBe(4);
    }

    [TestMethod]
    public void WritesAfterMoving_KeepEarlierItemsFirst()
    {
        var sut = BufferWith(6, "abcdef");
        sut.DropFirst(4);

        Write(sut, "ghij");

        TextOf(sut).ShouldBe("efghij");
    }

    [TestMethod]
    public void ReportWritten_OutOfRange_Throws()
    {
        var sut = BufferWith(10, "abcdef");

        Should.Throw<ArgumentOutOfRangeException>(() => sut.ReportWritten(5));
        Should.Throw<ArgumentOutOfRangeException>(() => sut.ReportWritten(-1));
    }

    [TestMethod]
    public void DropFirst_RemovesFromTheStart()
    {
        var sut = BufferWith(10, "abcdef");

        sut.DropFirst(2);

        TextOf(sut).ShouldBe("cdef");
        sut.Length.ShouldBe(4);
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(7)]
    public void DropFirst_OutOfRange_Throws(int count)
    {
        var sut = BufferWith(10, "abcdef");

        Should.Throw<ArgumentOutOfRangeException>(() => sut.DropFirst(count));
    }

    [TestMethod]
    public void Clear_EmptiesTheBuffer()
    {
        var sut = BufferWith(10, "abcdef");
        sut.DropFirst(2);

        sut.Clear();

        sut.Length.ShouldBe(0);
        sut.GetWritableSpan().Length.ShouldBe(10);
    }

    [TestMethod]
    public void Foreach_OverContents_DoesNotAllocate()
    {
        static int Count(CompactingBuffer<char> buffer)
        {
            var count = 0;
            foreach (var _ in buffer.Contents)
            {
                count++;
            }

            return count;
        }

        var sut = BufferWith(10, "abcdef");
        Count(sut); // Warm up so that JIT work is not counted.
        var before = GC.GetAllocatedBytesForCurrentThread();

        var count = Count(sut);

        GC.GetAllocatedBytesForCurrentThread().ShouldBe(before);
        count.ShouldBe(6);
    }
}
