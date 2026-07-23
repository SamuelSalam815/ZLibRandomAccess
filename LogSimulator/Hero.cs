namespace LogSimulator;

public record Hero(
    CharacterStatistics Statistics,
    int CurrentVitality,
    int RemainingExperience
);