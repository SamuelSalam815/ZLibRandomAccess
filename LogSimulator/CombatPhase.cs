using System.Text;
using LogSimulator.ChacterSpec;
using LogSimulator.Logging;
using LogSimulator.Rolls.Combat;
using LogSimulator.Rolls.LimitBreak;

namespace LogSimulator;

// TODO: Simplify control flow here
public record CombatPhase(
    GameState GameState,
    Character Adversary,
    int HeroVitality,
    int AdversaryVitality,
    int LimitBreakCount) : GamePhase(GameState)
{
    public static CombatPhase CreateFrom(GameState gameState, InitiateCombatResolution initiateCombatResolution, int limitBreakCount = 0)
    {
        return new CombatPhase(
            gameState.RecordEvent(initiateCombatResolution),
            initiateCombatResolution.Request.Adversary,
            initiateCombatResolution.HeroFortitudeRoll.RolledTotal,
            initiateCombatResolution.AdversaryFortitudeRoll.RolledTotal,
            limitBreakCount);
    }

    public override GamePhase? ProgressGame(DieRollGenerator dieRollGenerator)
    {
        var combatTurn = new CombatTurn(
            Hero,
            Adversary,
            new AttackRequest(Hero, Adversary).ResolveWith(dieRollGenerator),
            new AttackRequest(Adversary, Hero).ResolveWith(dieRollGenerator),
            HeroVitality,
            AdversaryVitality
        );
        var updatedGameState = GameState.RecordEvent(combatTurn);

        if (!combatTurn.IsCombatComplete)
        {
            return new CombatPhase(
                    updatedGameState,
                    Adversary,
                    combatTurn.RemainingHeroVitality,
                    combatTurn.RemainingAdversaryVitality,
                    LimitBreakCount);
        }

        if (!combatTurn.DidHeroWin)
        {
            return PerformDeathRoll(dieRollGenerator);
        }

        updatedGameState = updatedGameState
            .IncrementCombatCounter()
            .RecordEvent(SummarizeCombat(true));

        return Adversary == Bestiary.FinalBoss
            ? GameOverPhase.CreateFrom(updatedGameState with { FinalBossDefeated = true })
            : new OverworldPhase(updatedGameState);
    }

    private GameEventLogTree SummarizeCombat(bool didHeroWin)
    {
        var message = new StringBuilder();
        if (didHeroWin)
        {
            message.Append($"{Hero.Name} defeated {Adversary.Name} in combat");
        }
        else
        {
            message.Append($"{Adversary.Name} defeated {Hero.Name} in combat");
        }

        if (LimitBreakCount == 1)
        {
            message.Append($" after {Hero.Name} activated [LIMIT BREAK]!");
        }

        if (LimitBreakCount > 1)
        {
            message.Append($" after {Hero.Name} activated [LIMIT BREAK] {LimitBreakCount} times!");
        }

        return GameEventLog.GamePhaseEvent(message.ToString()).FlagAsSummary();
    }

    private GamePhase PerformDeathRoll(
        DieRollGenerator dieRollGenerator)
    {
        var limitBreak = LimitBreakResolution.CreateFrom(GameState, dieRollGenerator);
        var updatedGameState = limitBreak.GameState.RecordEvent(limitBreak);

        if (!limitBreak.IsSuccess)
        {
            return GameOverPhase.CreateFrom(updatedGameState.RecordEvent(SummarizeCombat(false)));
        }

        var beginCombat = new InitiateCombatRequest(limitBreak.GameState.Hero, Adversary).ResolveWith(dieRollGenerator);
        updatedGameState = updatedGameState.RecordEvent(beginCombat);

        return CreateFrom(updatedGameState, beginCombat, LimitBreakCount + 1);
    }
}
