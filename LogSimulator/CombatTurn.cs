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


    public GameEventDescription DescribeEvent()
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

        return new GameEventDescription(
            battlePrefix +
            $"{Hero.Name}'s Vitality [{InitialHeroVitality} -> {RemainingHeroVitality}]; {Adversary.Name}'s Vitality [{InitialAdversaryVitality} -> {RemainingAdversaryVitality}]",
            [
                HeroAttack.DescribeEvent(),
                AdversaryAttack.DescribeEvent()
            ]
        );
    }
};
