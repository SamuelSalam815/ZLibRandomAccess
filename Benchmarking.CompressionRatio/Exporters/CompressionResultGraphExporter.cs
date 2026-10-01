using Benchmarking.CompressionRatio.Values;
using Benchmarking.Helpers;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using CompressionMethod = Benchmarking.CompressionRatio.Values.CompressionMethod;

namespace Benchmarking.CompressionRatio.Exporters;

public sealed class CompressionResultGraphExporter(Stream outputStream) : ICompressionResultExporter
{
    private readonly List<CompressionResult> _results = [];

    public void Accept(CompressionResult result)
    {
        _results.Add(result);
    }

    public void Complete()
    {
        var groupedData = _results
            .Select(result => new
            {
                Method = result.Request.CompressionMethod,
                Level = result.Request.CompressionLevel,
                CompressionRatio = 1d - (double)result.NumberOfBytesAfterCompression /
                    result.Request.NumberOfBytesToCompress,
            })
            .GroupBy(x => x.Level)
            .OrderBy(x => x.Key)
            .ToList();
        // HACK: Migration to Benchmarking.Helpers.BarChartBuilder is unfinished. Bars still need mapping from
        // CompressionResult (average the ratio per method/level so each combination has one bar). Until then the
        // hand-rolled PlotModel below is still used.
        // new BarChartBuilder<CompressionMethod, NormalizedCompressionLevel>(
        //     new BarChartConfiguration<CompressionMethod, NormalizedCompressionLevel>
        //     {
        //         Title = "Compression Methods against Compression Ratios",
        //         CategoryAxisLabel = "Compression Methods",
        //         ValueAxisLabel = "Compression Ratio",
        //         SeriesLabel = "Compression Mode",
        //     }).Build(_results
        //     .Select(x => new BarChartBar<CompressionMethod, NormalizedCompressionLevel>()));
        var model = new PlotModel
        {
            Title = "Compression Methods against Compression Ratios",
        };

        model.Legends.Add(
            new Legend()
            {
                LegendTitle = "Compression Mode",
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendBorderThickness = 0
            });

        foreach (var group in groupedData)
        {
            var entries = group
                .GroupBy(x => x.Method)
                .Select(g => new {Method = g.Key, CompressionRatio = g.Average(x => x.CompressionRatio)})
                .OrderBy(x => x.Method);
            var series = new BarSeries { Title = group.Key.ToString(), StrokeColor = OxyColors.Black, StrokeThickness = 1 };
            foreach (var entry in entries)
            {
                series.Items.Add(new BarItem(entry.CompressionRatio));
            }
            model.Series.Add(series);
        }

        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Left,
            Title = "Compression Method",
        };
        var labels = groupedData.SelectMany(x => x).Select(x => x.Method).Distinct();
        foreach (var label in labels)
        {
            categoryAxis.Labels.Add(label.ToString());
        }

        var minDataValue = groupedData.Min(g => g.Min(x => x.CompressionRatio));
        var axisMinimum = Math.Max(0, minDataValue - 0.02);
        var valueAxis = new LinearAxis
        {
            Title = "Compression Ratio",
            Position = AxisPosition.Bottom,
            MinimumPadding = 0,
            MaximumPadding = 0.06,
            AbsoluteMinimum = axisMinimum,
            MajorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = OxyColors.Gray
        };
        model.Axes.Add(categoryAxis);
        model.Axes.Add(valueAxis);

        var exporter = new SvgExporter { Width = 1600, Height = 600 };
        exporter.Export(model, outputStream);
    }

    public void Dispose()
    {
        outputStream.Dispose();
    }
}
