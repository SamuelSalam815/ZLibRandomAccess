namespace LogSimulator;

public record GameProgress(GamePhase? NextGamePhase, List<GameEventDescription> GameEvents);
