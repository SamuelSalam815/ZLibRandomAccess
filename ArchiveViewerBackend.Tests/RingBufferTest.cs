using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(RingBuffer<>))]
public class RingBufferTest
{
    [TestMethod]
    [DataRow(6, "", "abc", "abc")]
    [DataRow(6, "abc", "123", "abc123")]
    [DataRow(6, "abc", "", "abc")]
    [DataRow(4, "abcd", "12", "cd12")]
    [DataRow(3, "a", "12345", "345")]
    [DataRow(3, "abc", "12345", "345")]
    public void Append_AddsBlockAfterExistingItems(int capacity, string initial, string block, string expected)
    {
        var buffer = BufferWith(capacity, initial);

        buffer.Append(block);

        buffer.ShouldEqualText(expected);
    }

    [TestMethod]
    [DataRow(6, "", "abc", "abc")]
    [DataRow(6, "abc", "123", "123abc")]
    [DataRow(6, "abc", "", "abc")]
    [DataRow(4, "abcd", "12", "12ab")]
    [DataRow(3, "a", "12345", "123")]
    [DataRow(3, "abc", "12345", "123")]
    public void Prepend_AddsBlockBeforeExistingItems(int capacity, string initial, string block, string expected)
    {
        var buffer = BufferWith(capacity, initial);

        buffer.Prepend(block);

        buffer.ShouldEqualText(expected);
    }

    [TestMethod]
    [DataRow("abcdef", 0, "abcdef")]
    [DataRow("abcdef", 2, "cdef")]
    [DataRow("abcdef", 6, "")]
    public void DropFirst_RemovesLeadingItems(string initial, int count, string expected)
    {
        var buffer = BufferWith(6, initial);

        buffer.DropFirst(count);

        buffer.ShouldEqualText(expected);
    }

    [TestMethod]
    [DataRow("abcdef", 0, "abcdef")]
    [DataRow("abcdef", 2, "abcd")]
    [DataRow("abcdef", 6, "")]
    public void DropLast_RemovesTrailingItems(string initial, int count, string expected)
    {
        var buffer = BufferWith(6, initial);

        buffer.DropLast(count);

        buffer.ShouldEqualText(expected);
    }

    [TestMethod]
    public void Append_AfterDropFirst_WrapsAround()
    {
        var buffer = BufferWith(4, "abc");
        buffer.DropFirst(2);

        buffer.Append("123");

        buffer.ShouldEqualText("c123");
        buffer.Contents.Second.IsEmpty.ShouldBeFalse();
    }

    [TestMethod]
    public void Prepend_AfterDropLast_WrapsAround()
    {
        var buffer = BufferWith(4, "abc");
        buffer.DropLast(2);

        buffer.Prepend("123");

        buffer.ShouldEqualText("123a");
        buffer.Contents.Second.IsEmpty.ShouldBeFalse();
    }

    [TestMethod]
    public void Prepend_ThenAppend_EvictsFromStart()
    {
        var buffer = BufferWith(6, "abc");

        buffer.Prepend("12");
        buffer.Append("XY");

        buffer.ShouldEqualText("2abcXY");
    }

    [TestMethod]
    public void DropFirst_ThenAppend_ThenPrepend_EvictsFromEnd()
    {
        var buffer = BufferWith(5, "abcd");

        buffer.DropFirst(3);
        buffer.Append("123");
        buffer.Prepend("XY");

        buffer.ShouldEqualText("XYd12");
    }

    [TestMethod]
    public void DropEverything_ThenReuse_Works()
    {
        var buffer = BufferWith(3, "abc");

        buffer.DropFirst(1);
        buffer.DropLast(2);
        buffer.ShouldEqualText("");
        buffer.Prepend("12");
        buffer.Append("34");

        buffer.ShouldEqualText("234");
    }

    [TestMethod]
    public void RepeatedlyDroppingAndAppending_SlidesAcrossTheBackingArray()
    {
        var buffer = BufferWith(4, "abcd");

        foreach (var next in "efghijklmn")
        {
            buffer.DropFirst(1);
            buffer.Append(next.ToString());
        }

        buffer.ShouldEqualText("klmn");
    }

    [TestMethod]
    public void DropFirst_MoreThanLength_Throws()
    {
        var buffer = BufferWith(4, "ab");

        Should.Throw<ArgumentOutOfRangeException>(() => buffer.DropFirst(3));
        Should.Throw<ArgumentOutOfRangeException>(() => buffer.DropFirst(-1));
    }

