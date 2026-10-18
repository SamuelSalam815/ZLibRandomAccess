using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using JetBrains.Annotations;
using LogSimulator.Appliction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using ZLibWrapper;

namespace LogSimulator.Application.Tests.Integration;

[TestClass]
[TestSubject(typeof(Program))]
public class ProgramIntegrationTest
{

    // Snapshot of 1 MB of logs with a 64 KB recovery point interval. Regenerate with:
    // dotnet run --project LogSimulator.Appliction -- <outputDirectory> 1 64
    private static readonly DirectoryInfo TestArtifactsDirectory = new(Path.Combine(AppContext.BaseDirectory, "TestArtifacts"));

    public static IEnumerable<object[]> LogTimeStamps()
    {
        yield return [new DateTimeOffset(2026, 10, 18, 21, 2, 20, TimeSpan.Zero)];
    }

    [TestMethod]
    [DynamicData(nameof(LogTimeStamps))]
    public void WhenLogsAreDecompressed_TheirDataAreIdentical(DateTimeOffset logTimestamp)
    {
        var filePathFactory = new CompressionFilePathFactory(TestArtifactsDirectory, logTimestamp);
        using var controlStream =
            new GZipStream(ReadFileStream(filePathFactory.BCLCompressionFile), CompressionMode.Decompress);
        using var zlibStream =
            new GZipReadingStreamWithRecoveryPoints(ReadFileStream(filePathFactory.ZLibCompressionFile));
        using var zlibStreamWithRecoveryPoints =
            new GZipReadingStreamWithRecoveryPoints(ReadFileStream(filePathFactory.ZLibCompressionWithRecoveryPointsFile));

        List<TextReader> streamReaders =
        [
            new StreamReader(controlStream),
            new StreamReader(zlibStream),
            new StreamReader(zlibStreamWithRecoveryPoints)
        ];

        const int blockSize = 10 * 1024 * 1024;
        var blocks = streamReaders.Select(_ => new char[blockSize]).ToArray();
        while (true)
        {
            var readResults = streamReaders.Select((reader, i) => reader.ReadBlock(blocks[i])).ToArray();
            var firstBlock = blocks.First();
            foreach (var block in blocks.Skip(1))
            {
                block.ShouldBe(firstBlock);
            }

            if (readResults.All(readCount => readCount == blockSize))
            {
                continue;
            }

            foreach (var reader in streamReaders)
            {
                reader.Dispose();
            }
            break;
        }
    }

    private static FileStream ReadFileStream(FileInfo fileToRead)
    {
        return File.Open(
            fileToRead.FullName,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);
    }
}
