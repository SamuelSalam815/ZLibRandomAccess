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
        var zlibVersion = ZLibLowLevelBindings.zlibVersion();

        zlibVersion.ShouldBe("1.3.2");
    }
}
