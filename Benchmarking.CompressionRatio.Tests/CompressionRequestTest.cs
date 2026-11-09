using Benchmarking.CompressionRatio;
using Benchmarking.CompressionRatio.Values;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Benchmarking.CompressionRatio.Tests;

[TestClass]
[TestSubject(typeof(CompressionRequest))]
public class CompressionRequestTest
{

    [TestMethod]
    public void RequestingAllCombinations_ProducesTheExpectedCombinations()
    {
        CompressionRequest.AllCombinationsOf(
                [
                    CompressionMethod.GZipUsingZLib,
                    CompressionMethod.GZipUsingBaseClassLibrary
                ],
                [
                    NormalizedCompressionLevel.Fastest,
                    NormalizedCompressionLevel.Default
                ],
                [
                    2,
                    3
                ]
            )
            .ShouldBe(
                [
                    new CompressionRequest(CompressionMethod.GZipUsingZLib, NormalizedCompressionLevel.Fastest, 2),
                    new CompressionRequest(CompressionMethod.GZipUsingZLib, NormalizedCompressionLevel.Fastest, 3),
                    new CompressionRequest(CompressionMethod.GZipUsingZLib, NormalizedCompressionLevel.Default, 2),
                    new CompressionRequest(CompressionMethod.GZipUsingZLib, NormalizedCompressionLevel.Default, 3),
                    new CompressionRequest(CompressionMethod.GZipUsingBaseClassLibrary, NormalizedCompressionLevel.Fastest, 2),
                    new CompressionRequest(CompressionMethod.GZipUsingBaseClassLibrary, NormalizedCompressionLevel.Fastest, 3),
                    new CompressionRequest(CompressionMethod.GZipUsingBaseClassLibrary, NormalizedCompressionLevel.Default, 2),
                    new CompressionRequest(CompressionMethod.GZipUsingBaseClassLibrary, NormalizedCompressionLevel.Default, 3),
                ]
                , ignoreOrder: true);
    }
}
