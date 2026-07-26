using LogSimulator.ChacterSpec;
using LogSimulator.Rolls.Combat;

namespace LogSimulator;

public record CombatPhase(
    Character Hero,
    Character Adversary,
    int HeroVitality,
    int AdversaryVitality) : GamePhase
{
    public static CombatPhase CreateFrom(CombatBegin combatBegin)
    {
        return new CombatPhase(
            combatBegin.Hero,
            combatBegin.Adversary,
            combatBegin.HeroFortitudeRoll.RolledTotal,
            combatBegin.AdversaryFortitudeRoll.RolledTotal);
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

        if (combatTurn.IsCombatComplete)
        {
            return combatTurn.DidHeroWin
                ? new GameProgress(new OverworldPhase(Hero), [combatTurn])
                : new GameProgress(new GameOverPhase(), [combatTurn]);
        }

        return new GameProgress(
            new CombatPhase(Hero, Adversary, combatTurn.RemainingHeroVitality, combatTurn.RemainingAdversaryVitality),
            [combatTurn]);
    }
}