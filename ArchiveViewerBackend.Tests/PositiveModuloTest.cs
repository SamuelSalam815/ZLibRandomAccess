using ArchiveViewerBackend;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(PositiveModulo))]
public class PositiveModuloTest
{

    [TestMethod]
    [DataRow(-3, 3, 0)]
    [DataRow(-2, 3, 1)]
    [DataRow(-1, 3, 2)]
    [DataRow(0, 3, 0)]
    [DataRow(1, 3, 1)]
    [DataRow(2, 3, 2)]
    [DataRow(3, 3, 0)]
    [DataRow(4, 3, 1)]
    [DataRow(5, 3, 2)]
    [DataRow(6, 3, 0)]
    public void ValueWrapsAround(long value, long @base, long expected)
    {
        PositiveModulo.Fix(value, @base).ShouldBe(expected);
    }
}
