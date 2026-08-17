using System.Collections.Generic;
using ArchiveViewerBackend;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(StreamSplitterLogic))]
public class StreamSplitterLogicTest
{
    public record SplitTestData(
        long TotalNumberOfBytes,
        long NumberOfBytesToOverlap,
        List<long> OffsetsToSplitOn,
        List<StreamSplitterLogic.SplitSpec> ExpectedSplits
    );

    private static StreamSplitterLogic.SplitSpec SplitSpec(long offset, long length)
    {
        return new StreamSplitterLogic.SplitSpec(offset, length);
    }

    public static IEnumerable<object?[]> SplitTestDataEnumerable()
    {
        yield return
        [
            new SplitTestData(
                1_000,
                0,
                [],
                [
                    SplitSpec(0, 1_000),
                ])
        ];

        yield return
        [
            new SplitTestData(
                1_001,
                100,
                [1_000],
                [
                    SplitSpec(0, 1_001),
                    SplitSpec(1_000, 1),
                ])
        ];

        yield return
        [
            new SplitTestData(
                1_000,
                300,
                [200, 400, 600, 800],
                [
                    SplitSpec(0, 500),
                    SplitSpec(200, 500),
                    SplitSpec(400, 500),
                    SplitSpec(600, 400),
                    SplitSpec(800, 200),
                ])
        ];

        yield return
        [
            new SplitTestData(
                1_000,
                0,
                [200, 700],
                [
                    SplitSpec(0, 200),
                    SplitSpec(200, 500),
                    SplitSpec(700, 300)
                ])
        ];

        yield return
        [
            new SplitTestData(
                1_000,
                100,
                [200, 700],
                [
                    SplitSpec(0, 300),
                    SplitSpec(200, 600),
                    SplitSpec(700, 300)
                ])
        ];
    }

    [TestMethod]
    [DynamicData(nameof(SplitTestDataEnumerable))]
    public void SplittingBytes_ProducesTheExpectedSplits(SplitTestData testData)
    {
        new StreamSplitterLogic(testData.TotalNumberOfBytes, testData.NumberOfBytesToOverlap)
            .DescribeSplits(testData.OffsetsToSplitOn)
            .ShouldBe(testData.ExpectedSplits);
    }


}
