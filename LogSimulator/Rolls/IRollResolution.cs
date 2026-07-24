namespace LogSimulator.Rolls;

public interface IRollResolution
{
    public CheckedRollRequest Request { get; }
    public bool IsSuccess();

    public GameEventDescription GetDescription();
}
