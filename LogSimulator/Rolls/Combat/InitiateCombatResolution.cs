using LogSimulator.Logging;

namespace LogSimulator.Rolls.Combat;

public record InitiateCombatResolution(InitiateCombatRequest Request, RollResolution HeroFortitudeRoll, RollResolution AdversaryFortitudeRoll) : IDescribableGameEvent
{
    public GameEventDescription DescribeEvent()
    {
        return new GameEventDescription(
            $"{Request.Hero.Name} begins battle with {HeroFortitudeRoll.RolledTotal} vitality and {Request.Adversary.Name} begins battle with {AdversaryFortitudeRoll.RolledTotal} vitality!",
            [
                HeroFortitudeRoll.DescribeEvent(),
                AdversaryFortitudeRoll.DescribeEvent()
            ]
        );
    }
}
