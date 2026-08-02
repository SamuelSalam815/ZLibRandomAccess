using System.Collections.Immutable;
using ZLibBindings.Constants;

namespace ZLibWrapper;

public class GZipReadingStreamWithRecoveryPoints(
    Stream stream,
    List<RecoveryPointOffset> recoveryPointOffsets,
    bool leaveOpen = false)
    : Stream
{
    public GZipReadingStreamWithRecoveryPoints(Stream stream, bool leaveOpen = false) : this(stream, [], leaveOpen)
    {
    }

    private bool _isDisposed;

    private readonly ZLibInflateStreamWithRecoveryPoints _stream = new(
        stream,
        leaveOpen,
        ZWindowBits.DefaultWindowSize | ZWindowBits.GZipStream);

    public ImmutableList<RecoveryPointOffset> RecoveryPointOffsets { get; } = recoveryPointOffsets.ToImmutableList();

    public override void Flush()
    {
        _stream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _stream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        // TODO: find closest offest then read and discard the remaining bytes
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        _stream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _stream.Write(buffer, offset, count);
    }

    public override bool CanRead => _stream.CanRead;
    public override bool CanSeek => _stream.CanSeek;
    public override bool CanWrite => _stream.CanWrite;
    public override long Length => _stream.Length;
    public override long Position
    {
        get => _stream.Position;
        set => _stream.Position = value;
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing || _isDisposed)
        {
            return;
        }

        _stream.Dispose();
        _isDisposed = true;
    }
}
