using LogSimulator.ChacterSpec;

namespace LogSimulator.Rolls;

public class RollBuilder(
    int baseDiceCount,
    int diceFaceCount,
    AdvantageRating advantageRating,
    RollModifierCollection rollModifiers)
{
    public const int StandardBaseDiceCount = 2;

    public const int StandardDiceFaceCount = 6;

    public const int StandardTestDifficulty = 10;

    public static RollBuilder StandardRoll() => new(
        StandardBaseDiceCount,
        StandardDiceFaceCount,
        AdvantageRating.Zero,
        RollModifierCollection.Empty
    );

    public static RollBuilder Roll(int baseDiceCount) =>
        new(
            baseDiceCount,
            StandardDiceFaceCount,
            AdvantageRating.Zero,
            RollModifierCollection.Empty);

    public RollBuilder Using(AbilityScore testedAbility) => Plus(testedAbility.Modifier);

    public RollBuilder Plus(RollModifier modifier) =>
        new(
            baseDiceCount,
            diceFaceCount,
            advantageRating,
            rollModifiers.Add(modifier));

    public RollBuilder SetBaseDiceCount(int newBaseDiceCount) => new(
        newBaseDiceCount,
        diceFaceCount,
        advantageRating,
        rollModifiers);

    public RollBuilder D(int newDiceFaceCount) => new(
        baseDiceCount,
        newDiceFaceCount,
        advantageRating,
        rollModifiers);

    public RollBuilder With(AdvantageRating newAdvantageRating) =>
        new(
            baseDiceCount,
            diceFaceCount,
            newAdvantageRating,
            rollModifiers);

    public RollBuilder WithAdvantage(int magnitude = 1) => With(new AdvantageRating(magnitude));

    public RollBuilder WithDisadvantage(int magnitude = 1) => With(new AdvantageRating(-magnitude));

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
            baseDiceCount,
            diceFaceCount,
            advantageRating,
            rollModifiers
        );
    }
}
