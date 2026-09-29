using System.Globalization;
using System.IO;
using System.Linq;
using Benchmarking.CompressionRatio.Exporters.CsvHelpers;
using Benchmarking.CompressionRatio.Values;
using CsvHelper;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Benchmarking.CompressionRatio.Tests.Exporters.CsvHelpers;

[TestClass]
[TestSubject(typeof(CompressionResultCsvDto))]
public class CompressionResultCsvDtoTest
{

    [TestMethod]
    public void RoundTripSerializationTest()
    {
        var sut = new CompressionResult(
            new CompressionRequest(CompressionMethod.GZipUsingZLib, NormalizedCompressionLevel.Default, 100),
            5
        );
        using var memoryStream = new MemoryStream();
        var csvWriter = new CsvWriter(new StreamWriter(memoryStream, leaveOpen: true), CultureInfo.InvariantCulture);
        csvWriter.WriteHeader<CompressionResultCsvDto>();
        csvWriter.NextRecord();
        csvWriter.WriteRecord(new CompressionResultCsvDto(sut));
        csvWriter.NextRecord();
        csvWriter.Dispose();
        memoryStream.Seek(0, SeekOrigin.Begin);
        var text = new StreamReader(memoryStream).ReadToEnd();
        using var csvReader = new CsvReader(new StringReader(text), CultureInfo.InvariantCulture);
        csvReader
            .GetRecords<CompressionResultCsvDto>()
            .Select(record => record.AsDomainObject())
            .ShouldBe([sut]);
    }
}
