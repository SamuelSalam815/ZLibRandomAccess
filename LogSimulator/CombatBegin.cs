using LogSimulator.ChacterSpec;
using LogSimulator.Logging;
using LogSimulator.Rolls;

namespace LogSimulator;

public record CombatBegin(Character Hero, Character Adversary, RollResolution HeroFortitudeRoll, RollResolution AdversaryFortitudeRoll) : IDescribableGameEvent
{
    public static RollResolution RollVitality(int fortitudeValue, DieRollGenerator  dieRollGenerator)
    {
        var baseVitalityModifier = new RollModifier("Base Vitality", (int)Math.Floor(fortitudeValue * 1.5));
        return RollBuilder
            .Roll(fortitudeValue)
            .D(3)
            .Plus(baseVitalityModifier)
            .ResolveWith(dieRollGenerator);
    }

    public void LogEvent(GameEventLogger logger)
    {
        logger.Log("Combat between '{0}' and '{1}' initiated! Determining combat resilience..", Hero.Name, Adversary.Name);
        logger.Log("{0}'s initial combat vitality is determined to be {1}!", Hero.Name, HeroFortitudeRoll.RolledTotal);
        HeroFortitudeRoll.LogEvent(logger);
        logger.Log("{0}'s initial combat vitality is determined to be {1}!", Adversary.Name, AdversaryFortitudeRoll.RolledTotal);
        AdversaryFortitudeRoll.LogEvent(logger);
    }
}