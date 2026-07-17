namespace ZLibWrapper;

internal enum ZLibWriteAction
{
    CompleteInput = 0,
    RequestMoreOutputSpace = 1,
    Continue = 2,
    FailToDecide = -1,
}
