using System;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveAccessPointVisualizer.Tests;

[TestClass]
[TestSubject(typeof(DataSize))]
public class DataSizeTest
{
    [TestMethod]
    [DataRow(1, 1024 * 1024)]
    [DataRow(2, 2 * 1024 * 1024)]
    [DataRow(15, 15 * 1024 * 1024)]
    public void FromMegaBytes_CreatesExpectedSize(long megaByteCount, long expectedSize)
    {
        DataSize.FromMegaBytes(megaByteCount).ByteCount.ShouldBe(expectedSize);
    }

    [TestMethod]
    [DataRow(1, 1024 * 1024 * 1024)]
    [DataRow(2, 2 * 1024 * 1024 * 1024L)]
    [DataRow(15, 15 * 1024 * 1024 * 1024L)]
    public void FromGigaBytes_CreatesExpectedSize(long megaByteCount, long expectedSize)
    {
        DataSize.FromGigaBytes(megaByteCount).ByteCount.ShouldBe(expectedSize);
    }

    [TestMethod]
    public void ArithmeticOverflow_InFromMegaBytesThrows()
    {
        var action = () => DataSize.FromMegaBytes(1_000_000_000_000L * 1024 * 1024L);
        action.ShouldThrow<OverflowException>();
    }

    [TestMethod]
    public void ArithmeticOverflow_InFromGigaBytesThrows()
    {
        var action = () => DataSize.FromGigaBytes(1_000_000_000_000L * 1024 * 1024L);
        action.ShouldThrow<OverflowException>();
    }

    [TestMethod]
    public void PassingNegativeNumber_ToFromMegaBytesThrows()
    {
        var action = () => DataSize.FromMegaBytes(-2);
        action.ShouldThrow<ArgumentException>();
    }

    [TestMethod]
    public void PassingNegativeNumber_ToFromGigaBytesThrows()
    {
        var action = () => DataSize.FromGigaBytes(-2);
        action.ShouldThrow<ArgumentException>();
    }
}
