namespace LogSimulator;

public record CharacterStatistics(
    int Fortitude,
    int Adaptability,
    int Prowess
)
{
    public int Total => Fortitude + Adaptability + Prowess;
};
