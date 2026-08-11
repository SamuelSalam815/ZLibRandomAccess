using System.Text;

namespace ArchiveViewerBackend;

public class StreamReaderWithByteCount : TextReader
{

    private readonly CircularArray<byte> _byteBuffer;
    private readonly CircularArray<char> _characterBuffer;
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
        _byteBuffer = new CircularArray<byte>(bufferSize);
        _characterBuffer = new CircularArray<char>(bufferSize);
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
        return _characterBuffer[0];
    }

    private void DecodeMoreCharactersIfAllCharactersHaveBeenRead()
    {
        if (_characterBuffer.Length > 0)
        {
            return;
        }

        while (_byteBuffer.Length < _byteBuffer.Capacity)
        {
            var byteSpan = _byteBuffer.GetNextUnusedSpan();
            var numBytesRead = _stream.Read(byteSpan);
            _byteBuffer.SimulateAdd(numBytesRead);
            if (numBytesRead < byteSpan.Length)
            {
                break;
            }
        }

        while (_byteBuffer.Length > 0 && _characterBuffer.Length < _characterBuffer.Capacity)
        {
            _decoder.Convert(_byteBuffer.GetNextUsedSpan(), _characterBuffer.GetNextUnusedSpan(), false, out var bytesUsed, out var charsUsed, out _);
            _byteBuffer.Drop(bytesUsed);
            _characterBuffer.SimulateAdd(charsUsed);
        }
    }

    public override int Read()
    {
        DecodeMoreCharactersIfAllCharactersHaveBeenRead();
        if (_characterBuffer.Length == 0)
        {
            return -1;
        }
        var result = _characterBuffer[0];
        NumberOfBytesRead += _encoding.GetByteCount([result]);
        _characterBuffer.Drop();
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
