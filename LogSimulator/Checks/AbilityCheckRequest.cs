namespace LogSimulator.Checks;

[Flags]
public enum ResolutionMethods
{
    None = 0,
    DieRoll = 1,
}

public record AbilityCheckRequest(
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

    public AbilityCheckResolution AutoSucceed() => new AbilityCheckResolution.ByDeclaration(this, true);
    public AbilityCheckResolution AutoFail() => new AbilityCheckResolution.ByDeclaration(this, false);

    public AbilityCheckResolution ResolveWith(DieRollGenerator dieRollGenerator)
    {
        var rolls = new int[TotalNumberOfDiceRequested];
        for (var i = 0; i < rolls.Length; i++)
        {
            rolls[i] = dieRollGenerator(DiceFaceCount);
        }

        return AbilityCheckResolution.ByRolling.CreateFrom(this, rolls);
    }
}
