using LogSimulator.ChacterSpec;
using LogSimulator.Combat.Rolls;
using LogSimulator.Logging;

namespace LogSimulator.Combat;

public record CombatTurn(
    Character Hero,
    Character Adversary,
    AttackResolution HeroAttack,
    AttackResolution AdversaryAttack,
    int InitialHeroVitality,
    int InitialAdversaryVitality) : ILoggableGameEvent
{
    public int RemainingHeroVitality => InitialHeroVitality - AdversaryAttack.DamageInflicted;
    public int RemainingAdversaryVitality => InitialAdversaryVitality - HeroAttack.DamageInflicted;
    public bool DidHeroWin => RemainingAdversaryVitality <= 0;
    public bool DidAdversaryWin => RemainingHeroVitality <= 0;
    public bool IsCombatComplete => DidHeroWin || DidAdversaryWin;


    public IEnumerable<GameEventLog> Log()
    {
        string battlePrefix;
        if (IsCombatComplete)
        {
            battlePrefix = DidHeroWin
                ? $"{Hero.Name} defeated {Adversary.Name} in combat! "
                : $"{Adversary.Name} defeated {Hero.Name} in combat! ";
        }
        else
        {
            battlePrefix = "Blows were exchanged in combat... ";
        }

        yield return GameEventLog.TurnEvent(
            battlePrefix +
            $"{Hero.Name}'s Vitality [{InitialHeroVitality} -> {RemainingHeroVitality}]; {Adversary.Name}'s Vitality [{InitialAdversaryVitality} -> {RemainingAdversaryVitality}]"
        );

        foreach (var log in HeroAttack.Log())
        {
            yield return log;
        }

        foreach (var log in AdversaryAttack.Log())
        {
            yield return log;
        }
    }
};
