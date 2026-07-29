using System.Collections.Immutable;

namespace LogSimulator.Logging;

public record GameEventLogs(DateTimeOffset CurrentTime, ImmutableList<Timestamped<GameEventLog>> Logs)
{

    public GameEventLogs(DateTimeOffset startingTime) : this(
        startingTime,
        ImmutableList<Timestamped<GameEventLog>>.Empty)
    {
    }

    private static readonly TimeSpan TimeToRoll = TimeSpan.FromSeconds(3);

    public GameEventLogs Add(GameEventLog log)
    {
        var updatedTime = CurrentTime;
        if (log.EventScope == EventScope.Roll)
        {
            updatedTime += TimeToRoll;
        }

        return new GameEventLogs(CurrentTime: updatedTime, Logs: Logs.Add(new Timestamped<GameEventLog>(updatedTime, log)));
    }

    public GameEventLogs AddRange(IEnumerable<GameEventLog> logs)
    {
        return logs.Aggregate(this, (accumulator, log) => accumulator.Add(log));
    }

    private static string PrettyPrint(Timestamped<GameEventLog> log)
    {
        return $"[{log.Timestamp.UtcDateTime:yyyy-MM-dd HHH:mm:ss}]{log.Payload}";
    }

    public override string ToString()
    {
        return string.Join(Environment.NewLine, Logs.Select(PrettyPrint));
    }
}
