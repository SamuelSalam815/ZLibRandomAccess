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
                    DefenseRoll.DescribeEvent(),
                    HitRoll.DescribeEvent()
                ]);
        }

        if (IsCriticalHit)
        {
            return new GameEventDescription(
                $"{Request.Attacker.Name}'s [CRITICAL] strike against {Request.Defender.Name} inflicted {DamageInflicted} damage!",
                [
                    DefenseRoll.DescribeEvent(),
                    HitRoll.DescribeEvent(),
                    DamageRoll.DescribeEvent()
                ]);
        }

        return new GameEventDescription(
            $"{Request.Attacker.Name}'s strike against {Request.Defender.Name} inflicted {DamageInflicted} damage!",
            [
                DefenseRoll.DescribeEvent(),
                HitRoll.DescribeEvent(),
                DamageRoll.DescribeEvent()
            ]);
    }
}
