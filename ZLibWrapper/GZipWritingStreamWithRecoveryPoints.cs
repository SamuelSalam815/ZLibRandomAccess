using System.Buffers;
using System.Text;
using ZLibBindings.Constants;

namespace ZLibWrapper;

public class GZipWritingStreamWithRecoveryPoints(
    Stream stream,
    bool leaveOpen = false,
    long? recoveryPointByteInterval = null,
    ZCompressionLevel compressionLevel = ZCompressionLevel.Z_DEFAULT_COMPRESSION,
    Encoding? encoding = null) : Stream
{
    public event Action<RecoveryPointOffset>? RecoveryPointWritten;
    public event Action? StreamClosed;

    private bool _isDisposed;

    public long TotalBytesWritten { get; private set; }
    private long _bytesWrittenSinceLastRecoveryPoint = 0;

    public long TotalLinesWritten { get; private set; }
    private bool _isAtLineStart = true;
    private readonly Encoding _encoding = encoding ?? Encoding.Default;
    // The decoder keeps characters split across writes, so counting works for any encoding.
    private readonly Decoder _decoder = (encoding ?? Encoding.Default).GetDecoder();

    private readonly ZLibDeflateStreamWithRecoveryPoints _deflateStream = new(
        stream,
        leaveOpen,
        new ZLibDeflateConfiguration
        {
            CompressionLevel = compressionLevel,
            WindowBits = ZWindowBits.DefaultWindowSize | ZWindowBits.GZipStream
        }
    );

    public override void Flush() => _deflateStream.NonRecoveryPointFlush();

    private void WriteRecoveryPoint()
    {
        _deflateStream.Flush();
        _bytesWrittenSinceLastRecoveryPoint = 0;
        RecoveryPointWritten?.Invoke(new RecoveryPointOffset(
            TotalBytesWritten, _deflateStream.Position, TotalLinesWritten, _isAtLineStart));
    }

    private void CountLines(byte[] buffer, int offset, int count)
    {
        var chars = ArrayPool<char>.Shared.Rent(_encoding.GetMaxCharCount(count));
        try
        {
            var charCount = _decoder.GetChars(buffer, offset, count, chars, 0);
            if (charCount == 0)
            {
                return;
            }

            TotalLinesWritten += new ReadOnlySpan<char>(chars, 0, charCount).Count('\n');
            _isAtLineStart = chars[charCount - 1] == '\n';
        }
        finally
        {
            ArrayPool<char>.Shared.Return(chars);
        }
    }

    public override int Read(byte[] buffer, int offset, int count) => _deflateStream.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => _deflateStream.Seek(offset, origin);

    public override void SetLength(long value) => _deflateStream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count)
    {
        _deflateStream.Write(buffer, offset, count);
        CountLines(buffer, offset, count);
        _bytesWrittenSinceLastRecoveryPoint += count;
        TotalBytesWritten += count;
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
