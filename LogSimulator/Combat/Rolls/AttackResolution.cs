using LogSimulator.Logging;
using LogSimulator.Rolls;

namespace LogSimulator.Combat.Rolls;

public record AttackResolution(
    AttackRequest Request,
    RollResolution DefenseRoll,
    CheckedRollResolution HitRoll,
    RollResolution? DamageRoll
) : ILoggableGameEvent
{
    public bool IsMiss => !IsHit;
    public bool IsHit => HitRoll.IsSuccess;

    public bool IsCriticalHit => AttackRequest.IsHitRollCritical(HitRoll);

    public int DamageInflicted => DamageRoll?.RolledTotal ?? 0;

    public IEnumerable<GameEventLog> Log()
    {
        string message;
        if (DamageRoll is null)
        {
            message =
                $"{Request.Attacker.Name}'s strike against {Request.Defender.Name}' missed! ({DamageInflicted} damage)";
        }
        else
        {
            message = IsCriticalHit
                ? $"{Request.Attacker.Name}'s [CRITICAL] strike against {Request.Defender.Name} inflicted {DamageInflicted} damage!"
                : $"{Request.Attacker.Name}'s strike against {Request.Defender.Name} inflicted {DamageInflicted} damage!";
        }

        yield return GameEventLog.CombatActionEvent(message);

        foreach (var log in DefenseRoll.Log())
        {
            yield return log;
        }

        foreach (var log in HitRoll.Log())
        {
            yield return log;
        }

        foreach (var log in DamageRoll?.Log() ?? [])
        {
            yield return log;
        }
    }
}
