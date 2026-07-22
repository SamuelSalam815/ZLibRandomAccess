using System.IO;
using System.IO.Compression;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using ZLibBindings.Constants;
using ZLibWrapper;

namespace ZLibWrapper.Tests;

[TestClass]
[TestSubject(typeof(ZLibInflateStreamWithRecoveryPoints))]
public class ZLibInflateStreamWithRecoveryPointsTest
{
    [TestMethod]
    [DataRow(ZWindowBits.WindowSize512B)]
    [DataRow(ZWindowBits.WindowSize32Kb)]
    [DataRow(ZWindowBits.WindowSize512B | ZWindowBits.GZipStream)]
    [DataRow(ZWindowBits.WindowSize32Kb | ZWindowBits.GZipStream)]
    [DataRow(ZWindowBits.RawDeflateStreamWindowSize512B)]
    [DataRow(ZWindowBits.RawDeflateStreamWindowSize32Kb)]
    public void JumpingToRecoveryPoint_ShouldAllowSuccessfulDataRead(ZWindowBits windowBits)
    {
        const string greetingLine = "Hello, World!";
        const string expectedData = "Foo Bar";
        long recoveryPointOffset;
        using var underlyingStream = new MemoryStream();
        using(var compressor = new ZLibDeflateStreamWithRecoveryPoints(underlyingStream, leaveOpen: true, new ZLibDeflateConfiguration{WindowBits = windowBits}))
        {
            using var streamWriter = new StreamWriter(compressor);
            streamWriter.WriteLine(greetingLine);
            streamWriter.Flush();
            recoveryPointOffset = streamWriter.BaseStream.Position;
            streamWriter.Write(expectedData);
        }

        underlyingStream.Position = 0;
        string actualData;
        using (var decompressor = new ZLibInflateStreamWithRecoveryPoints(underlyingStream, windowBits: windowBits))
        {
            decompressor.JumpTo(recoveryPointOffset);
            using var streamReader = new StreamReader(decompressor);
            actualData = streamReader.ReadToEnd();
        }

        actualData.ShouldBe(expectedData);
    }
}
