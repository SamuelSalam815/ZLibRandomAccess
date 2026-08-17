using System.Text;

namespace ArchiveViewerBackend;

public class StreamReaderWithByteCountLimit : StreamReaderWithByteCount
{
    private readonly int? _maximumNumberOfBytesRead;

    public StreamReaderWithByteCountLimit(
        Stream stream,
        int bufferSize = 1024,
        bool leaveOpen = false,
        int? maximumNumberOfBytesRead = null) : base(
        stream,
        Encoding.Default,
        bufferSize,
        leaveOpen)
    {
        _maximumNumberOfBytesRead = maximumNumberOfBytesRead;
    }

    public StreamReaderWithByteCountLimit(
        Stream stream,
        Encoding encoding,
        int bufferSize = 1024,
        bool leaveOpen = false,
        int? maximumNumberOfBytesRead = null) : base(stream, encoding, bufferSize, leaveOpen)
    {
        _maximumNumberOfBytesRead = maximumNumberOfBytesRead;
    }

    public override int Read()
    {
        if (NumberOfBytesRead >= _maximumNumberOfBytesRead)
        {
            return -1;
        }
        return base.Read();
    }
}
