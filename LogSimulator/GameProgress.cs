using LogSimulator.Logging;

namespace LogSimulator;

public record GameProgress(GamePhase? NextGamePhase, List<IDescribableGameEvent> GameEvents);
