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

    [TestMethod]
    public void ObservingTheByteLimit_PreventsCallsToReadFromInnerStream()
    {
        var byteLimit = 100;
        var innerStream = new StreamWithReadCount(new MemoryStream(new byte[byteLimit]));
        var sut = new StreamWithByteLimit(innerStream, byteLimit);
        sut.ReadExactly(new byte[byteLimit]);
        sut.Read(new byte[byteLimit]).ShouldBe(0);
        innerStream.ReadCount.ShouldBe(1);
    }

    private class StreamWithReadCount(Stream innerStream) : Stream
    {
        public int ReadCount { get; private set; }
        public override void Flush()
        {
            innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ReadCount++;
            return innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            innerStream.Write(buffer, offset, count);
        }

        public override bool CanRead => innerStream.CanRead;
        public override bool CanSeek => innerStream.CanSeek;
        public override bool CanWrite => innerStream.CanWrite;
        public override long Length => innerStream.Length;
        public override long Position
        {
            get => innerStream.Position;
            set => innerStream.Position = value;
        }
    }
}
