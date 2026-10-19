using OxyPlot;

namespace Benchmarking.Helpers;

public static class PlotModelSvgWriter
{
    /// <summary>
    /// Writes the plot as an SVG, replacing any existing file and creating missing directories.
    /// </summary>
    public static void Write(PlotModel model, string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath))!);
        using var stream = File.Create(filePath);
        new SvgExporter { Width = 1200, Height = 600 }.Export(model, stream);
    }
}
