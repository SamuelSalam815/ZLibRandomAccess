namespace LogSimulator.Rolls;

public record RollRequest(
    int BaseDiceCount,
    int DiceFaceCount,
    AdvantageRating AdvantageRating,
    AbilityScore? TestedAbility
)
{
    public bool IsAdvantaged => AdvantageRating.IsAdvantaged;

    public bool IsDisadvantaged => AdvantageRating.IsDisadvantaged;
    public int TotalNumberOfDiceRequested => BaseDiceCount + AdvantageRating.AdditionalDice;

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
