using System.IO;
using System.Linq;
using Benchmarking.CompressionRatio.Exporters;
using Benchmarking.CompressionRatio.Values;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Benchmarking.CompressionRatio.Tests.Exporters;

[TestClass]
[TestSubject(typeof(CompressionResultGraphExporter))]
public class CompressionResultGraphExporterTest
{
    private static CompressionResult Result(
        CompressionMethod method,
        NormalizedCompressionLevel level,
        long size,
        long compressed) =>
        new(new CompressionRequest(method, level, size), compressed);

    [TestMethod]
    public void ToBars_AveragesSpaceSavedOverSizes()
    {
        var bars = CompressionResultGraphExporter.ToBars(
        [
            Result(CompressionMethod.GZipUsingZLib, NormalizedCompressionLevel.Default, 100, 90),
            Result(CompressionMethod.GZipUsingZLib, NormalizedCompressionLevel.Default, 200, 160),
        ]);

        bars.Count.ShouldBe(1);
        bars[0].Category.ShouldBe("zlib");
        bars[0].Series.ShouldBe("Default");
        bars[0].Value.ShouldBe(15d, 1e-9);
    }

    [TestMethod]
    public void Complete_ReplacesExistingFileWithSvg()
    {
        var directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var path = Path.Combine(directory, "graph.svg");
        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(path, "old");
            using var sut = new CompressionResultGraphExporter(path);
            foreach (var request in CompressionRequest.AllCombinationsOf(
                         [CompressionMethod.GZipUsingZLib, CompressionMethod.GZipUsingBaseClassLibrary],
                         [NormalizedCompressionLevel.Default],
                         [1024, 2048]))
            {
                sut.Accept(new CompressionResult(request, request.NumberOfBytesToCompress / 2));
            }

            sut.Complete();

            File.ReadAllText(path).ShouldContain("<svg");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
