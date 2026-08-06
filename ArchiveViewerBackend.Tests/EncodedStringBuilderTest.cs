using System.Text;
using ArchiveViewerBackend.LineSearch;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(EncodedStringBuilder))]
public class EncodedStringBuilderTest
{

    [TestMethod]
    public void BuildingEncodedString_ProducesExpectedResults()
    {
        const string input = "abc";
        var encoding = Encoding.UTF8;
        var expectedBytes = "abc"u8.ToArray();

        new EncodedStringBuilder(input).SetEncoding(encoding).Create().Bytes.ShouldBe(expectedBytes);
        new EncodedStringBuilder(encoding).SetString(input).Create().Bytes.ShouldBe(expectedBytes);
    }

    [TestMethod]
    public void ProvidedString_IsUsedInEncodedString()
    {
        const string input = "abc";
        new EncodedStringBuilder(input).Create().String.ShouldBe(input);
    }

    [TestMethod]
    public void ProvidedEncoding_IsUsedInEncodedString()
    {
        var encoding = Encoding.UTF32;
        new EncodedStringBuilder(encoding).Create().Encoding.ShouldBe(encoding);
    }

    [TestMethod]
    public void DefaultEncoding_IsDotNetDefaultEncoding()
    {
        new EncodedStringBuilder("Hello").Create().Encoding.ShouldBe(Encoding.Default);
    }

    [TestMethod]
    public void DefaultString_IsTheEmptyString()
    {
        new EncodedStringBuilder(Encoding.UTF8).Create().String.ShouldBeEmpty();
    }
}
