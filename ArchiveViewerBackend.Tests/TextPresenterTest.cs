using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using ZLibWrapper;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(TextPresenter))]
public class TextPresenterTest
{
    private const int LineTotal = 1000;
    private const int RenderedLines = 10;
    private const int BufferedLines = 40;
    private const int BufferCapacity = 5_000;
    private const int RecoveryPointInterval = 2_000;

    private static readonly string[] AllLines = Enumerable.Range(0, LineTotal).Select(i => $"line {i}").ToArray();

    public static IEnumerable<object[]> Encodings()
    {
        yield return [Encoding.UTF8];
        yield return [Encoding.Unicode];
    }

    /// <summary>A stream that counts how often its position is set, which happens once per buffer reload.</summary>
    private sealed class PositionCountingStream(byte[] data) : MemoryStream(data)
    {
        public int PositionSetCount { get; private set; }

        public override long Position
        {
            get => base.Position;
            set
            {
                PositionSetCount++;
                base.Position = value;
            }
        }
    }

    private static byte[] TextBytes(Encoding encoding) =>
        encoding.GetBytes(string.Concat(AllLines.Select(line => line + "\n")));

    /// <summary>Recovery points at fixed byte intervals, which fall mid-line.</summary>
    private static List<RecoveryPointOffset> RecoveryPointsFor(byte[] data, Encoding encoding)
    {
        var points = new List<RecoveryPointOffset>();
        for (var offset = RecoveryPointInterval; offset < data.Length; offset += RecoveryPointInterval)
        {
            var lineFeeds = encoding.GetString(data, 0, offset).Count(character => character == '\n');
            points.Add(new RecoveryPointOffset(offset, 0, lineFeeds));
        }

        return points;
    }

    private static TextPresenter CreatePresenter(Stream stream, List<RecoveryPointOffset> points, Encoding encoding) =>
        new(stream, points, encoding, RenderedLines, BufferedLines, BufferCapacity);

    private static string[] ExpectedLines(long first) =>
        AllLines.Skip((int)first).Take(RenderedLines).ToArray();

    [TestMethod]
    [DynamicData(nameof(Encodings))]
    public void Render_InitiallyReturnsTheFirstLines(Encoding encoding)
    {
        var data = TextBytes(encoding);
        var sut = CreatePresenter(new MemoryStream(data), RecoveryPointsFor(data, encoding), encoding);

        sut.Render().ShouldBe(ExpectedLines(0));
    }

    [TestMethod]
    [DynamicData(nameof(Encodings))]
    public void ScrollTo_ReturnsTheRequestedLines_AnywhereInTheStream(Encoding encoding)
    {
        var data = TextBytes(encoding);
        var sut = CreatePresenter(new MemoryStream(data), RecoveryPointsFor(data, encoding), encoding);

        foreach (var firstLine in new long[] { 17, 40, 41, 123, 500, 499, 3, 0, 700, 701, 990, 5 })
        {
            sut.ScrollTo(firstLine);

            sut.FirstRenderedLine.ShouldBe(firstLine);
            sut.Render().ShouldBe(ExpectedLines(firstLine), $"first line {firstLine}");
        }
    }

    [TestMethod]
    public void ScrollTo_NearTheEnd_RendersTheRemainingLines()
    {
        var data = TextBytes(Encoding.UTF8);
        var sut = CreatePresenter(new MemoryStream(data), RecoveryPointsFor(data, Encoding.UTF8), Encoding.UTF8);

        sut.ScrollTo(995);

        sut.Render().ShouldBe(AllLines[995..]);
    }

    [TestMethod]
    public void ScrollTo_WithinTheBuffer_DoesNotSeek()
    {
        var data = TextBytes(Encoding.UTF8);
        var stream = new PositionCountingStream(data);
        var sut = CreatePresenter(stream, RecoveryPointsFor(data, Encoding.UTF8), Encoding.UTF8);
        var positionSetsAfterLoading = stream.PositionSetCount;

        sut.ScrollTo(BufferedLines - RenderedLines);
        sut.ScrollTo(3);

        stream.PositionSetCount.ShouldBe(positionSetsAfterLoading);
        sut.Render().ShouldBe(ExpectedLines(3));
    }

    [TestMethod]
    public void ScrollTo_FarBeforeTheBuffer_SeeksToARecoveryPoint()
    {
        var data = TextBytes(Encoding.UTF8);
        var stream = new PositionCountingStream(data);
        var sut = CreatePresenter(stream, RecoveryPointsFor(data, Encoding.UTF8), Encoding.UTF8);
        sut.ScrollTo(900);
        var positionSetsBefore = stream.PositionSetCount;

        sut.ScrollTo(100);

        stream.PositionSetCount.ShouldBe(positionSetsBefore + 1);
        sut.Render().ShouldBe(ExpectedLines(100));
    }

    [TestMethod]
    public void ScrollTo_ThroughAGzipWithRecoveryPoints_ReturnsTheRequestedLines()
    {
        var compressed = new MemoryStream();
        var points = new List<RecoveryPointOffset>();
        using (var writer = new GZipWritingStreamWithRecoveryPoints(compressed, true, RecoveryPointInterval, encoding: Encoding.UTF8))
        {
            writer.RecoveryPointWritten += points.Add;
            foreach (var line in AllLines)
            {
                writer.Write(Encoding.UTF8.GetBytes(line + "\n"));
            }
        }

        points.Count.ShouldBeGreaterThan(3);
        using var stream = BackwardSeekableStream.ForGZipWithRecoveryPoints(compressed, points);
        var sut = CreatePresenter(stream, points, Encoding.UTF8);

        foreach (var firstLine in new long[] { 900, 17, 450, 449, 990, 0 })
        {
            sut.ScrollTo(firstLine);

            sut.Render().ShouldBe(ExpectedLines(firstLine), $"first line {firstLine}");
        }
    }

    [TestMethod]
    public void Constructor_BufferSmallerThanRendered_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            new TextPresenter(new MemoryStream(), [], Encoding.UTF8, 10, 5, 100));
    }
}
