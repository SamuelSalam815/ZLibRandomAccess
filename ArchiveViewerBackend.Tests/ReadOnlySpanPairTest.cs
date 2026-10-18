using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(ReadOnlySpanPair<>))]
public class ReadOnlySpanPairTest
{
    // The text "abcde", split after "ab".
    private static ReadOnlySpanPair<char> Abcde() => new("ab", "cde");

    private static string ToText(ReadOnlySpanPair<char> pair)
    {
        var chars = new char[pair.Length];
        pair.CopyTo(chars);
        return new string(chars);
    }

    [TestMethod]
    public void Length_IsSumOfBothSpans()
    {
        Abcde().Length.ShouldBe(5);
    }

    [TestMethod]
    public void IsEmpty_WhenBothSpansAreEmpty()
    {
        new ReadOnlySpanPair<char>().IsEmpty.ShouldBeTrue();
        Abcde().IsEmpty.ShouldBeFalse();
    }

    [TestMethod]
    public void Indexer_ReadsAcrossTheSeam()
    {
        var pair = Abcde();

        new string([pair[0], pair[1], pair[2], pair[3], pair[4]]).ShouldBe("abcde");
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(5)]
    public void Indexer_OutOfRange_Throws(int index)
    {
        Should.Throw<IndexOutOfRangeException>(() => Abcde()[index]);
    }

    [TestMethod]
    [DataRow(0, 5, "abcde")]
    [DataRow(1, 1, "b")]
    [DataRow(1, 3, "bcd")]
    [DataRow(2, 2, "cd")]
    [DataRow(3, 2, "de")]
    [DataRow(0, 0, "")]
    [DataRow(5, 0, "")]
    public void Slice_ReturnsRequestedItems(int start, int length, string expected)
    {
        ToText(Abcde().Slice(start, length)).ShouldBe(expected);
    }

    [TestMethod]
    public void Slice_WithoutLength_RunsToTheEnd()
    {
        ToText(Abcde().Slice(1)).ShouldBe("bcde");
    }

    [TestMethod]
    [DataRow(-1, 1)]
    [DataRow(0, 6)]
    [DataRow(3, 3)]
    [DataRow(6, 0)]
    [DataRow(0, -1)]
    public void Slice_OutOfRange_Throws(int start, int length)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Abcde().Slice(start, length));
    }

    [TestMethod]
    public void CopyTo_CopiesBothSpansInOrder()
    {
        var destination = new char[6];

        Abcde().CopyTo(destination);

        new string(destination).ShouldBe("abcde\0");
    }

    [TestMethod]
    public void CopyTo_DestinationTooSmall_Throws()
    {
        Should.Throw<ArgumentException>(() => Abcde().CopyTo(new char[4]));
    }

    [TestMethod]
    public void Enumeration_YieldsItemsInOrder()
    {
        var items = new List<char>();

        foreach (var item in Abcde())
        {
            items.Add(item);
        }

        new string(items.ToArray()).ShouldBe("abcde");
    }

    [TestMethod]
    [DataRow("", "", "")]
    [DataRow("", "xy", "xy")]
    [DataRow("xy", "", "xy")]
    [DataRow("x", "y", "xy")]
    public void Enumeration_HandlesEmptySpans(string first, string second, string expected)
    {
        var items = new List<char>();

        foreach (var item in new ReadOnlySpanPair<char>(first, second))
        {
            items.Add(item);
        }

        new string(items.ToArray()).ShouldBe(expected);
    }

    [TestMethod]
    public void Enumeration_DoesNotAllocate()
    {
        static int Count(ReadOnlySpanPair<char> pair)
        {
            var count = 0;
            foreach (var _ in pair)
            {
                count++;
            }

            return count;
        }

        Count(Abcde()); // Warm up so that JIT work is not counted.
        var before = GC.GetAllocatedBytesForCurrentThread();

        var count = Count(Abcde());

        GC.GetAllocatedBytesForCurrentThread().ShouldBe(before);
        count.ShouldBe(5);
    }
}
