namespace Benchmarking.Helpers;

public record BarChartBar<TCategory, TSeries>(TCategory Category, TSeries Series, double Value);
