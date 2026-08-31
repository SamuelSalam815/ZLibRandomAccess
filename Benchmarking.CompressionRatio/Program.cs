using System.IO.Compression;
using LogSimulator.Simulator;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.Wpf;
using ZLibBindings.Constants;

namespace Benchmarking.CompressionRatio;

class Program
{
    private const int SampleCount = 3;

    static void Main(string[] args)
    {
        var thread = new Thread(() =>
        {
            var plot = PlotCompressionRatios();
            using var pngStream = new MemoryStream();
            var pngExporter = new PngExporter {Height = 1200, Width = 1600, Resolution = 180d};
            pngExporter.Export(plot, pngStream);
            File.WriteAllBytes("output.png", pngStream.ToArray());
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }

    private static PlotModel PlotCompressionRatios()
    {
        var dataSizeAxis = new CategoryAxis
        {
            Title = "Uncompressed Data Size",
            Position = AxisPosition.Left,
            ItemsSource = new[]
            {
                "512 MB",
                "1 GB",
                "2 GB",
                "4 GB",
                "8 GB",
            }
        };

        long[] inputDataSizes =
        [
            DataSize.MegaByte * 512,
            DataSize.MegaByte * 1024,
            DataSize.MegaByte * 2048,
            DataSize.MegaByte * 4096,
            DataSize.MegaByte * 8192,
        ];

        CompressionRequest[] compressionRequests =
        [
            new SystemGzipCompressionRequest(CompressionLevel.Optimal),
            // new SystemGzipCompressionRequest(CompressionLevel.SmallestSize),
            // new SystemGzipCompressionRequest(CompressionLevel.Fastest),

            new ZlibGzipCompressionRequest(ZCompressionLevel.Z_DEFAULT_COMPRESSION),
            // new ZlibGzipCompressionRequest(ZCompressionLevel.Z_BEST_COMPRESSION),
            // new ZlibGzipCompressionRequest(ZCompressionLevel.Z_BEST_SPEED),

            new ZlibGzipRecoveryPointCompressionRequest(DataSize.KiloByte, ZCompressionLevel.Z_DEFAULT_COMPRESSION),
            // new ZlibGzipRecoveryPointCompressionRequest(DataSize.KiloByte, ZCompressionLevel.Z_BEST_COMPRESSION),
            // new ZlibGzipRecoveryPointCompressionRequest(DataSize.KiloByte, ZCompressionLevel.Z_BEST_SPEED),

            new ZlibGzipRecoveryPointCompressionRequest(DataSize.MegaByte, ZCompressionLevel.Z_DEFAULT_COMPRESSION),
            // new ZlibGzipRecoveryPointCompressionRequest(DataSize.KiloByte * 8, ZCompressionLevel.Z_BEST_COMPRESSION),
            // new ZlibGzipRecoveryPointCompressionRequest(DataSize.KiloByte * 8, ZCompressionLevel.Z_BEST_SPEED),
        ];

        var results = new List<CompressionResult>();

        foreach (var dataSize in inputDataSizes)
        {
            foreach (var compressionRequest in compressionRequests)
            {
                for (var i = 0; i < SampleCount; i++)
                {
                    Console.WriteLine(
                        "Running request '{0}' with input size {1:N0} B, sample [{2}/{3}]...",
                        compressionRequest,
                        dataSize,
                        i + 1,
                        SampleCount);
                    results.Add(compressionRequest.CalculateCompressedPayloadSize(dataSize));
                    Console.WriteLine("Done.");
                }
            }
        }

        var plot = new PlotModel
        {
            Title =  "Compression Ratio vs Uncompressed Data Size",
            IsLegendVisible = true,
            Background = OxyColors.White
        };
        plot.Legends.Add(
            new Legend
            {
                LegendPlacement = LegendPlacement.Outside,
                LegendOrientation = LegendOrientation.Vertical,
                LegendPosition = LegendPosition.BottomLeft
            });
        plot.Axes.Add(
            new LinearAxis
            {
                Title = "Compression Ratio",
                Position = AxisPosition.Bottom,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = 1,
                Minimum = results.Min(x => x.CompressionRatio),
            });
        plot.Axes.Add(dataSizeAxis);
        foreach (var compressionMethodGroup in results.GroupBy(result => result.CompressionRequest))
        {
            var series = new BarSeries
            {
                Title = compressionMethodGroup.Key.ToString(),
                StrokeColor = OxyColors.Black,
            };
            var data = compressionMethodGroup
                .GroupBy(result => result.InputDataSize)
                .Select(group => (InputDataSize: group.Key,
                    AvgCompressionRatio: group.Select(x => x.CompressionRatio).Average()))
                .OrderBy(dataPoint => dataPoint.InputDataSize)
                .Select(dataPoint => new BarItem() { Value = dataPoint.AvgCompressionRatio });
            series.Items.AddRange(data);

            plot.Series.Add(series);
        }

        return plot;
    }
}
