using System.Collections.Immutable;
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
    ImmutableList<IDescribableGameEvent> CombatEvents) : GamePhase(GameState)
{
    public static CombatPhase CreateFrom(GameState gameState, InitiateCombatResolution initiateCombatResolution)
    {
        return new CombatPhase(
            gameState,
            initiateCombatResolution.Request.Adversary,
            initiateCombatResolution.HeroFortitudeRoll.RolledTotal,
            initiateCombatResolution.AdversaryFortitudeRoll.RolledTotal,
            [initiateCombatResolution]);
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
        var updatedCombatEvents = CombatEvents.Add(combatTurn);

        if (!combatTurn.IsCombatComplete)
        {
            return new CombatPhase(
                    GameState,
                    Adversary,
                    combatTurn.RemainingHeroVitality,
                    combatTurn.RemainingAdversaryVitality,
                    updatedCombatEvents);
        }

        if (!combatTurn.DidHeroWin)
        {
            return PerformDeathRoll(dieRollGenerator, updatedCombatEvents);
        }

        var updatedGameState = GameState
            .IncrementCombatCounter()
            .RecordEvent(new EndOfCombatEvent(true, Hero, Adversary, updatedCombatEvents));

        return Adversary == Bestiary.FinalBoss
            ? GameOverPhase.CreateFrom(updatedGameState with { FinalBossDefeated = true })
            : new OverworldPhase(updatedGameState);

    }

    private GamePhase PerformDeathRoll(
        DieRollGenerator dieRollGenerator,
        ImmutableList<IDescribableGameEvent> updatedCombatEvents)
    {
        var limitBreak = LimitBreakResolution.CreateFrom(GameState, dieRollGenerator);
        updatedCombatEvents = updatedCombatEvents.Add(limitBreak);

        if (!limitBreak.IsSuccess)
        {
            var endOfCombatEvent = new EndOfCombatEvent(false, limitBreak.GameState.Hero, Adversary, updatedCombatEvents);
            return GameOverPhase.CreateFrom(limitBreak.GameState.RecordEvent(endOfCombatEvent));
        }

        var beginCombat = new InitiateCombatRequest(limitBreak.GameState.Hero, Adversary).ResolveWith(dieRollGenerator);

        return CreateFrom(limitBreak.GameState, beginCombat) with {CombatEvents = CombatEvents.Add(limitBreak).Add(beginCombat)};
    }

    private class EndOfCombatEvent(bool didHeroWin, Character hero, Character adversary, ImmutableList<IDescribableGameEvent> combatEvents) : IDescribableGameEvent
    {
        public GameEventDescription DescribeEvent()
        {
            return new GameEventDescription(didHeroWin
                ? $"{hero.Name} defeated {adversary.Name} in combat!"
                : $"{adversary.Name} defeated {hero.Name} in combat!",
                combatEvents.Select(e => e.DescribeEvent()).ToImmutableList());
        }
    }
}
