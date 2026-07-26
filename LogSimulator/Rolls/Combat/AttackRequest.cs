using LogSimulator.ChacterSpec;

namespace LogSimulator.Rolls.Combat;

public record AttackRequest(Character Attacker, Character Defender)
{
    public AttackResolution ResolveWith(DieRollGenerator rollGenerator)
    {
        var defenseRoll = RollBuilder
            .Roll(1)
            .D(6)
            .Using(Defender.Agility)
            .Plus(new RollModifier("Defender Bonus", 3))
            .ResolveWith(rollGenerator);
        var hitRoll = RollBuilder
            .StandardRoll()
            .Using(Attacker.Prowess)
            .AgainstDifficulty(
                defenseRoll.RolledTotal,
                $"Will {Attacker.Name} successfully land a blow on {Defender.Name}?")
            .ResolveWith(rollGenerator);

        if (!hitRoll.IsSuccess)
        {
            return new AttackResolution(this, defenseRoll, hitRoll, null);
        }

        var damageRoll = RollBuilder.Roll(1).D(6).Using(Attacker.Prowess);
        if (IsHitRollCritical(hitRoll))
        {
            damageRoll = damageRoll
                .SetBaseDiceCount(2)
                .Plus(new RollModifier("Critical Hit", 6));
        }

        return new AttackResolution(this, defenseRoll, hitRoll, damageRoll.ResolveWith(rollGenerator));
    }

    public static bool IsHitRollCritical(CheckedRollResolution hitRoll) =>
        hitRoll.Roll.RolledTotal >= hitRoll.Request.Difficulty + 6;
}
