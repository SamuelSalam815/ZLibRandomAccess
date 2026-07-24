using System.Diagnostics;

namespace LogSimulator.Rolls;

public record ExtraRollRequest
{
    private ExtraRollRequest()
    {
    }

    public sealed record Advantage(int Count) : ExtraRollRequest;
    public sealed record Disadvantage(int Count) : ExtraRollRequest;
}

public static class ExtraRollRequestExtensions
{
    public static int GetDieCount(this ExtraRollRequest? extraRollRequest)
    {
        return extraRollRequest switch
        {
            null => 0,
            ExtraRollRequest.Advantage advantage => advantage.Count,
            ExtraRollRequest.Disadvantage disadvantage => disadvantage.Count,
            _ => throw new UnreachableException(
                $"Switch was thought to be exhaustive on {extraRollRequest.GetType()}, but was not actually exhaustive!")
        };
    }
}
