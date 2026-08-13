using LogSimulator.ChacterSpec;
using LogSimulator.Logging;
using LogSimulator.Rolls;

namespace LogSimulator.Combat.Rolls;

public record LimitBreakResolution(GameState GameState, CheckedRollResolution LimitBreakRoll) : ILoggableGameEvent
{
    public bool IsSuccess => LimitBreakRoll.IsSuccess;

    public static LimitBreakResolution CreateFrom(GameState gameState, DieRollGenerator dieRollGenerator)
    {
        var hero = gameState.Hero;
        var limitBreakRoll = RollBuilder.RollFor(5)
            .D(6)
            .WithAdvantage()
            .AgainstDifficulty(25, $"Will {hero.Name} [LIMIT BREAK] to fight again with greater capabilities?")
            .ResolveWith(dieRollGenerator);

        if (!limitBreakRoll.IsSuccess)
        {
            return new LimitBreakResolution(gameState, limitBreakRoll);
        }

        var limitBrokenHero = hero with
        {
            Stats = new StatBlock(
                hero.Fortitude.Value + 2,
                hero.Agility.Value + 2,
                hero.Prowess.Value + 2
            )
        };

        return new LimitBreakResolution(gameState with
        {
            Hero = limitBrokenHero,
            NumberOfLimitBreaks = gameState.NumberOfLimitBreaks + 1
        }, limitBreakRoll);
    }

    public IEnumerable<GameEventLog> Log() => LimitBreakRoll.Log();
};
