namespace LogSimulator.Rolls;

/// <summary>
/// The result of performing a tested roll.
/// </summary>
/// <param name="Request">The parameters of the test.</param>
/// <param name="Roll">The actual roll.</param>
public readonly record struct TestResult(TestRequest Request, RollResult Roll)
{
    public bool IsPassed => Roll.ResolvedValue >= Request.TestDifficulty;
};
