using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace ZLibBindings.Tests;

[TestClass]
[TestSubject(typeof(ZLibLowLevelBindings))]
public class ZLibLowLevelBindingsTest
{

    [TestMethod]
    public void ZLibVersion_IsExpected()
    {
        ZLibLowLevelBindings.ZlibVersion().ShouldBe("1.3.2");
    }
}
