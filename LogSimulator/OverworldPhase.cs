using LogSimulator.Rolls;
using LogSimulator.Rolls.Combat;

namespace LogSimulator;

public record OverworldPhase(GameState GameState) : GamePhase(GameState)
{
    public override GameProgress ProgressGame(DieRollGenerator dieRollGenerator)
    {
        if (GameState.NumberOfCombatsCompleted >= 36)
        {
            return FinalConfrontation(dieRollGenerator);
        }

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

        var combatBegin = new InitiateCombatRequest(Hero, Bestiary.Slime).ResolveWith(dieRollGenerator);
        return new GameProgress(CombatPhase.CreateFrom(GameState, combatBegin), [avoidRandomEncounterRoll, combatBegin]);
    }

    private GameProgress FinalConfrontation(DieRollGenerator dieRollGenerator)
    {
        var bossEncounterRoll =
            RollBuilder
                .StandardRoll()
                .WithAdvantage(2)
                .Plus(new Modifier("Diminished Agility", Hero.Agility.Value / 2))
                .AgainstStandardDifficulty($"Can {Hero.Name} avoid the wrath of the boss?")
                .AutoFail();

        var combatBegin = new InitiateCombatRequest(Hero, Bestiary.FinalBoss).ResolveWith(dieRollGenerator);
        return new GameProgress(CombatPhase.CreateFrom(GameState, combatBegin), [bossEncounterRoll, combatBegin]);
    }
}
