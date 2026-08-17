using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(StreamWithByteLimit))]
public class StreamWithByteLimitTest
{

    [TestMethod]
    [DataRow(-1)]
    [DataRow(-5)]
    [DataRow(-10)]
    public void ProvidingANegativeLimit_ThrowsException(long limit)
    {
        var action = () => new StreamWithByteLimit(new MemoryStream(), limit);
        action.ShouldThrow<Exception>();
    }

    [TestMethod]
    [DataRow(new byte[]{0,1,2,3,4,5}, 3, new byte[]{0,1,2})]
    [DataRow(new byte[]{0,1,2,3,4,5}, 100, new byte[]{0,1,2,3,4,5})]
    public void ProvidedLimit_ShouldLimitNumberOfBytesRead(
        byte[] bytesToRead,
        long byteLimit,
        byte[] expectedReadBytes
        )
    {
        var stream = new MemoryStream(bytesToRead);
        var streamWithByteLimit = new StreamWithByteLimit(stream, byteLimit);
        streamWithByteLimit.Length.ShouldBe(expectedReadBytes.Length);
        var actualBytesRead = new List<byte>();
        while (true)
        {
            var nextByte = streamWithByteLimit.ReadByte();
            if (nextByte == -1)
            {
                break;
            }

            actualBytesRead.Add((byte)nextByte);
        }
        actualBytesRead.ShouldBe(expectedReadBytes);
    }
}
