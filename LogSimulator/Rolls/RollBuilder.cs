namespace LogSimulator.Rolls;

public class RollBuilder(
    int baseDiceCount,
    int diceFaceCount,
    AdvantageRating advantageRating,
    AbilityScore? testedAbility)
{
    public const int StandardBaseDiceCount = 2;

    public const int StandardDiceFaceCount = 6;

    public const int StandardTestDifficulty = 10;

    public static RollBuilder StandardRoll() => new(
        StandardBaseDiceCount,
        StandardDiceFaceCount,
        AdvantageRating.Zero,
        null
    );

    public static RollBuilder Roll(int baseDiceCount) =>
        new(
            baseDiceCount,
            StandardDiceFaceCount,
            AdvantageRating.Zero,
            null);

    public RollBuilder Using(AbilityScore newTestedAbility) => new(
        baseDiceCount,
        diceFaceCount,
        advantageRating,
        newTestedAbility);

    public RollBuilder SetBaseDiceCount(int newBaseDiceCount) => new(
        newBaseDiceCount,
        diceFaceCount,
        advantageRating,
        testedAbility);

    public RollBuilder D(int newDiceFaceCount) => new(
        baseDiceCount,
        newDiceFaceCount,
        advantageRating,
        testedAbility);

    public RollBuilder With(AdvantageRating newAdvantageRating) =>
        new(
            baseDiceCount,
            diceFaceCount,
            newAdvantageRating,
            testedAbility);

    public RollBuilder WithAdvantage(int magnitude = 1) => With(new AdvantageRating(magnitude));

    public RollBuilder WithDisadvantage(int magnitude = 1) => With(new AdvantageRating(-magnitude));

    public CheckedRollRequest AgainstStandardDifficulty(string testQuestion) =>
        AgainstDifficulty(StandardTestDifficulty, testQuestion);
    public CheckedRollRequest AgainstDifficulty(int difficulty, string testQuestion) => new(
        testQuestion,
        difficulty,
        CreateRequest());

    public RollResolution ResolveWith(params int[] rolls)
    {
        return RollResolution.CreateFrom(CreateRequest(), rolls);
    }

    public RollRequest CreateRequest()
    {
        return new RollRequest(
            baseDiceCount,
            diceFaceCount,
            advantageRating,
            testedAbility
        );
    }
}
