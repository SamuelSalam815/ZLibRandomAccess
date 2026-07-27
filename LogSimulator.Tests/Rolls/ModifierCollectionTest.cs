using System;
using JetBrains.Annotations;
using LogSimulator.Rolls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace LogSimulator.Tests.Rolls;

[TestClass]
[TestSubject(typeof(ModifierCollection))]
public class ModifierCollectionTest
{

    [TestMethod]
    public void AddingAModifier_ShouldStoreTheModifier()
    {
        var modifier = new Modifier("Test", 3);
        ModifierCollection.Empty.Add(modifier).Set.ShouldContain(modifier);
    }

    [TestMethod]
    public void AddingTheSameModifierTwice_ShouldThrow()
    {
        var modifier = new Modifier("Test", 3);
        var action = () => ModifierCollection.Empty.Add(modifier).Add(modifier);
        action.ShouldThrow<Exception>();
    }

    [TestMethod]
    public void CannotAddModifier_AfterItHasAlreadyBeenAdded()
    {
        var modifier = new Modifier("Test", 3);
        ModifierCollection.Empty.Add(modifier).CanAdd(modifier).ShouldBeFalse();
    }
}
