using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using ZLibBindings.Constants;

namespace ZLibWrapper.Tests;

[TestClass]
[TestCategory("Integration")]
public class ZLibRoundTripTests
{
    private static byte[][] GenerateData(long blockSize, long blockCount)
    {
        var result = new byte[blockCount][];
        for (var i = 0; i < blockCount; i++)
        {
            var block = new byte[blockSize];
            Random.Shared.NextBytes(block);
            result[i] = block;
        }

        return result;
    }

    [TestMethod]
    [DataRow(ZWindowBits.ZLib512BWindow)]
    [DataRow(ZWindowBits.ZLib32KbWindow)]
    [DataRow(ZWindowBits.GZip512BWindow)]
    [DataRow(ZWindowBits.GZip32KbWindow)]
    [DataRow(ZWindowBits.RawDeflate512BWindow)]
    [DataRow(ZWindowBits.RawDeflate32KbWindow)]
    public void DataFromRoundTripCompression_ShouldBePreserved(ZWindowBits windowBits)
    {
        // Use 20Mb of data
        const int blockSize = 1024 * 1024;
        const int blockCount = 20;
        var expectedData = GenerateData(blockSize, blockCount);
        using var dataStream = new MemoryStream();

        using (var compressionStream = new ZLibDeflateStream(
                   dataStream,
                   true,
                   new ZLibDeflateConfiguration { WindowBits = windowBits }))
        {
            foreach(var  block in expectedData)
            {
                compressionStream.Write(block);
            }
        }

        dataStream.Position = 0;
        var actualData = new byte[blockCount][];
        for (var i = 0; i < blockCount; i++)
        {
            actualData[i] = new byte[blockSize];
        }

        using (var decompressionStream = new ZLibInflateStream(dataStream, windowBits: windowBits))
        {
            foreach (var block in actualData)
            {
                decompressionStream.ReadExactly(block);
            }
        }

        actualData.ShouldBe(expectedData);
    }
}
