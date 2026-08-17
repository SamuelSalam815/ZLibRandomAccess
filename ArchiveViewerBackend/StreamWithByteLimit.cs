namespace ArchiveViewerBackend;

public class StreamWithByteLimit : Stream
{
    private readonly Stream _stream;

    private readonly long _byteLimit;

    private long _byteCount;

    public StreamWithByteLimit(Stream stream, long byteLimit)
    {
        if (byteLimit < 0)
        {
            throw new ArgumentException("ByteLimit cannot be negative", nameof(byteLimit));
        }
        _stream = stream;
        _byteLimit = byteLimit;
        Length = Math.Min(stream.Length - stream.Position, byteLimit);
    }

    public override void Flush() => _stream.Flush();

    public override int Read(byte[] buffer, int offset, int count)
    {
        int readCount;
        if (_byteCount + count < _byteLimit)
        {
            readCount = count;
        }
        else
        {
            readCount = (int)(_byteLimit - _byteCount);
        }

        _byteCount += readCount;
        return _stream.Read(buffer, offset, readCount);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override bool CanRead => _stream.CanRead;
    public override bool CanSeek => _stream.CanSeek;
    public override bool CanWrite => false;
    public override long Length { get; }
    public override long Position
    {
        get => _stream.Position;
        set => throw new NotSupportedException();
    }
}
