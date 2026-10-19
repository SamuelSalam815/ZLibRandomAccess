using System.Text;

namespace ArchiveViewerBackend;

public class StreamReaderWithByteCount : TextReader
{

    private readonly CompactingBuffer<byte> _byteBuffer;
    private readonly CompactingBuffer<char> _characterBuffer;
    private readonly Stream _stream;
    private readonly Encoding _encoding;
    private readonly Decoder _decoder;
    public int NumberOfBytesRead { get; private set; }
    private bool _isDisposed;
    private readonly bool _leaveInnerStreamOpen;

    public StreamReaderWithByteCount(Stream stream, int bufferSize = 1024, bool leaveOpen = false) : this(stream, Encoding.Default, bufferSize, leaveOpen)
    {
    }

    public StreamReaderWithByteCount(Stream stream, Encoding encoding, int bufferSize = 1024, bool leaveOpen = false)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(bufferSize, Utf16.MaxCodeUnitsPerCharacter);
        _byteBuffer = new CompactingBuffer<byte>(bufferSize);
        _characterBuffer = new CompactingBuffer<char>(bufferSize);
        _stream = stream;
        _leaveInnerStreamOpen = leaveOpen;
        _encoding = encoding;
        _decoder = encoding.GetDecoder();
    }

    public void ResetByteCount()
    {
        NumberOfBytesRead = 0;
    }

    public override int Peek()
    {
        DecodeMoreCharactersIfAllCharactersHaveBeenRead();
        return _characterBuffer.Length == 0 ? -1 : _characterBuffer.Contents[0];
    }

    private void DecodeMoreCharactersIfAllCharactersHaveBeenRead()
    {
        if (_characterBuffer.Length > 0)
        {
            return;
        }

        while (_byteBuffer.Length < _byteBuffer.Capacity)
        {
            var byteSpan = _byteBuffer.GetWritableSpan();
            var numBytesRead = _stream.Read(byteSpan);
            _byteBuffer.ReportWritten(numBytesRead);
            if (numBytesRead < byteSpan.Length)
            {
                break;
            }
        }

        // The decoder throws unless the output has room for the next whole character.
        while (_byteBuffer.Length > 0
               && _characterBuffer.Capacity - _characterBuffer.Length >= Utf16.MaxCodeUnitsPerCharacter)
        {
            _decoder.Convert(_byteBuffer.Contents, _characterBuffer.GetWritableSpan(), false, out var bytesUsed, out var charsUsed, out _);
            _byteBuffer.DropFirst(bytesUsed);
            _characterBuffer.ReportWritten(charsUsed);
        }
    }

    public override int Read()
    {
        DecodeMoreCharactersIfAllCharactersHaveBeenRead();
        if (_characterBuffer.Length == 0)
        {
            return -1;
        }
        var result = _characterBuffer.Contents[0];
        NumberOfBytesRead += _encoding.GetByteCount([result]);
        _characterBuffer.DropFirst(1);
        return result;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            if (!_leaveInnerStreamOpen)
            {
                _stream.Dispose();
            }
            _isDisposed = true;
        }
        base.Dispose(disposing);
    }
}
