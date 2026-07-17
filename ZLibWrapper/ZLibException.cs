using ZLibBindings.Constants;

namespace ZLibWrapper;

public class ZLibException : Exception
{
    public ZReturnCode ReturnCode { get; }
    public ZLibException(string? message, ZReturnCode returnCode) : base(
        $"Encountered return code '{returnCode}' ({(int)returnCode})"
        + (message is null ? string.Empty : $" with error message '{message}'")
        )
    {
        ReturnCode = returnCode;
    }
}
