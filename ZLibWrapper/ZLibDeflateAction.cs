namespace ZLibWrapper;

internal enum ZLibDeflateAction
{
    FailedToDecide,
    FatalError,
    InputBufferConsumed,
    OutputBufferConsumed,
    CallDeflateAgain
}