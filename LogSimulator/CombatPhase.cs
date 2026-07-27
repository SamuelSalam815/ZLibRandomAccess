using LogSimulator.ChacterSpec;
using LogSimulator.Logging;
using LogSimulator.Rolls.Combat;
using LogSimulator.Rolls.LimitBreak;

namespace LogSimulator;

public record CombatPhase(
    GameState GameState,
    Character Adversary,
    int HeroVitality,
    int AdversaryVitality) : GamePhase(GameState)
{
    public static CombatPhase CreateFrom(GameState gameState, InitiateCombatResolution initiateCombatResolution)
    {
        return new CombatPhase(
            gameState,
            initiateCombatResolution.Request.Adversary,
            initiateCombatResolution.HeroFortitudeRoll.RolledTotal,
            initiateCombatResolution.AdversaryFortitudeRoll.RolledTotal);
    }

    public override GameProgress ProgressGame(DieRollGenerator dieRollGenerator)
    {
        var combatTurn = new CombatTurn(
            Hero,
            Adversary,
            new AttackRequest(Hero, Adversary).ResolveWith(dieRollGenerator),
            new AttackRequest(Adversary, Hero).ResolveWith(dieRollGenerator),
            HeroVitality,
            AdversaryVitality
        );

        var events = new List<IDescribableGameEvent>{combatTurn};
        if (!combatTurn.IsCombatComplete)
        {
            return new GameProgress(
                new CombatPhase(
                    GameState,
                    Adversary,
                    combatTurn.RemainingHeroVitality,
                    combatTurn.RemainingAdversaryVitality),
                events);
        }

        if (!combatTurn.DidHeroWin)
        {
            return PerformDeathRoll(dieRollGenerator, events);
        }

        var gameState = GameState.IncrementCombatCounter();
        return Adversary == Bestiary.FinalBoss
            ? new GameProgress(new GameOverPhase(gameState with { FinalBossDefeated = true }), events)
            : new GameProgress(new OverworldPhase(gameState), events);

    }

    private GameProgress PerformDeathRoll(DieRollGenerator dieRollGenerator, List<IDescribableGameEvent> events)
    {
        var limitBreak = LimitBreakResolution.CreateFrom(GameState, dieRollGenerator);

        if (!limitBreak.IsSuccess)
        {
            return new GameProgress(new GameOverPhase(GameState), [..events, limitBreak]);
        }

        var beginCombat = new InitiateCombatRequest(limitBreak.GameState.Hero, Adversary).ResolveWith(dieRollGenerator);

        return new GameProgress(
            CreateFrom(limitBreak.GameState, beginCombat),
            [..events, limitBreak, beginCombat]
        );
    }
}
