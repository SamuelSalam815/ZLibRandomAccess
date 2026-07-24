using System.Collections.Immutable;

namespace LogSimulator.Rolls;

public sealed record CheckedRollResolution : IRollResolution
{
    public CheckedRollRequest Request { get; }
    public ImmutableArray<int> RollsWithRerolls { get; }
    public ImmutableArray<int> RollsSelected { get; }
    public ImmutableArray<int> RollsDiscarded { get; }
    public int RolledTotal { get; }

    private CheckedRollResolution(
        CheckedRollRequest request,
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

    public bool IsSuccess() => RolledTotal >= Request.Difficulty;

    public GameEventDescription GetDescription()
    {
        var result = new GameEventDescription();
        result.AddLine("Testing '{0}'...", Request.Question);
        result.AddLine(
            IsSuccess()
                ? "Success! Rolled value={0} beats test difficulty={1}!"
                : "Failure! Rolled value={0} fails test difficulty={1}!",
            RolledTotal,
            Request.Difficulty);

        switch (Request.AdvantageRating)
        {
            case { IsAdvantaged: true, Value: 1 }:
                result.AddLine("Rolled with advantage");
                break;
            case { IsAdvantaged: true, Value: var advantage }:
                result.AddLine("Rolled with advantage {0}", advantage);
                break;
            case { IsDisadvantaged: true, Value: 1 }:
                result.AddLine("Rolled with disadvantage");
                break;
            case { IsDisadvantaged: true, Value: var disadvantage }:
                result.AddLine("Rolled with disadvantage {0}", -disadvantage);
                break;
        }

        result.AddLine("Rolled [{0}]", string.Join(", ", RollsWithRerolls));
        result.AddLine("Kept [{0}] = {1}", string.Join(" + ", RollsSelected), RolledTotal);

        return result;
    }

    public static CheckedRollResolution CreateFrom(CheckedRollRequest request, int[] rolls)
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

        return new CheckedRollResolution(
            request,
            [..rolls],
            [..selectedRolls],
            [..discardedRolls],
            selectedRolls.Sum());
    }
}
