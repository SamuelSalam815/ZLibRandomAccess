using LogSimulator.ChacterSpec;
using LogSimulator.Rolls;
using LogSimulator.Rolls.Combat;

namespace LogSimulator;

public record OverworldPhase(Character Hero) : GamePhase
{
    public override GameProgress ProgressGame(DieRollGenerator dieRollGenerator)
    {
        var avoidRandomEncounterRoll = RollBuilder
            .StandardRoll()
            .WithAdvantage(2)
            .Plus(new Modifier("Diminished Agility", Hero.Agility.Value / 2))
            .AgainstStandardDifficulty($"Can {Hero.Name} roam the overworld in peace?")
            .ResolveWith(dieRollGenerator);

        if (avoidRandomEncounterRoll.IsSuccess)
        {
            return new GameProgress(this, [avoidRandomEncounterRoll]);
        }

        var slime = new Character("Slime", new StatBlock(3,3,3));
        var combatBegin = new InitiateCombatRequest(Hero, slime).ResolveWith(dieRollGenerator);
        return new GameProgress(CombatPhase.CreateFrom(combatBegin), [avoidRandomEncounterRoll, combatBegin]);
    }
}
