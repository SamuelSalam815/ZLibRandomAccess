using JetBrains.Annotations;
using LogSimulator.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace LogSimulator.Tests.Logging;

[TestClass]
[TestSubject(typeof(GameEventDescription))]
public class GameEventDescriptionTest
{

    [TestMethod]
    public void StringRepresentation_ShouldIndentSubEvents()
    {
        new GameEventDescription(
            "Hello, World!",
            [
                "Foo",
                new GameEventDescription("Fizz", ["Buzz", "Buzz"]),
                "Bar",
            ])
            .ToString()
            .ShouldBe("""
                      Hello, World!
                      |   Foo
                      |   Fizz
                      |   |   Buzz
                      |   |   Buzz
                      |   Bar

                      """
            );
    }
}
