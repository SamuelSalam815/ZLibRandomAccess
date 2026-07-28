using LogSimulator.Logging;

namespace LogSimulator.Rolls.Combat;

public record InitiateCombatResolution(InitiateCombatRequest Request, RollResolution HeroFortitudeRoll, RollResolution AdversaryFortitudeRoll) : ILoggableGameEvent
{
    public GameEventLogTree Log()
    {
        return GameEventLog.CombatActionEvent(
                $"{Request.Hero.Name} begins battle with {HeroFortitudeRoll.RolledTotal} vitality and {Request.Adversary.Name} begins battle with {AdversaryFortitudeRoll.RolledTotal} vitality!"
            )
            .Add(HeroFortitudeRoll)
            .Add(AdversaryFortitudeRoll);
    }
}
