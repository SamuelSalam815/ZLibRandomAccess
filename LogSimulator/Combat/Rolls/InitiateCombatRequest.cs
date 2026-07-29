using LogSimulator.ChacterSpec;
using LogSimulator.Rolls;

namespace LogSimulator.Combat.Rolls;

public record InitiateCombatRequest(Character Hero, Character Adversary)
{
    private static RollResolution RollVitality(Character character, DieRollGenerator  dieRollGenerator)
    {
        var fortitudeValue = character.Fortitude.Value;
        var baseVitalityModifier = new Modifier("Base Vitality", (int)Math.Floor(fortitudeValue * 1.5));
        return RollBuilder
            .For($"{character.Name}'s starting vitality")
            .Roll(fortitudeValue)
            .D(3)
            .Plus(baseVitalityModifier)
            .ResolveWith(dieRollGenerator);
    }

    public InitiateCombatResolution ResolveWith(DieRollGenerator dieRollGenerator)
    {
        return new InitiateCombatResolution(
            this,
            RollVitality(Hero, dieRollGenerator),
            RollVitality(Adversary, dieRollGenerator));
    }
}
