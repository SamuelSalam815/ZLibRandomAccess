namespace LogSimulator.Logging;

public interface ILoggableGameEvent
{
    public IEnumerable<GameEventLog> Log();
}
