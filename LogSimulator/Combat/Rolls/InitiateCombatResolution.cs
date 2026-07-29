using LogSimulator.Logging;
using LogSimulator.Rolls;

namespace LogSimulator.Combat.Rolls;

public record InitiateCombatResolution(InitiateCombatRequest Request, RollResolution HeroFortitudeRoll, RollResolution AdversaryFortitudeRoll) : ILoggableGameEvent
{
    public IEnumerable<GameEventLog> Log()
    {
        return new[]
            {
                GameEventLog.GamePhaseEvent(
                    $"{Request.Hero.Name} begins battle with {HeroFortitudeRoll.RolledTotal} vitality and {Request.Adversary.Name} begins battle with {AdversaryFortitudeRoll.RolledTotal} vitality!"
                )
            }
            .Concat(HeroFortitudeRoll.Log())
            .Concat(AdversaryFortitudeRoll.Log());
    }
}
