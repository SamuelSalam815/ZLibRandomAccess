using System.IO;
using System.IO.Compression;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ZLibWrapper.Tests;

[TestClass]
[TestSubject(typeof(ZLibInflateStream))]
public class ZLibInflateStreamTest
{

    [TestMethod]
    public void DecompressingCompressedData_ShouldSucceed()
    {
        const string testData = "Hello World!";
        using var compressedStream = Compress(testData);
        using var sut = new ZLibInflateStream(compressedStream);
        using var streamReader = new StreamReader(sut);

        var result = streamReader.ReadToEnd();

        result.ShouldBe(testData);
    }

    private Stream Compress(string data)
    {
        var outputStream = new MemoryStream();
        using var sut = new ZLibDeflateStream(outputStream, leaveOpen: true);
        using var streamWriter = new StreamWriter(sut);

        streamWriter.WriteLine(data);
        streamWriter.Flush();

        outputStream.Position = 0;
        return outputStream;
    }
}
