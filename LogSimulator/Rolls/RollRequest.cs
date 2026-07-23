namespace LogSimulator.Rolls;

/// <summary>
/// Represents a request to roll dice
/// </summary>
/// <param name="BaseDieCount">The base number of die to use in the total roll. This may be adjusted by <see cref="Advantage"/>.</param>
/// <param name="DieSize">How many faces each die has.</param>
/// <param name="Modifier">A flat value to add to the result of a roll.</param>
/// <param name="Advantage">
///     The number of additional dice to roll. This number of dice will also be discarded before determining the total
///     where dice are discarded by the lowest value first.
/// </param>
public readonly record struct RollRequest(int BaseDieCount, int DieSize, int Modifier, int Advantage)
{
    public int RequiredRollCount => BaseDieCount + Advantage;

    public const int DefaultDieSize = 6;

    /// <summary>
    /// Builds a roll request for the given number of dice at the <see cref="DefaultDieSize"/>
    /// </summary>
    public static RollRequest Roll(int baseDieCount) => new(baseDieCount, DefaultDieSize, 0, 0);

    /// <summary>
    /// Creates a new roll request by replacing the die size.
    /// </summary>
    public RollRequest D(int newDieSize) => this with { DieSize = newDieSize };

    /// <summary>
    /// Creates a new roll request by replacing the roll modifier.
    /// </summary>
    public RollRequest Plus(int newModifier) => this with { Modifier = newModifier };

    /// <summary>
    /// Creates a new roll request by replacing the advantage rating.
    /// </summary>
    public RollRequest WithAdvantage(int newAdvantage = 1) => this with { Advantage = newAdvantage };

    /// <summary>
    /// Resolves the request by providing the list of values actually rolled.
    /// This should consist of <see cref="RequiredRollCount"/> values in the range [1, <see cref="DieSize"/>]
    /// </summary>
    public RollResult ResolveWith(IEnumerable<int> rolledValues)
    {
        return ResolveWith(rolledValues.ToArray());
    }

    /// <summary>
    /// Resolves the request by providing the list of values actually rolled.
    /// This should consist of <see cref="RequiredRollCount"/> values in the range [1, <see cref="DieSize"/>]
    /// </summary>
    public RollResult ResolveWith(int[] rolledValues)
    {
        if (rolledValues.Length != RequiredRollCount)
        {
            throw new ArgumentException(
                $"Did not receive the required amount of rolled values! (Required {RequiredRollCount} rolls, but got {rolledValues.Length} rolls instead!)");
        }

        var diceSize = DieSize;
        var invalidRolls = rolledValues.Where(roll => roll < 1 || roll > diceSize).ToList();

        if (invalidRolls.Count > 0)
        {
            throw new ArgumentException(
                $"Received values [{string.Join(",", invalidRolls)}] which are out of range of the valid die rolls for this request (each roll may only be in the range [{1}, {DieSize}]", nameof(rolledValues));
        }

        return new RollResult(this, rolledValues);
    }
};
