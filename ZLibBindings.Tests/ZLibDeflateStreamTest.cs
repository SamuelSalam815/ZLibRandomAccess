using System.IO;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZLibWrapper;

namespace ZLibBindings.Tests;

[TestClass]
[TestSubject(typeof(ZLibDeflateStream))]
public class ZLibDeflateStreamTest
{

    [TestMethod]
    public void CompressingData_ShouldSucceed()
    {
        const string inputString = "Hello, World!";
        using var inputStream = new MemoryStream();
        using var sut = new ZLibDeflateStream(inputStream);
        using var streamWriter = new StreamWriter(sut);

        streamWriter.WriteLine(inputString);
        streamWriter.Flush();
    }
}
