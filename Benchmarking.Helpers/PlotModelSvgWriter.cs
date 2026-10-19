using OxyPlot;

namespace Benchmarking.Helpers;

public static class PlotModelSvgWriter
{
    /// <summary>
    /// Writes the plot as an SVG on a white background, replacing any existing file and creating missing directories.
    /// </summary>
    public static void Write(PlotModel model, string filePath)
    {
        // The default background is transparent, which leaves black text unreadable on a dark page (GitHub dark mode).
        model.Background = OxyColors.White;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath))!);
        using var stream = File.Create(filePath);
        new SvgExporter { Width = 1200, Height = 600 }.Export(model, stream);
    }
}
