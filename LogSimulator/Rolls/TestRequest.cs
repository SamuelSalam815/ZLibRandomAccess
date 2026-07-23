namespace LogSimulator.Rolls;

/// <summary>
/// Represents parameters used to perform a tested roll against a given difficulty.
/// </summary>
/// <param name="RollRequest">The roll parameters.</param>
/// <param name="TestDifficulty">The value to meet or beat in the test.</param>
public record struct TestRequest(RollRequest RollRequest, int TestDifficulty)
{
    public TestResult ResolveWith(IEnumerable<int> rolls)
    {
        return new TestResult(this, RollRequest.ResolveWith(rolls));
    }

    public TestResult ResolveWith(int[] rolls)
    {
        return new TestResult(this, RollRequest.ResolveWith(rolls));
    }
}

public static class TestRequestExtensions
{
    public static TestRequest WithDifficulty(this RollRequest rollRequest, int difficulty)
    {
        return new TestRequest(rollRequest, difficulty);
    }
}
