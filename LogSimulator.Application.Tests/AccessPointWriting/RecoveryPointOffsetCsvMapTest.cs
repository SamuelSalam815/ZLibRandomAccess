using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using JetBrains.Annotations;
using LogSimulator.Appliction.AccessPointWriting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using ZLibWrapper;

namespace LogSimulator.Application.Tests.AccessPointWriting;

[TestClass]
[TestSubject(typeof(RecoveryPointOffsetCsvMap))]
public class RecoveryPointOffsetCsvMapTest
{

    [TestMethod]
    public void CanPerformRoundTrip_UsingCsvHelper()
    {
        using var memoryStream = new MemoryStream();
        List<RecoveryPointOffset> expectedItems =
        [
            new(101, 321),
            new(401, 123),
        ];

        using(var textWriter = new StreamWriter(memoryStream, leaveOpen: true))
        {
            using var csvWriter = new CsvWriter(textWriter, CultureInfo.InvariantCulture);
            csvWriter.Context.RegisterClassMap<RecoveryPointOffsetCsvMap>();
            csvWriter.WriteRecords(expectedItems);
        }

        memoryStream.Position = 0;
        List<RecoveryPointOffset> actualItems;
        using (var textReader = new StreamReader(memoryStream))
        {
            using var csvReader = new CsvReader(textReader, CultureInfo.InvariantCulture);
            csvReader.Context.RegisterClassMap<RecoveryPointOffsetCsvMap>();
            actualItems = csvReader.GetRecords<RecoveryPointOffset>().ToList();
        }

        actualItems.ShouldBe(expectedItems);
    }
}
