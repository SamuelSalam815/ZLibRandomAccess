namespace LogSimulator.Rolls;

public class CheckedRollBuilder(
    string question,
    AbilityScore? testedAbility,
    int baseDiceCount,
    int diceFaceCount,
    AdvantageRating advantageRating,
    int difficulty
)
{
    public const int StandardBaseDiceCount = 2;

    public const int StandardDiceFaceCount = 6;

    public const int StandardTestDifficulty = 10;

    public static CheckedRollBuilder Test(string question) =>
        new(
            question,
            null,
            StandardBaseDiceCount,
            StandardDiceFaceCount,
            AdvantageRating.Zero,
            StandardTestDifficulty);

    public CheckedRollBuilder With(AbilityScore newTestedAbility) => new(
        question,
        newTestedAbility,
        baseDiceCount,
        diceFaceCount,
        advantageRating,
        difficulty
    );

    public CheckedRollBuilder Roll(int newBaseDiceCount) => new(
        question,
        testedAbility,
        newBaseDiceCount,
        diceFaceCount,
        advantageRating,
        difficulty
    );

    public CheckedRollBuilder D(int newDiceFaceCount) => new(
        question,
        testedAbility,
        baseDiceCount,
        newDiceFaceCount,
        advantageRating,
        difficulty
    );

    public CheckedRollBuilder With(AdvantageRating newAdvantageRating) =>
        new(
        question,
        testedAbility,
        baseDiceCount,
        diceFaceCount,
        newAdvantageRating,
        difficulty
    );

    public CheckedRollBuilder WithAdvantage(int magnitude) => With(new AdvantageRating(magnitude));

    public CheckedRollBuilder WithDisadvantage(int magnitude) => With(new AdvantageRating(-magnitude));

    public CheckedRollBuilder AgainstDifficulty(int newDifficulty) => new(
        question,
        testedAbility,
        baseDiceCount,
        diceFaceCount,
        advantageRating,
        newDifficulty
    );

    public CheckedRollResolution ResolveWith(params int[] rolls)
    {
        return CheckedRollResolution.CreateFrom(Create(), rolls);
    }

    public CheckedRollRequest Create()
    {
        return new CheckedRollRequest(
            question,
            testedAbility,
            baseDiceCount,
            diceFaceCount,
            advantageRating,
            difficulty
        );
    }

    public static implicit operator CheckedRollRequest(CheckedRollBuilder builder) => builder.Create();
};
