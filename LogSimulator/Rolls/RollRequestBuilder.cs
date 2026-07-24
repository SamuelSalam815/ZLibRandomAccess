namespace LogSimulator.Rolls;

public record RollRequestBuilder(int DieCount, int DieSize, ExtraRollRequest? ExtraRollRequest = null)
{
    private const int StandardRollCount = 2;
    private const int StandardDieSize = 6;

    public static RollRequestBuilder Roll(int dieCount) => new(dieCount, StandardDieSize);

    public static RollRequestBuilder StandardRoll() => new(StandardRollCount, StandardDieSize);

    public RollRequestBuilder D(int dieSize) => this with {DieSize = dieSize};

    public RollRequestBuilder WithAdvantage(int count = 1) =>
        this with { ExtraRollRequest = new ExtraRollRequest.Advantage(count) };

    public RollRequestBuilder WithDisadvantage(int count = 1) =>
        this with { ExtraRollRequest = new ExtraRollRequest.Disadvantage(count) };

    public RollRequestBuilder With(ExtraRollRequest extraRollRequest) => this with { ExtraRollRequest = extraRollRequest };

    public DiceRollRequest Create() => new(DieCount, DieSize, ExtraRollRequest);

    public static implicit operator DiceRollRequest(RollRequestBuilder rollRequestBuilder)
    {
        return rollRequestBuilder.Create();
    }
}
