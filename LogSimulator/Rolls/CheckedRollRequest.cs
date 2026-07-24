namespace LogSimulator.Rolls;

public record CheckedRollRequest(
    string Question,
    AbilityScore? TestedAbility,
    int BaseDiceCount,
    int DiceFaceCount,
    AdvantageRating AdvantageRating,
    int Difficulty
    )
{
    public bool IsAdvantaged => AdvantageRating.IsAdvantaged;

    public bool IsDisadvantaged => AdvantageRating.IsDisadvantaged;
    public int TotalNumberOfDiceRequested => BaseDiceCount + AdvantageRating.AdditionalDice;

    public AutomaticCheckedRollResolution AutoSucceed() => new AutomaticCheckedRollResolution(this, true);
    public IRollResolution AutoFail() => new AutomaticCheckedRollResolution(this, false);

    public CheckedRollResolution ResolveWith(DieRollGenerator dieRollGenerator)
    {
        var rolls = new int[TotalNumberOfDiceRequested];
        for (var i = 0; i < rolls.Length; i++)
        {
            rolls[i] = dieRollGenerator(DiceFaceCount);
        }

        return CheckedRollResolution.CreateFrom(this, rolls);
    }
}
