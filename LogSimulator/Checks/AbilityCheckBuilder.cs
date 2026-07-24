namespace LogSimulator.Checks;

public class AbilityCheckBuilder(
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

    public static AbilityCheckBuilder Test(string question) =>
        new(
            question,
            null,
            StandardBaseDiceCount,
            StandardDiceFaceCount,
            AdvantageRating.Zero,
            StandardTestDifficulty);

    public AbilityCheckBuilder With(AbilityScore newTestedAbility) => new(
        question,
        newTestedAbility,
        baseDiceCount,
        diceFaceCount,
        advantageRating,
        difficulty
    );

    public AbilityCheckBuilder Roll(int newBaseDiceCount) => new(
        question,
        testedAbility,
        newBaseDiceCount,
        diceFaceCount,
        advantageRating,
        difficulty
    );

    public AbilityCheckBuilder D(int newDiceFaceCount) => new(
        question,
        testedAbility,
        baseDiceCount,
        newDiceFaceCount,
        advantageRating,
        difficulty
    );

    public AbilityCheckBuilder With(AdvantageRating newAdvantageRating) =>
        new(
        question,
        testedAbility,
        baseDiceCount,
        diceFaceCount,
        newAdvantageRating,
        difficulty
    );

    public AbilityCheckBuilder WithAdvantage(int magnitude) => With(new AdvantageRating(magnitude));

    public AbilityCheckBuilder WithDisadvantage(int magnitude) => With(new AdvantageRating(-magnitude));

    public AbilityCheckBuilder AgainstDifficulty(int newDifficulty) => new(
        question,
        testedAbility,
        baseDiceCount,
        diceFaceCount,
        advantageRating,
        newDifficulty
    );

    public AbilityCheckResolution.ByRolling ResolveWith(params int[] rolls)
    {
        return AbilityCheckResolution.ByRolling.CreateFrom(Create(), rolls);
    }

    public AbilityCheckRequest Create()
    {
        return new AbilityCheckRequest(
            question,
            testedAbility,
            baseDiceCount,
            diceFaceCount,
            advantageRating,
            difficulty
        );
    }

    public static implicit operator AbilityCheckRequest(AbilityCheckBuilder builder) => builder.Create();
};
