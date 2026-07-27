namespace LogSimulator.Rolls;

public record RollRequest(
    int BaseDiceCount,
    int DiceFaceCount,
    AdvantageRating AdvantageRating,
    ModifierCollection TotalRollModifiers,
    ModifierCollection BaseDiceCountModifiers,
    string? QuantityName)
{
    public bool IsAdvantaged => AdvantageRating.IsAdvantaged;

    public bool IsDisadvantaged => AdvantageRating.IsDisadvantaged;
    public int TotalNumberOfDiceRequested => BaseDiceCount + BaseDiceCountModifiers.Total + AdvantageRating.AdditionalDice;

    public int NumberOfDiceToKeep => BaseDiceCount + BaseDiceCountModifiers.Total;

    public RollResolution ResolveWith(DieRollGenerator dieRollGenerator)
    {
        var rolls = new int[TotalNumberOfDiceRequested];
        for (var i = 0; i < rolls.Length; i++)
        {
            rolls[i] = dieRollGenerator(DiceFaceCount);
        }

        return ResolveWith(rolls);
    }

    public RollResolution ResolveWith(params int[] rolls)
    {
        return RollResolution.CreateFrom(this, rolls);
    }
}
