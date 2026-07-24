namespace LogSimulator.Rolls;

public record RollBuilder(int DieCount, int DieSize, ExtraRollRequest? ExtraRollRequest = null)
{
    private const int StandardRollCount = 2;
    private const int StandardDieSize = 6;

    public static RollBuilder Roll(int dieCount) => new(dieCount, StandardDieSize);

    public static RollBuilder StandardRoll() => new(StandardRollCount, StandardDieSize);

    public RollBuilder D(int dieSize) => this with {DieSize = dieSize};

    public RollBuilder WithAdvantage(int count = 1) =>
        this with { ExtraRollRequest = new ExtraRollRequest.Advantage(count) };

    public RollBuilder WithDisadvantage(int count = 1) =>
        this with { ExtraRollRequest = new ExtraRollRequest.Disadvantage(count) };

    public RollBuilder With(ExtraRollRequest extraRollRequest) => this with { ExtraRollRequest = extraRollRequest };

    public DiceRollRequest Create() => new(DieCount, DieSize, ExtraRollRequest);

    public static implicit operator DiceRollRequest(RollBuilder rollBuilder)
    {
        return rollBuilder.Create();
    }
}
