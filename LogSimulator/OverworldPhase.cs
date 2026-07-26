using LogSimulator.ChacterSpec;
using LogSimulator.Rolls;

namespace LogSimulator;

public record OverworldPhase(Character Hero) : GamePhase
{
    public override GameProgress ProgressGame(DieRollGenerator dieRollGenerator)
    {
        var avoidRandomEncounterRoll = RollBuilder
            .StandardRoll()
            .WithAdvantage()
            .AgainstStandardDifficulty($"Can {Hero.Name} roam the overworld in peace?")
            .ResolveWith(dieRollGenerator);

        if (avoidRandomEncounterRoll.IsSuccess)
        {
            return new GameProgress(this, [avoidRandomEncounterRoll]);
        }

        var slime = new Character("Slime", new StatBlock(3,3,3));
        var combatBegin = new CombatBegin(
            Hero,
            slime,
            CombatBegin.RollVitality(Hero.Fortitude.Value, dieRollGenerator),
            CombatBegin.RollVitality(slime.Fortitude.Value, dieRollGenerator)
        );
        return new GameProgress(CombatPhase.CreateFrom(combatBegin), [avoidRandomEncounterRoll, combatBegin]);
    }
}
