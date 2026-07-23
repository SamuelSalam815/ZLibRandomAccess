namespace LogSimulator.Rolls;

/// <summary>
/// Represents the result of performing a roll specified by <see cref="RollRequest"/>
/// </summary>
/// <param name="RollRequest">The request for a roll.</param>
/// <param name="RolledValues">The result of each die roll.</param>
public record struct RollResult(RollRequest RollRequest, int[] RolledValues)
{
    public int ResolvedValue => RolledValues.OrderDescending().Take(RollRequest.BaseDieCount).Sum() + RollRequest.Modifier;
}
