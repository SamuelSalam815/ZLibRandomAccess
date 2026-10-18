using System.IO.Compression;
using ZLibWrapper;

namespace ArchiveViewerBackend;

/// <summary>
/// A read-only stream of decompressed bytes that can seek backwards. Seeking forwards reads and discards
/// bytes. Seeking backwards asks a restart function for a stream that is positioned at or before the target,
/// then reads forward from there. <see cref="Position"/> is the offset in the decompressed data.
/// </summary>
/// <typeparam name="TContext">
/// The resource the streams are made from, such as a file handle. The stream owns it and disposes it.
/// </typeparam>
public sealed class BackwardSeekableStream<TContext> : Stream where TContext : IDisposable
{
    private readonly TContext _context;
    private readonly Func<TContext, long, (Stream Stream, long Offset)> _restartAtOrBefore;
    private Stream _inner;
    private long _position;
    private bool _isDisposed;

    /// <param name="context">Passed to <paramref name="restartAtOrBefore"/> on every call. Disposed with this stream.</param>
    /// <param name="restartAtOrBefore">
    /// Given the context and a target offset, returns a readable stream and the offset it is positioned at,
    /// which must not be after the target. It is called once with 0 to get the first stream. It may return the
    /// stream it returned before; any other stream replaces the old one, which is then disposed.
    /// </param>
    public BackwardSeekableStream(TContext context, Func<TContext, long, (Stream Stream, long Offset)> restartAtOrBefore)
    {
        _context = context;
        _restartAtOrBefore = restartAtOrBefore;
        (_inner, _position) = restartAtOrBefore(context, 0);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var bytesRead = _inner.Read(buffer, offset, count);
        _position += bytesRead;
        return bytesRead;
    }

    /// <summary>
    /// Moves to an offset from the start or the current position. Seeking past the end stops at the end.
    /// Returns the new position.
    /// </summary>
    public override long Seek(long offset, SeekOrigin origin)
    {
        var target = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _position + offset,
            _ => throw new NotSupportedException("The length of the decompressed data is not known"),
        };
        if (target < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), target, "Cannot seek before the start of the data");
        }

        if (target < _position)
        {
            Restart(target);
        }

        SkipForwardTo(target);
        return _position;
    }

    private void Restart(long target)
    {
        var (stream, offset) = _restartAtOrBefore(_context, target);
        if (!ReferenceEquals(stream, _inner))
        {
            _inner.Dispose();
        }

        _inner = stream;
        _position = offset;
    }

    private void SkipForwardTo(long target)
    {
        var scratch = new byte[(int)Math.Min(target - _position, 8192)];
        while (_position < target)
        {
            var bytesRead = Read(scratch, 0, (int)Math.Min(target - _position, scratch.Length));
            if (bytesRead == 0)
            {
                return;
            }
        }
    }

    public override void Flush()
    {
    }

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException("The length of the decompressed data is not known");

    public override long Position
    {
        get => _position;
        set => Seek(value, SeekOrigin.Begin);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            _inner.Dispose();
            _context.Dispose();
            _isDisposed = true;
        }

        base.Dispose(disposing);
    }
}

/// <summary>Creates <see cref="BackwardSeekableStream{TContext}"/>s over gzip data. Each takes ownership of the compressed stream.</summary>
public static class BackwardSeekableStream
{
    /// <summary>
    /// Seeks backwards in a plain gzip stream by decompressing again from the start.
    /// <paramref name="compressed"/> must be seekable and is disposed with the returned stream.
    /// </summary>
    public static Stream ForGZip(Stream compressed) =>
        new BackwardSeekableStream<Stream>(compressed, static (source, _) =>
        {
            source.Position = 0;
            return (new GZipStream(source, CompressionMode.Decompress, leaveOpen: true), 0);
        });

    /// <summary>
    /// Seeks backwards in a gzip stream with recovery points by jumping to the last recovery point at or
    /// before the target. <paramref name="compressed"/> must be seekable and is disposed with the returned stream.
    /// </summary>
    public static Stream ForGZipWithRecoveryPoints(Stream compressed, List<RecoveryPointOffset> recoveryPoints) =>
        new BackwardSeekableStream<RecoveryPointContext>(
            new RecoveryPointContext(compressed, recoveryPoints),
            RestartAtRecoveryPoint);

    private static (Stream Stream, long Offset) RestartAtRecoveryPoint(RecoveryPointContext context, long target)
    {
        var pointIndex = context.RecoveryPoints.FindLastIndex(point => point.OffsetInUncompressedStream <= target);
        if (context.Reader is not null && pointIndex >= 0)
        {
            return (context.Reader, context.Reader.JumpTo(pointIndex).OffsetInUncompressedStream);
        }

        // No recovery point before the target, so decompress from the start of the file.
        // The seekable stream disposes the reader that this one replaces.
        context.Compressed.Position = 0;
        context.Reader = new GZipReadingStreamWithRecoveryPoints(context.Compressed, context.RecoveryPoints, leaveOpen: true);
        return (context.Reader, 0);
    }

    private sealed class RecoveryPointContext(Stream compressed, List<RecoveryPointOffset> recoveryPoints) : IDisposable
    {
        public Stream Compressed { get; } = compressed;

        public List<RecoveryPointOffset> RecoveryPoints { get; } = recoveryPoints;

        /// <summary>The reader made by the latest restart from the start of the file, or null before the first.</summary>
        public GZipReadingStreamWithRecoveryPoints? Reader { get; set; }

        public void Dispose()
        {
            Reader?.Dispose();
            Compressed.Dispose();
        }
    }
}
