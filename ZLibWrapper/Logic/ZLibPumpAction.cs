namespace ZLibWrapper.Logic;

internal enum ZLibPumpAction
{
    RequestMoreInputSpace = 0,
    RequestMoreOutputSpace = 1,
    Continue = 2,
    CompleteStream = 3,
    FailToDecide = -1,
}
