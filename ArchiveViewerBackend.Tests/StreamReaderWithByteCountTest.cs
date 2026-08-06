using System.IO;
using System.Text;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(StreamReaderWithByteCount))]
public class StreamReaderWithByteCountTest
{
    private static readonly Encoding TestEncoding = Encoding.UTF8;

    [TestMethod]
    [DataRow("foo bar", 'f')]
    [DataRow("Hello, World!", 'h')]
    public void PeekReturnsNextCharacter(string input, char nextCharacter)
    {
        new StreamReaderWithByteCount(EncodeWith(input, TestEncoding), TestEncoding)
            .Peek()
            .ShouldBe(nextCharacter);
    }

    [TestMethod]
    [DataRow("foo bar", 'f')]
    [DataRow("Hello, World!", 'h')]
    public void PeekIsIdempotent(string input, char nextCharacter)
    {
        var stream = new StreamReaderWithByteCount(EncodeWith(input, TestEncoding), TestEncoding);
        _ = stream.Peek();
        stream.Peek().ShouldBe(nextCharacter);
    }

    private static Stream EncodeWith(string input, Encoding testEncoding)
    {
        return new MemoryStream(testEncoding.GetBytes(input));
    }
}
