using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace Benchmarking.Helpers;

public class BarChartBuilder<TCategory, TSeries>(BarChartConfiguration<TCategory, TSeries> config)
{
    public PlotModel Build(List<BarChartBar<TCategory, TSeries>> bars)
    {
        var categories = bars.Select(x => x.Category).Distinct().OrderBy(x => x, config.CategoryComparer).ToList();
        var numSeries = bars.Select(x => x.Series).Distinct().Count();
        var expectedBarCount = categories.Count * numSeries;

        if (bars.Count != expectedBarCount)
        {
            throw new ArgumentException(
                $"Expected exactly one value for each Category, Series combination ({categories.Count} Categories * {numSeries} Series => {expectedBarCount} values). But got {bars.Count} values instead!",
                nameof(bars));
        }

        var model = new PlotModel
        {
            Title = config.Title,
        };

        model.Legends.Add(
            new Legend
            {
                LegendTitle = config.SeriesLabel,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendBorderThickness = 0
            });

        foreach (var seriesGroup in bars
                     .GroupBy(x => x.Series)
                     .OrderBy(x => x.Key, config.SeriesComparer)
                     .Select(seriesData => new
                     {
                         Series = seriesData.Key,
                         Data = seriesData.OrderBy(x => x.Category, config.CategoryComparer)
                     })
                )
        {
            var currentSeries = new BarSeries
            {
                Title = seriesGroup.Series?.ToString(),
                StrokeColor = OxyColors.Black,
                StrokeThickness = 1
            };
            foreach (var bar in seriesGroup.Data)
            {
                currentSeries.Items.Add(new BarItem { Value = bar.Value });
            }

            model.Series.Add(currentSeries);
        }

        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Left,
            Title = config.CategoryAxisLabel,
        };

        foreach (var category in categories)
        {
            categoryAxis.Labels.Add(category?.ToString());
        }

        var valueAxis = new LinearAxis
        {
            Title = config.ValueAxisLabel,
            Position = AxisPosition.Bottom,
            MinimumPadding = 0,
            MaximumPadding = 0.06,
            MajorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = OxyColors.Gray,
        };

        if (config.MinimumValue.HasValue)
        {
            valueAxis.Minimum = config.MinimumValue.Value;
        }

        if (config.MaximumValue.HasValue)
        {
            valueAxis.Maximum = config.MaximumValue.Value;
        }

        model.Axes.Add(categoryAxis);
        model.Axes.Add(valueAxis);

        return model;
    }
}
