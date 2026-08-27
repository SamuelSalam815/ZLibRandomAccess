using System.IO.Compression;
using LogSimulator.Simulator;
using ZLibBindings.Constants;

namespace Benchmarking.CompressionRatio;

class Program
{
    private const int SampleCount = 5;

    static void Main(string[] args)
    {
        long[] inputDataSizes =
        [
            DataSize.KiloByte * 64,
            // DataSize.KiloByte * 128,
            // DataSize.KiloByte * 256,
            // DataSize.KiloByte * 512,
            // DataSize.MegaByte,
            // DataSize.MegaByte * 2,
            // DataSize.MegaByte * 4,
            // DataSize.MegaByte * 8,
        ];

        CompressionRequest[] compressionRequests =
        [
            new SystemGzipCompressionRequest(CompressionLevel.Fastest),
            new SystemGzipCompressionRequest(CompressionLevel.Optimal),
            // new SystemGzipCompressionRequest(CompressionLevel.SmallestSize),
            //
            // new ZlibGzipCompressionRequest(ZCompressionLevel.Z_DEFAULT_COMPRESSION),
            // new ZlibGzipCompressionRequest(ZCompressionLevel.Z_BEST_COMPRESSION),
            // new ZlibGzipCompressionRequest(ZCompressionLevel.Z_BEST_SPEED),
            //
            // new ZlibGzipRecoveryPointCompressionRequest(DataSize.KiloByte, ZCompressionLevel.Z_DEFAULT_COMPRESSION),
            // new ZlibGzipRecoveryPointCompressionRequest(DataSize.KiloByte, ZCompressionLevel.Z_BEST_COMPRESSION),
            // new ZlibGzipRecoveryPointCompressionRequest(DataSize.KiloByte, ZCompressionLevel.Z_BEST_SPEED),
            //
            // new ZlibGzipRecoveryPointCompressionRequest(DataSize.KiloByte * 8, ZCompressionLevel.Z_DEFAULT_COMPRESSION),
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

        foreach (var group in results.GroupBy(result => (result.CompressionRequest, result.InputDataSize)))
        {
            Console.WriteLine(group.Key.CompressionRequest);
            Console.WriteLine("Input data size: {0:N0}", group.Key.InputDataSize);
            var compressionRatios = group.Select(result => result.CompressionRatio).ToList();
            var compressionRatioList = string.Join(", ", compressionRatios);
            Console.WriteLine("Compression ratio: avg {0} [{1}]", compressionRatios.Average(),compressionRatioList);
            Console.WriteLine();
        }
    }
}
