using System.IO;
using System.Text;
using JetBrains.Annotations;
using LogSimulator.Simulator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace LogSimulator.Tests.Simulator;

[TestClass]
[TestSubject(typeof(LogSimulator.Simulator.LogSimulator))]
public class LogSimulatorTest
{

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(7)]
    [DataRow(32)]
    [DataRow(DataSize.KiloByte)]
    public void ExactDataSize_IsWrittenToProvidedStream(long targetInputDataSize)
    {
        var memoryStream = new MemoryStream();
        var sut = new LogSimulator.Simulator.LogSimulator();
        sut.SimulateLogs(memoryStream, Encoding.Default, targetInputDataSize);
        memoryStream.Length.ShouldBe(targetInputDataSize);
    }
}
