using LogSimulator.ChacterSpec;
using LogSimulator.Logging;
using LogSimulator.Rolls.Combat;

namespace LogSimulator;

public record CombatTurn(
    Character Hero,
    Character Adversary,
    AttackResolution HeroAttack,
    AttackResolution AdversaryAttack,
    int InitialHeroVitality,
    int InitialAdversaryVitality) : IDescribableGameEvent
{
    public int RemainingHeroVitality => InitialHeroVitality - AdversaryAttack.DamageInflicted;
    public int RemainingAdversaryVitality => InitialAdversaryVitality - HeroAttack.DamageInflicted;
    public bool DidHeroWin => RemainingAdversaryVitality <= 0;
    public bool DidAdversaryWin => RemainingHeroVitality <= 0;
    public bool IsCombatComplete => DidHeroWin || DidAdversaryWin;

    public void LogEvent(GameEventLogger logger)
    {
        logger.Log("{0} and {1} exchange blows in a round of combat...", Hero.Name, Adversary.Name);
        HeroAttack.LogEvent(logger);
        AdversaryAttack.LogEvent(logger);
        if (IsCombatComplete)
        {
            LogCombatConclusion(logger);
            return;
        }

        LogCombatContinuation(logger);
    }

    private void LogCombatContinuation(GameEventLogger logger)
    {
        logger.Log("{0} has {1} remaining vitality...", Hero.Name, RemainingHeroVitality);
        logger.Log("{0} has {1} remaining vitality...", Adversary.Name, RemainingAdversaryVitality);
    }

    private void LogCombatConclusion(GameEventLogger logger)
    {
        if (DidHeroWin)
        {
            if (RemainingHeroVitality == 0)
            {
                logger.Log("{0} defeated {1} by the skin of their teeth!", Hero.Name, Adversary.Name);
                return;
            }

            logger.Log("{0} defeated {1}!", Hero.Name, Adversary.Name);
        }
        else
        {
            logger.Log("{0} defeated {1}!", Adversary.Name, Hero.Name);
        }
    }
};