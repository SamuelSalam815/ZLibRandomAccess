namespace LogSimulator.Logging;

public interface IDescribableGameEvent
{
    public void LogEvent(GameEventLogger logger);
}
