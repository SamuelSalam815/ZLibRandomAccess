using System;
using JetBrains.Annotations;
using LogSimulator.Rolls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace LogSimulator.Tests.Rolls;

[TestClass]
[TestSubject(typeof(RollModifierCollection))]
public class RollModifierCollectionTest
{

    [TestMethod]
    public void AddingAModifier_ShouldStoreTheModifier()
    {
        var modifier = new RollModifier("Test", 3);
        RollModifierCollection.Empty.Add(modifier).Set.ShouldContain(modifier);
    }

    [TestMethod]
    public void AddingTheSameModifierTwice_ShouldThrow()
    {
        var modifier = new RollModifier("Test", 3);
        var action = () => RollModifierCollection.Empty.Add(modifier).Add(modifier);
        action.ShouldThrow<Exception>();
    }

    [TestMethod]
    public void CannotAddModifier_AfterItHasAlreadyBeenAdded()
    {
        var modifier = new RollModifier("Test", 3);
        RollModifierCollection.Empty.Add(modifier).CanAdd(modifier).ShouldBeFalse();
    }
}
