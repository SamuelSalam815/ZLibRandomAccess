using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Benchmarking;

public class NumberColumn : IColumn
{
    private readonly Func<long> _getNumber;

    public string Id { get; }
    public string ColumnName { get; }

    public NumberColumn(string columnName, Func<long> getNumber)
    {
        _getNumber = getNumber;
        ColumnName = columnName;
        Id = nameof(NumberColumn) + "." + ColumnName;
    }

    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
    public string GetValue(Summary summary, BenchmarkCase benchmarkCase) => _getNumber().ToString();

    public bool IsAvailable(Summary summary) => true;
    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Custom;
    public int PriorityInCategory => 0;
    public bool IsNumeric => true;
    public UnitType UnitType => UnitType.Size;
    public string Legend => $"Custom '{ColumnName}' number column";
    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);
    public override string ToString() => ColumnName;
}
