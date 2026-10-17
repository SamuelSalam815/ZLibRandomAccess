using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

[TestClass]
[TestSubject(typeof(CircularArrayV2<>))]
public class CircularArrayV2Test
{
    [TestMethod]
    public void Capacity_ShouldGetPassedThrough()
    {
        new CircularArrayV2<int>(1337).Capacity.ShouldBe(1337);
    }

    [TestMethod]
    public void LengthShouldBeZero_AfterCreation()
    {
        new CircularArrayV2<int>(3).Length.ShouldBe(0);
    }
}
