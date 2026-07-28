using JetBrains.Annotations;
using LogSimulator.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace LogSimulator.Tests.Logging;

[TestClass]
[TestSubject(typeof(GameEventLogTree))]
public class GameEventLogTreeTest
{

    [TestMethod]
    public void StringRepresentation_ShouldIndentSubEvents()
    {
        GameEventLog
            .GameEvent("Hello, World!")
            .AddDirectChild(GameEventLog.GameEvent("Foo"))
            .AddDirectChild(
                GameEventLog.GameEvent("Fizz")
                    .AddDirectChild(GameEventLog.GameEvent("Buzz"))
                    .AddDirectChild(GameEventLog.GameEvent("Buzz")))
            .AddDirectChild(GameEventLog.GameEvent("Bar"))
            .ToString()
            .ShouldBe("""
                       [GAME] Hello, World!
                       |   [GAME] Foo
                       |   [GAME] Fizz
                       |   |   [GAME] Buzz
                       |   |   [GAME] Buzz
                       |   [GAME] Bar

                       """
            );
    }
}
