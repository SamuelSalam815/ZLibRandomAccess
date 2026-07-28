using System.Collections.Immutable;
using System.Text;
using LogSimulator.Logging;

namespace LogSimulator.Rolls;

public sealed record RollResolution : ILoggableGameEvent
{
    public RollRequest Request { get; }
    public ImmutableArray<int> RollPool { get; }
    public ImmutableArray<int> RollsSelected { get; }
    public ImmutableArray<int> RollsPruned { get; }
    public int RolledTotal { get; }

    private RollResolution(
        RollRequest request,
        ImmutableArray<int> rollPool,
        ImmutableArray<int> rollsSelected,
        ImmutableArray<int> rollsPruned,
        int rolledTotal)
    {
        Request = request;
        RollPool = rollPool;
        RollsSelected = rollsSelected;
        RollsPruned = rollsPruned;
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
            .Take(request.NumberOfDiceToKeep)
            .OrderBy(roll => roll.Index)
            .Select(roll => roll.Value)
            .ToArray();

        var discardedRolls = orderedRolls
            .Skip(request.NumberOfDiceToKeep)
            .OrderBy(roll => roll.Index)
            .Select(roll => roll.Value)
            .ToArray();

        var rolledTotal = selectedRolls.Sum() + request.TotalRollModifiers.Total;

        return new RollResolution(
            request,
            [..rolls],
            [..selectedRolls],
            [..discardedRolls],
            rolledTotal);
    }

    public GameEventLogTree Log()
    {
        return GameEventLog.RollEvent(GetPrimaryDescription())
            .AddDirectChildren(LogDiceCountModifiers())
            .AddDirectChildren(LogDiceSum())
            .AddDirectChildren(LogRolledTotalModifiers());
    }

    private string GetPrimaryDescription()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder
            .Append("Rolled ")
            .Append(RolledTotal);

        if (Request.QuantityName is { } namedQuantity)
        {
            stringBuilder.Append(" for ").Append(namedQuantity);
        }

        stringBuilder.Append(" using ")
            .Append(Request.NumberOfDiceToKeep)
            .Append('d')
            .Append(Request.DiceFaceCount);

        if (!Request.TotalRollModifiers.IsEmpty)
        {
            if (Request.TotalRollModifiers.Total >= 0)
            {
                stringBuilder.Append('+');
            }

            stringBuilder.Append(Request.TotalRollModifiers.Total);
        }

        if (Request.IsAdvantaged)
        {
            stringBuilder.Append(" with advantage");
        }

        if (Request.IsDisadvantaged)
        {
            stringBuilder.Append(" with disadvantage");
        }

        if (Request.AdvantageRating.AdditionalDice > 1)
        {
            stringBuilder.Append(' ').Append(Request.AdvantageRating.AdditionalDice);
        }

        return stringBuilder.ToString();
    }

    private IEnumerable<GameEventLogTree> LogRolledTotalModifiers()
    {
        foreach (var (modifierName, modifierValue) in Request.TotalRollModifiers.Set)
        {
            var modifierDescription = new StringBuilder()
                .Append("Added ");
            if (modifierValue >= 0)
            {
                modifierDescription.Append('+');
            }

            modifierDescription
                .Append(modifierValue)
                .Append($" to final roll from modifier '{modifierName}'");

            yield return GameEventLog.RollEvent(modifierDescription.ToString());
        }
    }

    private IEnumerable<GameEventLogTree> LogDiceCountModifiers()
    {
        foreach (var (modifierName, modifierValue) in Request.BaseDiceCountModifiers.Set)
        {
            var modifierDescription = new StringBuilder()
                .Append("Added ");
            if (modifierValue >= 0)
            {
                modifierDescription.Append('+');
            }

            modifierDescription
                .Append(modifierValue)
                .Append($" dice to the final roll from modifier '{modifierName}'");

            yield return GameEventLog.RollEvent(modifierName);
        }
    }

    private IEnumerable<GameEventLogTree> LogDiceSum()
    {
        if (Request.IsAdvantaged || Request.IsDisadvantaged)
        {
            yield return GameEventLog.RollEvent($"Roll pool: [{string.Join(", ", RollPool)}]");
            yield return GameEventLog.RollEvent($"Rolls pruned: [{string.Join(", ", RollsPruned)}]");
            yield return GameEventLog.RollEvent($"Rolls selected: [{string.Join(" + ", RollsSelected)}] = {RollsSelected.Sum()}");
        }
        else
        {
            yield return GameEventLog.RollEvent($"Rolled: [{string.Join(" + ", RollsSelected)}] = {RollsSelected.Sum()}");
        }
    }
}
