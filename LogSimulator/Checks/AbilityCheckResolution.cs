namespace LogSimulator.Checks;

public abstract partial record AbilityCheckResolution
{
    public AbilityCheckRequest Request { get; }

    private AbilityCheckResolution(AbilityCheckRequest request)
    {
        Request = request;
    }

    public abstract bool IsSuccess();

    public abstract GameEventDescription GetDescription();

    public static implicit operator bool(AbilityCheckResolution resolution) => resolution.IsSuccess();
}
