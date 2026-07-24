namespace LogSimulator.Rolls;

public record struct AdvantageRating(int Value)
{
    public static readonly AdvantageRating Zero = new AdvantageRating(0);

    public int AdditionalDice => Math.Abs(Value);
    public bool IsAdvantaged => Value > 0;
    public bool IsDisadvantaged => Value < 0;
}
