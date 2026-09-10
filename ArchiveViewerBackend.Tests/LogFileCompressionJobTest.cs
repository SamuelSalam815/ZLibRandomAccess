using System;
using System.Text;
using ArchiveAccessPointVisualizer;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(LogFileCompressionJob))]
public class LogFileCompressionJobTest
{
    [TestMethod]
    public void CreatingACompressionJobWhenOutputFileIsNotGzipFile_ShouldThrow()
    {
        var constructor = () => new LogFileCompressionJob("test", 0, Encoding.Default);
        constructor.ShouldThrow<ArgumentException>();
    }

    [TestMethod]
    public void NonNullRecoverPointFile_WhenRecoverPointIntervalIsNotNull()
    {
        new LogFileCompressionJob("test.gz", 0, Encoding.Default, 100).RecoveryPointFilePath.ShouldBe("test.csv");
    }

    [TestMethod]
    public void NullRecoverPointFile_WhenRecoverPointIntervalIsNull()
    {
        new LogFileCompressionJob("test.gz", 0, Encoding.Default).RecoveryPointFilePath.ShouldBeNull();
    }
}
