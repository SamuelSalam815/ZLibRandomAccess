using System;
using System.IO;
using System.Linq;
using System.Text;
using ArchiveViewerBackend.TextSearching;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(ParallelScanTextSearcher))]
public class ParallelScanTextSearcherTest
{

    [TestMethod]
    public void Dispose_DisposesAllParallelStreams()
    {
        var parallelStreams =
            Enumerable
                .Range(0, 10)
                .Select(_ => new MemoryStream(Encoding.Default.GetBytes("Hello, World!")))
                .ToList();

        var sut = new ParallelScanTextSearcher(
            parallelStreams.Select((s, i) => s.WithByteOffset<Stream>(i)),
            Encoding.Default);

        sut.Dispose();

        foreach (var stream in parallelStreams)
        {
            Action readAttempt = () => stream.ReadByte();
            readAttempt.ShouldThrow<Exception>();
        }
    }
}

[TestClass]
[TestSubject(typeof(ScanTextSearcher))]
public class ScanTextSearcherTest
{
    [TestMethod]
    public void Dispose_DisposesInnerStream()
    {
        var innerStream = new MemoryStream([1, 2, 3]);
        var sut = new ScanTextSearcher(innerStream, Encoding.Default);
        sut.Dispose();
        Action readAttempt = () => innerStream.ReadByte();
        readAttempt.ShouldThrow<Exception>();
    }

    [TestMethod]
    public void DisposeDoesNotCloseInnerStream_WhenLeaveOpenIsTrue()
    {
        var innerStream = new MemoryStream([1, 2, 3]);
        var sut = new ScanTextSearcher(innerStream, Encoding.Default, leaveOpen: true);
        sut.Dispose();
        Action readAttempt = () => innerStream.ReadByte();
        readAttempt.ShouldNotThrow();
    }
}
