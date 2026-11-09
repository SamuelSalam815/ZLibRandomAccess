using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using ZLibWrapper;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(BackwardSeekableStream))]
public class BackwardSeekableStreamTest
{
    private const int DataLength = 50_000;
    private const int RecoveryPointInterval = 8_000;

    private static readonly byte[] Data = CreateData();

    private static byte[] CreateData()
    {
        var data = new byte[DataLength];
        new Random(1).NextBytes(data);
        return data;
    }

    private static byte[] ReadAt(Stream stream, long position, int count)
    {
        stream.Position = position;
        var bytes = new byte[count];
        stream.ReadExactly(bytes);
        return bytes;
    }

    private static MemoryStream CompressWithBcl()
    {
        var compressed = new MemoryStream();
        using (var gzip = new GZipStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
        {
            gzip.Write(Data);
        }

        return compressed;
    }

    private static MemoryStream CompressWithRecoveryPoints(out List<RecoveryPointOffset> recoveryPoints)
    {
        var compressed = new MemoryStream();
        var points = new List<RecoveryPointOffset>();
        using (var writer = new GZipWritingStreamWithRecoveryPoints(compressed, true, RecoveryPointInterval))
        {
            writer.RecoveryPointWritten += points.Add;
            for (var offset = 0; offset < DataLength; offset += 1000)
            {
                writer.Write(Data, offset, 1000);
            }
        }

        recoveryPoints = points;
        return compressed;
    }

    private sealed class DisposeCounter : IDisposable
    {
        public int Count { get; private set; }

        public void Dispose() => Count++;
    }

    private static BackwardSeekableStream<DisposeCounter> Create(
        Func<long, (Stream Stream, long Offset)> restartAtOrBefore,
        DisposeCounter? context = null) =>
        new(context ?? new DisposeCounter(), (_, target) => restartAtOrBefore(target));

    [TestMethod]
    public void Dispose_DisposesTheContextAndTheCurrentStream()
    {
        var context = new DisposeCounter();
        var inner = new MemoryStream(Data);
        var sut = Create(_ => (inner, 0), context);

        sut.Dispose();

        context.Count.ShouldBe(1);
        inner.CanRead.ShouldBeFalse();
    }

    [TestMethod]
    public void Dispose_AfterARestart_DisposesTheReplacedStreamToo()
    {
        var context = new DisposeCounter();
        var streams = new List<MemoryStream>();
        var sut = Create(_ =>
        {
            var stream = new MemoryStream(Data);
            streams.Add(stream);
            return (stream, 0);
        }, context);
        ReadAt(sut, 100, 1);
        ReadAt(sut, 10, 1);

        streams.Count.ShouldBe(2);
        streams[0].CanRead.ShouldBeFalse();
        streams[1].CanRead.ShouldBeTrue();

        sut.Dispose();

        streams[1].CanRead.ShouldBeFalse();
        context.Count.ShouldBe(1);
    }

    [TestMethod]
    public void ForGZip_Dispose_DisposesTheCompressedStream()
    {
        var compressed = CompressWithBcl();

        BackwardSeekableStream.ForGZip(compressed).Dispose();

        compressed.CanRead.ShouldBeFalse();
    }

    [TestMethod]
    public void ForGZipWithRecoveryPoints_Dispose_DisposesTheCompressedStream()
    {
        var compressed = CompressWithRecoveryPoints(out var recoveryPoints);
        var sut = BackwardSeekableStream.ForGZipWithRecoveryPoints(compressed, recoveryPoints);
        ReadAt(sut, 48_000, 1);
        ReadAt(sut, 100, 1);

        sut.Dispose();

        compressed.CanRead.ShouldBeFalse();
    }

    [TestMethod]
    public void Seek_Forwards_DoesNotRestart()
    {
        var restartTargets = new List<long>();
        using var sut = Create(target =>
        {
            restartTargets.Add(target);
            return (new MemoryStream(Data), 0);
        });

        ReadAt(sut, 30_000, 10).ShouldBe(Data[30_000..30_010]);

        restartTargets.ShouldBe([0]);
    }

    [TestMethod]
    public void Seek_Backwards_RestartsAtTargetAndContinuesFromTheReturnedOffset()
    {
        const long restartOffset = 20_000;
        var restartTargets = new List<long>();
        using var sut = Create(target =>
        {
            restartTargets.Add(target);
            var offset = target == 0 ? 0 : restartOffset;
            return (new MemoryStream(Data) { Position = offset }, offset);
        });
        ReadAt(sut, 40_000, 1);

        ReadAt(sut, 25_000, 10).ShouldBe(Data[25_000..25_010]);

        restartTargets.ShouldBe([0, 25_000]);
    }

    [TestMethod]
    public void Seek_PastTheEnd_StopsAtTheEnd()
    {
        using var sut = Create(_ => (new MemoryStream(Data), 0));

        sut.Seek(DataLength + 100, SeekOrigin.Begin).ShouldBe(DataLength);
    }

    [TestMethod]
    public void Seek_BeforeTheStart_Throws()
    {
        using var sut = Create(_ => (new MemoryStream(Data), 0));

        Should.Throw<ArgumentOutOfRangeException>(() => sut.Seek(-1, SeekOrigin.Begin));
    }

    [TestMethod]
    public void Seek_FromEnd_IsNotSupported()
    {
        using var sut = Create(_ => (new MemoryStream(Data), 0));

        Should.Throw<NotSupportedException>(() => sut.Seek(-1, SeekOrigin.End));
    }

    [TestMethod]
    [DataRow(45_000, 100)]
    [DataRow(10_000, 100)]
    [DataRow(7_999, 100)]
    [DataRow(0, 100)]
    [DataRow(49_900, 100)]
    public void ForGZip_SeekingBackwards_ReadsTheRequestedBytes(int position, int count)
    {
        var compressed = CompressWithBcl();
        using var sut = BackwardSeekableStream.ForGZip(compressed);
        ReadAt(sut, 48_000, 1);

        ReadAt(sut, position, count).ShouldBe(Data[position..(position + count)]);
    }

    [TestMethod]
    [DataRow(45_000, 100)]
    [DataRow(16_000, 100)]
    [DataRow(15_999, 100)]
    [DataRow(7_999, 100)]
    [DataRow(0, 100)]
    [DataRow(49_900, 100)]
    public void ForGZipWithRecoveryPoints_SeekingBackwards_ReadsTheRequestedBytes(int position, int count)
    {
        var compressed = CompressWithRecoveryPoints(out var recoveryPoints);
        recoveryPoints.Count.ShouldBeGreaterThan(3);
        using var sut = BackwardSeekableStream.ForGZipWithRecoveryPoints(compressed, recoveryPoints);
        ReadAt(sut, 48_000, 1);

        ReadAt(sut, position, count).ShouldBe(Data[position..(position + count)]);
    }
}
