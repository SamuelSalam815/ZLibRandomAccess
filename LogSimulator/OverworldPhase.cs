using System.Diagnostics.CodeAnalysis;
using LogSimulator.ChacterSpec;
using LogSimulator.Logging;
using LogSimulator.Rolls;
using LogSimulator.Rolls.Combat;

namespace LogSimulator;

public record OverworldPhase(GameState GameState, int AdventureCounter = 0) : GamePhase(GameState)
{
    public static OverworldPhase NewGame(GameState gameState)
    {
        return new OverworldPhase(gameState);
    }

    public override GamePhase ProgressGame(DieRollGenerator dieRollGenerator)
    {
        if (ContinueAdventure(dieRollGenerator, out var updatedGameState, out var combatInitiation))
        {
            return this with { GameState = updatedGameState, AdventureCounter = AdventureCounter + 1 };
        }

        return CombatPhase.CreateFrom(updatedGameState, combatInitiation);
    }

    private GameEventLogTree SummarizeOverworldPhase(Character adversary)
    {
        return GameEventLog
            .GamePhaseEvent($"{Hero.Name} roamed for {AdventureCounter} rounds before encountering {adversary.Name}!")
            .FlagAsSummary();
    }

    private bool ContinueAdventure(
        DieRollGenerator dieRollGenerator,
        out GameState updatedGameState,
        [NotNullWhen(false)] out InitiateCombatResolution? combatBegin)
    {
        var randomEncounterRollBuilder = RollBuilder
            .StandardRoll()
            .WithAdvantage(2)
            .Plus(new Modifier("Diminished Agility", Hero.Agility.Value / 2));

        if (GameState.NumberOfCombatsCompleted >= 36)
        {
            var bossEncounterRoll =
                randomEncounterRollBuilder
                    .AgainstStandardDifficulty($"Can {Hero.Name} avoid the wrath of the boss?")
                    .AutoFail();

            var finalBoss = Bestiary.FinalBoss;
            combatBegin = new InitiateCombatRequest(Hero, finalBoss).ResolveWith(dieRollGenerator);
            updatedGameState = GameState
                .RecordEvent(bossEncounterRoll)
                .RecordEvent(SummarizeOverworldPhase(finalBoss));
            return false;
        }

        var avoidRandomEncounterRoll = randomEncounterRollBuilder
            .AgainstStandardDifficulty($"Can {Hero.Name} roam the overworld in peace?")
            .ResolveWith(dieRollGenerator);

        updatedGameState = GameState.RecordEvent(avoidRandomEncounterRoll);

        if (avoidRandomEncounterRoll.IsSuccess)
        {
            combatBegin = null;
            return true;
        }

        var slime = Bestiary.Slime;
        combatBegin = new InitiateCombatRequest(Hero, slime).ResolveWith(dieRollGenerator);

        updatedGameState = updatedGameState.RecordEvent(SummarizeOverworldPhase(slime));
        return false;
    }

    private CombatPhase FinalConfrontation(DieRollGenerator dieRollGenerator)
    {
        var bossEncounterRoll =
            RollBuilder
                .StandardRoll()
                .WithAdvantage(2)
                .Plus(new Modifier("Diminished Agility", Hero.Agility.Value / 2))
                .AgainstStandardDifficulty($"Can {Hero.Name} avoid the wrath of the boss?")
                .AutoFail();

        var combatBegin = new InitiateCombatRequest(Hero, Bestiary.FinalBoss).ResolveWith(dieRollGenerator);
        return CombatPhase.CreateFrom(GameState.RecordEvents(bossEncounterRoll), combatBegin);
    }
}
