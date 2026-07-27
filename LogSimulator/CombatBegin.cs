using LogSimulator.ChacterSpec;
using LogSimulator.Logging;
using LogSimulator.Rolls;

namespace LogSimulator;

// TODO: adhere to 'Request, Resolution' pattern here

public record CombatBegin(Character Hero, Character Adversary, RollResolution HeroFortitudeRoll, RollResolution AdversaryFortitudeRoll) : IDescribableGameEvent
{
    public static RollResolution RollVitality(int fortitudeValue, DieRollGenerator  dieRollGenerator)
    {
        var baseVitalityModifier = new Modifier("Base Vitality", (int)Math.Floor(fortitudeValue * 1.5));
        return RollBuilder
            .For("Combat Vitality")
            .Roll(fortitudeValue)
            .D(3)
            .Plus(baseVitalityModifier)
            .ResolveWith(dieRollGenerator);
    }

    public GameEventDescription DescribeEvent()
    {
        return new GameEventDescription(
            $"{Hero.Name} begins battle with {HeroFortitudeRoll.RolledTotal} vitality and {Adversary.Name} begins battle with {AdversaryFortitudeRoll.RolledTotal} vitality!",
            [
                HeroFortitudeRoll.DescribeEvent(),
                AdversaryFortitudeRoll.DescribeEvent()
            ]
        );
    }
}
