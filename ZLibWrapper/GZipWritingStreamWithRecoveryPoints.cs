using System.Threading.Channels;
using ZLibBindings.Constants;

namespace ZLibWrapper;

public class GZipWritingStreamWithRecoveryPoints(
    Stream stream,
    bool leaveOpen = false,
    long? recoveryPointByteInterval = null) : Stream
{
    public event Action<RecoveryPointOffset>? RecoveryPointWritten;
    public event Action? StreamClosed;

    private bool _isDisposed;

    private long _totalBytesWritten = 0;
    private long _bytesWrittenSinceLastRecoveryPoint = 0;

    private readonly ZLibDeflateStreamWithRecoveryPoints _deflateStream = new(
        stream,
        leaveOpen,
        new ZLibDeflateConfiguration { WindowBits = ZWindowBits.DefaultWindowSize | ZWindowBits.GZipStream }
    );

    public override void Flush() => _deflateStream.NonRecoveryPointFlush();

    private void WriteRecoveryPoint()
    {
        _deflateStream.Flush();
        _bytesWrittenSinceLastRecoveryPoint = 0;
        RecoveryPointWritten?.Invoke(new RecoveryPointOffset(_totalBytesWritten, _deflateStream.Position));
    }

    public override int Read(byte[] buffer, int offset, int count) => _deflateStream.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => _deflateStream.Seek(offset, origin);

    public override void SetLength(long value) => _deflateStream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count)
    {
        _deflateStream.Write(buffer, offset, count);
        _bytesWrittenSinceLastRecoveryPoint += count;
        _totalBytesWritten += count;
        if (_bytesWrittenSinceLastRecoveryPoint >= recoveryPointByteInterval)
        {
            WriteRecoveryPoint();
        }
    }

    public override bool CanRead => _deflateStream.CanRead;
    public override bool CanSeek => _deflateStream.CanSeek;
    public override bool CanWrite => _deflateStream.CanWrite;
    public override long Length => _deflateStream.Length;
    public override long Position
    {
        get => _deflateStream.Position;
        set => _deflateStream.Position = value;
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing || _isDisposed)
        {
            return;
        }

        _deflateStream.Dispose();
        _isDisposed = true;
        StreamClosed?.Invoke();
    }
}
