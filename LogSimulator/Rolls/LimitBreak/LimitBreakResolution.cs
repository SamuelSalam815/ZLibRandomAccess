using LogSimulator.ChacterSpec;
using LogSimulator.Logging;

namespace LogSimulator.Rolls.LimitBreak;

public record LimitBreakResolution(GameState GameState, CheckedRollResolution LimitBreakRoll) : IDescribableGameEvent
{
    public bool IsSuccess => LimitBreakRoll.IsSuccess;

    public static LimitBreakResolution CreateFrom(GameState gameState, DieRollGenerator dieRollGenerator)
    {
        var hero = gameState.Hero;
        var limitBreakRoll = RollBuilder.RollFor(6)
            .D(6)
            .AgainstDifficulty(36, $"Will {hero.Name} break their limits?")
            .ResolveWith(dieRollGenerator);

        if (!limitBreakRoll.IsSuccess)
        {
            return new LimitBreakResolution(gameState, limitBreakRoll);
        }

        var limitBrokenHero = hero with
        {
            Stats = new StatBlock(
                hero.Fortitude.Value + 6,
                hero.Agility.Value + 6,
                hero.Prowess.Value + 6
            )
        };

        return new LimitBreakResolution(gameState with
        {
            Hero = limitBrokenHero,
            NumberOfLimitBreaks = gameState.NumberOfLimitBreaks + 1
        }, limitBreakRoll);
    }

    public GameEventDescription DescribeEvent()
    {
        return new GameEventDescription(
            IsSuccess
                ? $"[LIMIT BREAK] {GameState.Hero.Name} rises again with renewd vigor!"
                : $"{GameState.Hero.Name} fails to overcome their limits...",
            [LimitBreakRoll.DescribeEvent()]);
    }
};
