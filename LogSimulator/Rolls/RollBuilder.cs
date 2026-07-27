using LogSimulator.ChacterSpec;

namespace LogSimulator.Rolls;

public record RollBuilder
{
    private RollBuilder(
        int baseDiceCount,
        int diceFaceCount,
        AdvantageRating advantageRating,
        ModifierCollection totalRollModifiers,
        ModifierCollection baseDiceCountModifiers,
        string? quantityName)
    {
        BaseDiceCount = baseDiceCount;
        DiceFaceCount = diceFaceCount;
        AdvantageRating = advantageRating;
        TotalRollModifiers = totalRollModifiers;
        QuantityName = quantityName;
        BaseDiceCountModifiers = baseDiceCountModifiers;
    }

    public const int StandardBaseDiceCount = 2;

    public const int StandardDiceFaceCount = 6;

    public const int StandardTestDifficulty = 10;

    private int BaseDiceCount { get; init; }
    private int DiceFaceCount { get; init; }
    private AdvantageRating AdvantageRating { get; init; }
    private ModifierCollection TotalRollModifiers { get; init; }
    private ModifierCollection BaseDiceCountModifiers { get; init; }
    private string? QuantityName { get; init; }

    public static RollBuilder For(string quantityName) => StandardRoll() with {QuantityName =  quantityName};

    public static RollBuilder RollFor(int baseDiceCount) => StandardRoll().Roll(baseDiceCount);

    public static RollBuilder StandardRoll() => new(
        StandardBaseDiceCount,
        StandardDiceFaceCount,
        AdvantageRating.Zero,
        ModifierCollection.Empty,
        ModifierCollection.Empty,
        string.Empty);


    public RollBuilder Roll(int newBaseDiceCount) => this with {BaseDiceCount = newBaseDiceCount};

    public RollBuilder Plus(AbilityScore testedAbility) => Plus(testedAbility.Modifier);

    public RollBuilder Plus(Modifier modifier) => this with { TotalRollModifiers = TotalRollModifiers.Add(modifier) };

    public RollBuilder D(int newDiceFaceCount) => this with {DiceFaceCount = newDiceFaceCount};

    public RollBuilder With(AdvantageRating newAdvantageRating) => this with { AdvantageRating = newAdvantageRating };

    public RollBuilder WithAdvantage(int magnitude = 1) => With(new AdvantageRating(magnitude));

    public RollBuilder WithDisadvantage(int magnitude = 1) => With(new AdvantageRating(-magnitude));

    public RollBuilder ModifyDiceCount(Modifier modifier) =>
        this with { BaseDiceCountModifiers = BaseDiceCountModifiers.Add(modifier) };

    public CheckedRollRequest AgainstStandardDifficulty(string testQuestion) =>
        AgainstDifficulty(StandardTestDifficulty, testQuestion);
    public CheckedRollRequest AgainstDifficulty(int difficulty, string testQuestion) => new(
        testQuestion,
        difficulty,
        CreateRequest());

    public RollResolution ResolveWith(DieRollGenerator dieRollGenerator)
    {
        return CreateRequest().ResolveWith(dieRollGenerator);
    }

    public RollResolution ResolveWith(params int[] rolls)
    {
        return CreateRequest().ResolveWith(rolls);
    }

    public RollRequest CreateRequest()
    {
        return new RollRequest(
            BaseDiceCount,
            DiceFaceCount,
            AdvantageRating,
            TotalRollModifiers,
            BaseDiceCountModifiers,
            QuantityName);
    }
}
