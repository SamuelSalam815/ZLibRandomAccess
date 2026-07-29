namespace LogSimulator.Logging;

public record Timestamped<T>(DateTimeOffset Timestamp, T Payload);