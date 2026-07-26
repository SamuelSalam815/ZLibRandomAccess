using LogSimulator.Logging;

namespace LogSimulator.Rolls.Combat;

public record AttackResolution(
    AttackRequest Request,
    RollResolution DefenseRoll,
    CheckedRollResolution HitRoll,
    RollResolution? DamageRoll
) : IDescribableGameEvent
{
    public bool IsCriticalHit => AttackRequest.IsHitRollCritical(HitRoll);

    public int DamageInflicted => DamageRoll?.RolledTotal ?? 0;

    public void LogEvent(GameEventLogger logger)
    {
        logger.Log("{0}'s evasion gave them a difficulty to hit of {1}", Request.Defender.Name, DefenseRoll.RolledTotal);
        DefenseRoll.LogEvent(logger);
        HitRoll.LogEvent(logger);
        if (DamageRoll is null)
        {
            logger.Log("The strike was evaded!");
        }
        else
        {
            logger.Log("Hit confirmed!");
            if (IsCriticalHit)
            {
                logger.Log("Critical!");
            }

            logger.Log("Rolling for damage...");
            DamageRoll.LogEvent(logger);

            logger.Log("{0} dealt {1} damage to {2}!", Request.Attacker.Name, DamageRoll.RolledTotal, Request.Defender.Name);
        }
    }
}
