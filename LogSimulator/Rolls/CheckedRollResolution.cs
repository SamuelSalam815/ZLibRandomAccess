using LogSimulator.Logging;

namespace LogSimulator.Rolls;

public sealed record CheckedRollResolution(CheckedRollRequest Request, RollResolution Roll) : IDescribableGameEvent
{
    public bool IsSuccess => Roll.RolledTotal >= Request.Difficulty;

    public static void LogQuestionResolution(bool isAnswerYes, string question, GameEventLogger logger)
    {
        logger.Log("Resolved '{0}' to {1}!", question, isAnswerYes ? "YES" : "NO");
    }

    public void LogEvent(GameEventLogger logger)
    {
        LogQuestionResolution(IsSuccess, Request.Question, logger);
        logger.Log(
            IsSuccess
                ? "Rolled value ({0}) beats test difficulty ({1})"
                : "Rolled value ({0}) fails test difficulty ({1})",
            Roll.RolledTotal,
            Request.Difficulty);
        Roll.LogEvent(logger);
    }
}
