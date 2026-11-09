using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OxyPlot.Axes;
using OxyPlot.Series;
using Shouldly;

namespace Benchmarking.Helpers.Tests;

[TestClass]
[TestSubject(typeof(BarChartBuilder<,>))]
public class BarChartBuilderTest
{
    private static readonly List<BarChartBar<string, string>> Bars =
    [
        new("small", "a", 1),
        new("large", "a", 2),
        new("small", "b", 3),
        new("large", "b", 4),
    ];

    private static BarChartConfiguration<string, string> Config(string? valueLabelFormat = null) => new()
    {
        Title = "title",
        ValueLabelFormat = valueLabelFormat,
        CategoryComparer = Comparer<string>.Create((x, y) => string.CompareOrdinal(y, x)),
    };

    [TestMethod]
    public void Build_PutsFirstCategoryOnTop()
    {
        var model = new BarChartBuilder<string, string>(Config()).Build(Bars);

        var categoryAxis = model.Axes.OfType<CategoryAxis>().Single();
        categoryAxis.Labels.ShouldBe(["small", "large"]);
        categoryAxis.StartPosition.ShouldBe(1);
        categoryAxis.EndPosition.ShouldBe(0);
    }

    [TestMethod]
    public void Build_WithoutValueLabelFormat_ShowsNoLabels()
    {
        var model = new BarChartBuilder<string, string>(Config()).Build(Bars);

        model.Series.Cast<BarSeries>().ShouldAllBe(series => series.LabelFormatString == null);
    }

    [TestMethod]
    public void Build_WithValueLabelFormat_LabelsEveryBarSeries()
    {
        var model = new BarChartBuilder<string, string>(Config("{0:0.0}")).Build(Bars);

        model.Series.Cast<BarSeries>().ShouldAllBe(series => series.LabelFormatString == "{0:0.0}");
    }

    [TestMethod]
    public void Write_UsesAnOpaqueBackgroundSoTextIsReadableOnDarkPages()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".svg");
        try
        {
            var model = new BarChartBuilder<string, string>(Config()).Build(Bars);

            PlotModelSvgWriter.Write(model, path);

            model.Background.ShouldBe(OxyPlot.OxyColors.White);
            File.ReadAllText(path).ShouldContain("fill:rgb(255,255,255);stroke:none;");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void Build_WithMissingBar_Throws()
    {
        Should.Throw<ArgumentException>(() => new BarChartBuilder<string, string>(Config()).Build(Bars[..3]));
    }
}
