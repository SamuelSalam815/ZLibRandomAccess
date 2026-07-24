namespace LogSimulator.Checks;

public class AbilityCheckBuilder(
    string Question,
    AbilityScore? TestedAbility,
    int BaseDiceCount,
    int DiceFaceCount,
    AdvantageRating AdvantageRating,
    int Difficulty
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
        Question,
        newTestedAbility,
        BaseDiceCount,
        DiceFaceCount,
        AdvantageRating,
        Difficulty
    );

    public AbilityCheckBuilder Roll(int newBaseDiceCount) => new(
        Question,
        TestedAbility,
        newBaseDiceCount,
        DiceFaceCount,
        AdvantageRating,
        Difficulty
    );

    public AbilityCheckBuilder D(int newDiceFaceCount) => new(
        Question,
        TestedAbility,
        BaseDiceCount,
        newDiceFaceCount,
        AdvantageRating,
        Difficulty
    );

    public AbilityCheckBuilder With(AdvantageRating newAdvantageRating) =>
        new(
        Question,
        TestedAbility,
        BaseDiceCount,
        DiceFaceCount,
        newAdvantageRating,
        Difficulty
    );

    public AbilityCheckBuilder WithAdvantage(int magnitude) => With(new AdvantageRating(magnitude));

    public AbilityCheckBuilder WithDisadvantage(int magnitude) => With(new AdvantageRating(-magnitude));

    public AbilityCheckBuilder AgainstDifficulty(int newDifficulty) => new(
        Question,
        TestedAbility,
        BaseDiceCount,
        DiceFaceCount,
        AdvantageRating,
        newDifficulty
    );

    public AbilityCheckRequest Create()
    {
        return new AbilityCheckRequest(
            Question,
            TestedAbility,
            BaseDiceCount,
            DiceFaceCount,
            AdvantageRating,
            Difficulty
        );
    }

    public static implicit operator AbilityCheckRequest(AbilityCheckBuilder builder) => builder.Create();
};
