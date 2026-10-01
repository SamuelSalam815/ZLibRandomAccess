namespace Benchmarking.Helpers;

public record BarChartConfiguration<TCategory, TSeries>
{
    public required string Title { get; init; }
    public string CategoryAxisLabel { get; init; } = string.Empty;
    public string ValueAxisLabel { get; init; } = string.Empty;
    public string SeriesLabel { get; init; } = string.Empty;
    public Comparer<TCategory> CategoryComparer { get; init; } = Comparer<TCategory>.Default;
    public Comparer<TSeries> SeriesComparer { get; init; } = Comparer<TSeries>.Default;
    public double? MaximumValue { get; init; }
    public double? MinimumValue { get; init; }
}
