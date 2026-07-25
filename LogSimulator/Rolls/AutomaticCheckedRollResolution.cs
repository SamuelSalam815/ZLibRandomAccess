using LogSimulator.Logging;

namespace LogSimulator.Rolls;

public sealed record AutomaticCheckedRollResolution(CheckedRollRequest Request, bool IsSuccess) : IDescribableGameEvent
{
    public void LogEvent(GameEventLogger logger)
    {
        logger.Log("Automatically resolved question!");
        CheckedRollResolution.LogQuestionResolution(IsSuccess, Request.Question, logger);
    }
}
