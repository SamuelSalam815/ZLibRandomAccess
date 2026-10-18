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

        buffer.ShouldContain(expected);
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

        buffer.ShouldContain(expected);
    }

    [TestMethod]
    [DataRow("abcdef", 0, "abcdef")]
    [DataRow("abcdef", 2, "cdef")]
    [DataRow("abcdef", 6, "")]
    public void DropFromStart_RemovesLeadingItems(string initial, int count, string expected)
    {
        var buffer = BufferWith(6, initial);

        buffer.DropFromStart(count);

        buffer.ShouldContain(expected);
    }

    [TestMethod]
    [DataRow("abcdef", 0, "abcdef")]
    [DataRow("abcdef", 2, "abcd")]
    [DataRow("abcdef", 6, "")]
    public void DropFromEnd_RemovesTrailingItems(string initial, int count, string expected)
    {
        var buffer = BufferWith(6, initial);

        buffer.DropFromEnd(count);

        buffer.ShouldContain(expected);
    }

    [TestMethod]
    public void Append_AfterDropFromStart_WrapsAround()
    {
        var buffer = BufferWith(4, "abc");
        buffer.DropFromStart(2);

        buffer.Append("123");

        buffer.ShouldContain("c123");
        buffer.SecondSegment.IsEmpty.ShouldBeFalse();
    }

    [TestMethod]
    public void Prepend_AfterDropFromEnd_WrapsAround()
    {
        var buffer = BufferWith(4, "abc");
        buffer.DropFromEnd(2);

        buffer.Prepend("123");

        buffer.ShouldContain("123a");
        buffer.SecondSegment.IsEmpty.ShouldBeFalse();
    }

    [TestMethod]
    public void Prepend_ThenAppend_EvictsFromStart()
    {
        var buffer = BufferWith(6, "abc");

        buffer.Prepend("12");
        buffer.Append("XY");

        buffer.ShouldContain("2abcXY");
    }

    [TestMethod]
    public void DropFromStart_ThenAppend_ThenPrepend_EvictsFromEnd()
    {
        var buffer = BufferWith(5, "abcd");

        buffer.DropFromStart(3);
        buffer.Append("123");
        buffer.Prepend("XY");

        buffer.ShouldContain("XYd12");
    }

    [TestMethod]
    public void DropEverything_ThenReuse_Works()
    {
        var buffer = BufferWith(3, "abc");

        buffer.DropFromStart(1);
        buffer.DropFromEnd(2);
        buffer.ShouldContain("");
        buffer.Prepend("12");
        buffer.Append("34");

        buffer.ShouldContain("234");
    }

    [TestMethod]
    public void RepeatedlyDroppingAndAppending_SlidesAcrossTheBackingArray()
    {
        var buffer = BufferWith(4, "abcd");

        foreach (var next in "efghijklmn")
        {
            buffer.DropFromStart(1);
            buffer.Append(next.ToString());
        }

        buffer.ShouldContain("klmn");
    }

    [TestMethod]
    public void DropFromStart_MoreThanLength_Throws()
    {
        var buffer = BufferWith(4, "ab");

        Should.Throw<ArgumentOutOfRangeException>(() => buffer.DropFromStart(3));
        Should.Throw<ArgumentOutOfRangeException>(() => buffer.DropFromStart(-1));
    }

    [TestMethod]
    public void DropFromEnd_MoreThanLength_Throws()
    {
        var buffer = BufferWith(4, "ab");

        Should.Throw<ArgumentOutOfRangeException>(() => buffer.DropFromEnd(3));
        Should.Throw<ArgumentOutOfRangeException>(() => buffer.DropFromEnd(-1));
    }

    [TestMethod]
    public void Drop_OnEmptyBuffer_Throws()
    {
        var buffer = BufferWith(4, "");

        Should.Throw<InvalidOperationException>(() => buffer.DropFromStart(1));
        Should.Throw<InvalidOperationException>(() => buffer.DropFromEnd(1));
    }

    [TestMethod]
    public void Drop_Zero_OnEmptyBuffer_DoesNothing()
    {
        var buffer = BufferWith(4, "");

        buffer.DropFromStart(0);
        buffer.DropFromEnd(0);

        buffer.ShouldContain("");
    }

    [TestMethod]
    public void Indexer_OutOfRange_Throws()
    {
        var buffer = BufferWith(4, "ab");

        Should.Throw<IndexOutOfRangeException>(() => buffer[2]);
        Should.Throw<IndexOutOfRangeException>(() => buffer[-1]);
    }

    [TestMethod]
    public void EveryShortOperationSequence_MatchesStringModel()
    {
        (string Kind, int Size)[] operations =
        [
            ("Append", 1), ("Append", 3), ("Prepend", 1), ("Prepend", 3),
            ("DropStart", 1), ("DropStart", 2), ("DropEnd", 1), ("DropEnd", 2),
        ];
        const int sequenceLength = 4;
        var sequenceCount = (int)Math.Pow(operations.Length, sequenceLength);

        for (var capacity = 1; capacity <= 4; capacity++)
        {
            for (var sequence = 0; sequence < sequenceCount; sequence++)
            {
                var buffer = new RingBuffer<char>(capacity);
                var model = "";
                var nextChar = 'a';

                for (var step = 0; step < sequenceLength; step++)
                {
                    var (kind, size) = operations[sequence / (int)Math.Pow(operations.Length, step) % operations.Length];
                    var block = new string(Enumerable.Range(nextChar, size).Select(c => (char)c).ToArray());

                    switch (kind)
                    {
                        case "Append":
                            nextChar += (char)size;
                            buffer.Append(block);
                            model = (model + block)[^Math.Min(capacity, model.Length + size)..];
                            break;
                        case "Prepend":
                            nextChar += (char)size;
                            buffer.Prepend(block);
                            model = (block + model)[..Math.Min(capacity, model.Length + size)];
                            break;
                        case "DropStart" when size <= model.Length:
                            buffer.DropFromStart(size);
                            model = model[size..];
                            break;
                        case "DropEnd" when size <= model.Length:
                            buffer.DropFromEnd(size);
                            model = model[..^size];
                            break;
                    }

                    buffer.ShouldContain(model);
                }
            }
        }
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
    public static void ShouldContain(this RingBuffer<char> buffer, string expected)
    {
        buffer.Length.ShouldBe(expected.Length);
        new string(buffer.ToArray()).ShouldBe(expected);
        new string(Enumerable.Range(0, buffer.Length).Select(i => buffer[i]).ToArray()).ShouldBe(expected);
        (new string(buffer.FirstSegment.Span) + new string(buffer.SecondSegment.Span)).ShouldBe(expected);
    }
}
