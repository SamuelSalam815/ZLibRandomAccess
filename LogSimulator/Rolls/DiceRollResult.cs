namespace LogSimulator.Rolls;

public record DiceRollResult
{
    public DiceRollRequest DiceRollRequest { get; }
    public int[] DieRolls { get; }
    public int[] DiscardedRolls { get; }

    public int RollTotal { get; }

    public GameEventDescription RollDescription { get; }

    private DiceRollResult(
        DiceRollRequest diceRollRequest,
        int[] dieRolls,
        int[] discardedRolls,
        int rollTotal,
        GameEventDescription rollDescription)
    {
        DiceRollRequest = diceRollRequest;
        DieRolls = dieRolls;
        DiscardedRolls = discardedRolls;
        RollTotal = rollTotal;
        RollDescription = rollDescription;
    }

    public static DiceRollResult Create(
        DiceRollRequest diceRollRequest,
        int[] rolls,
        GameEventDescription rollDescription)
    {
        if (rolls.Length != diceRollRequest.RequiredRollCount)
        {
            throw new ArgumentException(
                $"Provided number of die rolls ({rolls.Length}) does not match the requested number of rolls ({diceRollRequest.BaseDieCount})");
        }

        if (rolls.Any(roll => roll < 1 || roll > diceRollRequest.DieSize))
        {
            var rollsString = string.Join(", ", rolls);
            throw new ArgumentException(
                $"Argument '{nameof(rolls)}'={rollsString} contains one or more rolls outside of the requested range for dice with {diceRollRequest.DieSize} sides: [1, {diceRollRequest.DieSize}]");
        }

        var orderedRolls = diceRollRequest.ExtraRollRequest switch
        {
            null => rolls,
            ExtraRollRequest.Advantage => rolls.OrderDescending().ToArray(),
            ExtraRollRequest.Disadvantage => rolls.Order().ToArray(),
            _ => throw UnexpectedCaseException.CreateFrom(diceRollRequest.ExtraRollRequest)
        };

        var selectedRolls = orderedRolls.Take(diceRollRequest.BaseDieCount).ToArray();
        var discardedRolls = orderedRolls.Skip(diceRollRequest.BaseDieCount).ToArray();

        rollDescription.AddLine("Discarded rolls: [{0}]", string.Join(", ", discardedRolls));
        if (discardedRolls.Length != diceRollRequest.ExtraRollRequest.GetDieCount())
        {
            throw new ArgumentException(
                $"Provided number of discarded rolls ({discardedRolls.Length}) does not match the requested number of extra rolls ({diceRollRequest.ExtraRollRequest.GetDieCount()})");
        }

        var totalRoll = selectedRolls.Sum();
        rollDescription.AddLine("Total Roll: {0} = {1}", string.Join(" + ", selectedRolls), totalRoll);
        return new DiceRollResult(diceRollRequest, selectedRolls, discardedRolls, totalRoll, rollDescription);
    }

    public bool Against(Test test, out GameEventDescription testDescription)
    {
        var didSucceed = test.AttemptWith(RollTotal, out testDescription);
        testDescription.Append(RollDescription);
        return didSucceed;
    }
}
