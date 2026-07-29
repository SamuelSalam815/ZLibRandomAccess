using LogSimulator.ChacterSpec;
using LogSimulator.Rolls;

namespace LogSimulator.Combat.Rolls;

public record AttackRequest(Character Attacker, Character Defender)
{
    public AttackResolution ResolveWith(DieRollGenerator rollGenerator)
    {
        var defenseRoll = RollBuilder
            .For($"{Defender.Name}'s Combat Evasion")
            .Roll(1)
            .D(6)
            .Plus(Defender.Agility)
            .Plus(new Modifier("Defender Bonus", 3))
            .ResolveWith(rollGenerator);

        var hitRoll = RollBuilder
            .StandardRoll()
            .Plus(Attacker.Prowess)
            .AgainstDifficulty(
                defenseRoll.RolledTotal,
                $"Will {Attacker.Name} successfully land a blow on {Defender.Name}?")
            .ResolveWith(rollGenerator);

        if (!hitRoll.IsSuccess)
        {
            return new AttackResolution(this, defenseRoll, hitRoll, null);
        }

        var damageRoll = RollBuilder.For($"{Attacker.Name}'s Damage").Roll(1).D(6).Plus(Attacker.Prowess);
        if (IsHitRollCritical(hitRoll))
        {
            const string modifierName = "Critical Hit Bonus";
            damageRoll = damageRoll.ModifyDiceCount(new Modifier(modifierName, 1)).Plus(new Modifier(modifierName, 6));
        }

        return new AttackResolution(this, defenseRoll, hitRoll, damageRoll.ResolveWith(rollGenerator));
    }

    public static bool IsHitRollCritical(CheckedRollResolution hitRoll) =>
        hitRoll.Roll.RolledTotal >= hitRoll.Request.Difficulty + 6;
}
