namespace LogSimulator.Rolls;

public record struct ContestResult(
    ContestRequest ContestRequest,
    RollResult HeroRoll,
    RollResult AdversaryRoll);
