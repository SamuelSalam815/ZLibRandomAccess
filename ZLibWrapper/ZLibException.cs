using ZLibBindings.Constants;

namespace ZLibWrapper;

public class ZLibException : Exception
{
    public ZReturnCode ReturnCode { get; }
    public ZLibException(string? message, ZReturnCode returnCode) : base(message)
    {
        ReturnCode = returnCode;
    }
}
