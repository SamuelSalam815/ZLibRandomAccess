using System.Collections.Immutable;
using LogSimulator.Logging;

namespace LogSimulator.Rolls;

public sealed record RollResolution : IDescribableGameEvent
{
    public RollRequest Request { get; }
    public ImmutableArray<int> RollsWithRerolls { get; }
    public ImmutableArray<int> RollsSelected { get; }
    public ImmutableArray<int> RollsDiscarded { get; }
    public int RolledTotal { get; }

    private RollResolution(
        RollRequest request,
        ImmutableArray<int> rollsWithRerolls,
        ImmutableArray<int> rollsSelected,
        ImmutableArray<int> rollsDiscarded,
        int rolledTotal)
    {
        Request = request;
        RollsWithRerolls = rollsWithRerolls;
        RollsSelected = rollsSelected;
        RollsDiscarded = rollsDiscarded;
        RolledTotal = rolledTotal;
    }

    public static RollResolution CreateFrom(RollRequest request, int[] rolls)
    {
        if (rolls.Length != request.TotalNumberOfDiceRequested)
        {
            throw new ArgumentException(
                $"The number of provided rolls ({rolls}) does not equal the total requested number ({request.TotalNumberOfDiceRequested}");
        }

        var rollsOutOfRange = rolls.Where(roll => roll < 1 || roll > request.DiceFaceCount).ToArray();
        if (rollsOutOfRange.Length != 0)
        {
            throw new ArgumentException(
                $"Encountered rolled values are outside of the expected range: {string.Join(", ", rollsOutOfRange)}. Expected range [1, {request.DiceFaceCount}]");
        }

        (int Value, int Index)[] orderedRolls = rolls
            .Select((value, i) => (value, i))
            .ToArray();

        if (request.IsAdvantaged)
        {
            orderedRolls = orderedRolls.OrderDescending().ToArray();
        }

        if (request.IsDisadvantaged)
        {
            orderedRolls = orderedRolls.Order().ToArray();
        }

        var selectedRolls = orderedRolls
            .Take(request.BaseDiceCount)
            .OrderBy(roll => roll.Index)
            .Select(roll => roll.Value)
            .ToArray();

        var discardedRolls = orderedRolls
            .Skip(request.BaseDiceCount)
            .OrderBy(roll => roll.Index)
            .Select(roll => roll.Value)
            .ToArray();

        var abilityModifier = request.TestedAbility?.Value ?? 0;
        var rolledTotal = selectedRolls.Sum() + abilityModifier;

        return new RollResolution(
            request,
            [..rolls],
            [..selectedRolls],
            [..discardedRolls],
            rolledTotal);
    }

    public void LogEvent(GameEventLogger logger)
    {
        var modifier = Request.TestedAbility?.Value ?? 0;
        var modifierString = modifier >= 0 ? $"+{modifier}" : modifier.ToString();
        logger.Log(
            "Rolled a total of {0} using {1}d{2}{3} with {4} advantage",
            RolledTotal,
            Request.BaseDiceCount,
            Request.DiceFaceCount,
            modifierString,
            Request.AdvantageRating.Value);

        if (Request.TestedAbility is {Value: var abilityModifier, Name: var abilityName})
        {
            logger.Log("Added {0} to total from '{1}' ability score", abilityModifier, abilityName);
        }
        else
        {
            abilityModifier = 0;
            logger.Log("Modifier is {0} from the roll having no associated ability", abilityModifier);
        }

        if (RollsDiscarded.Length > 0)
        {
            logger.Log("Rolling {0} additional dice due to advantage rating ({1})", Request.AdvantageRating.AdditionalDice,  Request.AdvantageRating.Value);
        }

        if (Request.IsAdvantaged)
        {
            logger.Log("Positive advantage rating causes the top {0} dice rolls to be used in the final total", Request.BaseDiceCount);
        }

        if (Request.IsDisadvantaged)
        {
            logger.Log("Negative advantage rating (disadvantage) causes the bottom {0} dice rolls to be used in the final total", Request.BaseDiceCount);
        }

        if (RollsDiscarded.Length > 0)
        {
            logger.Log("Dice Rolls: [{0}]", string.Join(", ", RollsWithRerolls));
            logger.Log("Discarded Rolls: [{0}]", string.Join(", ", RollsDiscarded));
            logger.Log("Chosen Rolls: [{0}]", string.Join(", ", RollsSelected));
            logger.Log("Final total: {0} + {1} = {2}", RollsSelected.Sum(), abilityModifier, RolledTotal);
            return;
        }

        logger.Log("Dice Rolls: [{0}]", string.Join(", ", RollsSelected));
        logger.Log("Final total: {0} + {1} = {2}", RollsSelected.Sum(), abilityModifier, RolledTotal);
    }
}