    [TestMethod]
    public void DropLast_MoreThanLength_Throws()
    {
        var buffer = BufferWith(4, "ab");

        Should.Throw<ArgumentOutOfRangeException>(() => buffer.DropLast(3));
        Should.Throw<ArgumentOutOfRangeException>(() => buffer.DropLast(-1));
    }

    [TestMethod]
    public void Drop_OnEmptyBuffer_Throws()
    {
        var buffer = BufferWith(4, "");

        Should.Throw<ArgumentOutOfRangeException>(() => buffer.DropFirst(1));
        Should.Throw<ArgumentOutOfRangeException>(() => buffer.DropLast(1));
    }

    [TestMethod]
    public void Drop_Zero_OnEmptyBuffer_DoesNothing()
    {
        var buffer = BufferWith(4, "");

        buffer.DropFirst(0);
        buffer.DropLast(0);

        buffer.ShouldEqualText("");
    }

    [TestMethod]
    public void Clear_EmptiesTheBuffer_AndAllowsReuse()
    {
        var buffer = BufferWith(4, "abc");
        buffer.DropFirst(2);

        buffer.Clear();
        buffer.ShouldEqualText("");
        buffer.Append("1234");

        buffer.ShouldEqualText("1234");
    }

    [TestMethod]
    public void Clear_OnEmptyBuffer_DoesNothing()
    {
        var buffer = BufferWith(4, "");

        buffer.Clear();

        buffer.ShouldEqualText("");
    }

    [TestMethod]
    public void ReportWritten_AddsItemsWrittenIntoTheWritableSpan()
    {
        var buffer = BufferWith(6, "abc");

        "123".CopyTo(buffer.GetWritableSpan());
        buffer.ReportWritten(3);

        buffer.ShouldEqualText("abc123");
    }

    [TestMethod]
    public void ReportWritten_FewerThanSpanLength_AddsOnlyThose()
    {
        var buffer = BufferWith(6, "abc");

        "12".CopyTo(buffer.GetWritableSpan());
        buffer.ReportWritten(1);

        buffer.ShouldEqualText("abc1");
    }

    [TestMethod]
    public void GetWritableSpan_ExtendsToTheEndOfTheBackingArray_ThenWraps()
    {
        var buffer = BufferWith(4, "abc");
        buffer.DropFirst(2);

        buffer.GetWritableSpan().Length.ShouldBe(1);
        "1".CopyTo(buffer.GetWritableSpan());
        buffer.ReportWritten(1);
        buffer.GetWritableSpan().Length.ShouldBe(2);
        "23".CopyTo(buffer.GetWritableSpan());
        buffer.ReportWritten(2);

        buffer.ShouldEqualText("c123");
        buffer.GetWritableSpan().IsEmpty.ShouldBeTrue();
    }

    [TestMethod]
    public void ReportWritten_MoreThanSpanLength_Throws()
    {
        var buffer = BufferWith(4, "ab");

        Should.Throw<ArgumentOutOfRangeException>(() => buffer.ReportWritten(3));
        Should.Throw<ArgumentOutOfRangeException>(() => buffer.ReportWritten(-1));
    }

    [TestMethod]
    public void Indexer_OutOfRange_Throws()
    {
        var buffer = BufferWith(4, "ab");

        Should.Throw<IndexOutOfRangeException>(() => buffer[2]);
        Should.Throw<IndexOutOfRangeException>(() => buffer[-1]);
    }

    private static RingBuffer<char> BufferWith(int capacity, string initial)
    {
        var buffer = new RingBuffer<char>(capacity);
        buffer.Append(initial);
        return buffer;
    }
}

internal static class RingBufferTestExtensions
{
    /// <summary>Asserts that every way of reading the buffer yields <paramref name="expected"/>.</summary>
    public static void ShouldEqualText(this RingBuffer<char> buffer, string expected)
    {
        buffer.Length.ShouldBe(expected.Length);
        new string(buffer.ToArray()).ShouldBe(expected);
        new string(Enumerable.Range(0, buffer.Length).Select(i => buffer[i]).ToArray()).ShouldBe(expected);
        var contents = new char[buffer.Length];
        buffer.Contents.CopyTo(contents);
        new string(contents).ShouldBe(expected);
    }
}
