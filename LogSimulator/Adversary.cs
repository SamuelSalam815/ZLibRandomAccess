namespace LogSimulator;

public record Adversary(
    string Name,
    CharacterStatistics Statistics,
    int CurrentVitality,
    int ExperienceValue
)
{
    public Adversary(string name, CharacterStatistics statistics) : this(
        name,
        statistics,
        statistics.Fortitude,
        statistics.Total * 10
        )
    {
    }
};
