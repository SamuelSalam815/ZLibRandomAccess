using System.IO.Compression;
using System.Text;
using Benchmarking.CompressionRatio.Values;
using ZLibBindings.Constants;
using ZLibWrapper;

namespace Benchmarking.CompressionRatio.BenchmarkRunners;

public class CompressionBenchmarkRunner : ICompressionBenchmarkRunner
{
    public CompressionResult Run(CompressionRequest request, CancellationToken cancellationToken)
    {
        Console.WriteLine("Running request {0}", request);
        var sim = new LogSimulator.Simulator.LogSimulator(
            "Neo",
            DateTime.Now,
            Random.Shared);
        using var byteCountStream = new StreamWithWriteCount(Stream.Null, cancellationToken);
        using Stream compressionStream = request.CompressionMethod switch
        {
            CompressionMethod.GZipUsingZLib or CompressionMethod.GZipUsingZLibWithFullFlushing => new
                GZipWritingStreamWithRecoveryPoints(
                    byteCountStream,
                    recoveryPointByteInterval: request.CompressionMethod switch
                    {
                        CompressionMethod.GZipUsingZLib => null,
                        CompressionMethod.GZipUsingZLibWithFullFlushing => 1024L * 1024,
                        _ => throw new ArgumentOutOfRangeException(nameof(request.CompressionMethod))
                    },
                    compressionLevel: request.CompressionLevel switch
                    {
                        NormalizedCompressionLevel.Default => ZCompressionLevel.Z_DEFAULT_COMPRESSION,
                        NormalizedCompressionLevel.Fastest => ZCompressionLevel.Z_BEST_SPEED,
                        NormalizedCompressionLevel.Smallest => ZCompressionLevel.Z_BEST_COMPRESSION,
                        _ => throw new ArgumentOutOfRangeException(nameof(request.CompressionLevel))
                    }),
            CompressionMethod.GZipUsingBaseClassLibrary => new GZipStream(
                byteCountStream,
                request.CompressionLevel switch
                {
                    NormalizedCompressionLevel.Default => CompressionLevel.Optimal,
                    NormalizedCompressionLevel.Fastest => CompressionLevel.Fastest,
                    NormalizedCompressionLevel.Smallest => CompressionLevel.SmallestSize,
                }),
            _ => throw new ArgumentOutOfRangeException(nameof(request.CompressionMethod))
        };

        sim.SimulateLogs(compressionStream, Encoding.Default, request.NumberOfBytesToCompress);
        return new CompressionResult(request, byteCountStream.WriteCount);
    }

    private class StreamWithWriteCount(Stream innerStream, CancellationToken cancellationToken) : Stream
    {
        public long WriteCount { get; private set; }

        public override void Flush()
        {
            cancellationToken.ThrowIfCancellationRequested();
            innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            cancellationToken.ThrowIfCancellationRequested();
            innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteCount += count;
            innerStream.Write(buffer, offset, count);
        }

        public override bool CanRead => innerStream.CanRead;
        public override bool CanSeek => innerStream.CanSeek;
        public override bool CanWrite => innerStream.CanWrite;
        public override long Length => innerStream.Length;
        public override long Position
        {
            get => innerStream.Position;
            set => innerStream.Position = value;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                innerStream.Dispose();
            }
        }
    }
}
