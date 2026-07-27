using LogSimulator.Logging;

namespace LogSimulator.Rolls.Combat;

public record AttackResolution(
    AttackRequest Request,
    RollResolution DefenseRoll,
    CheckedRollResolution HitRoll,
    RollResolution? DamageRoll
) : IDescribableGameEvent
{
    public bool IsMiss => !IsHit;
    public bool IsHit => HitRoll.IsSuccess;

    public bool IsCriticalHit => AttackRequest.IsHitRollCritical(HitRoll);

    public int DamageInflicted => DamageRoll?.RolledTotal ?? 0;

    public GameEventDescription DescribeEvent()
    {
        if (DamageRoll is null)
        {
            return new GameEventDescription(
                $"{Request.Attacker.Name}'s strike against {Request.Defender.Name} missed! ({DamageInflicted} damage)",
                [
                    DescribeDefenseRoll(),
                    DescribeHitRoll()
                ]);
        }

        if (IsCriticalHit)
        {
            return new GameEventDescription(
                $"{Request.Attacker.Name}'s [CRITICAL] strike against {Request.Defender.Name} inflicted {DamageInflicted} damage!",
                [
                    DescribeDefenseRoll(),
                    DescribeHitRoll(),
                    DescribeDamageRoll(DamageRoll)
                ]);
        }

        return new GameEventDescription(
            $"{Request.Attacker.Name}'s strike against {Request.Defender.Name} inflicted {DamageInflicted} damage!",
            [
                DescribeDefenseRoll(),
                DescribeHitRoll(),
                DescribeDamageRoll(DamageRoll)
            ]);
    }

    private GameEventDescription DescribeHitRoll()
    {
        return HitRoll.DescribeEvent();
    }

    private GameEventDescription DescribeDefenseRoll()
    {
        var defenseDescription = DefenseRoll.DescribeEvent();
        return defenseDescription with { Description = "Rolling Evasion: " + defenseDescription.Description };
    }

    private GameEventDescription DescribeDamageRoll(RollResolution damageRoll)
    {
        var damageDescription = damageRoll.DescribeEvent();
        return damageDescription with { Description = "Rolling Damage: " + damageDescription.Description };
    }
}
