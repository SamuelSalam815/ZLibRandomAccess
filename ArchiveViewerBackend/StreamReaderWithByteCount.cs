using System.Text;

namespace ArchiveViewerBackend;

public class StreamReaderWithByteCount : TextReader
{

    private readonly CircularArray<byte> _byteBuffer = new(1024);
    private readonly CircularArray<char> _characterBuffer = new(1024);
    private readonly Stream _stream;
    private readonly Decoder _decoder;
    public int NumberOfBytesRead { get; private set; }
    public int NumberOfCharactersRead { get; private set; }
    private bool _isDisposed;
    private readonly bool _leaveInnerStreamOpen;

    public StreamReaderWithByteCount(Stream stream, Encoding? encoding = null, bool leaveOpen = false)
    {
        _stream = stream;
        _leaveInnerStreamOpen = leaveOpen;
        encoding ??= Encoding.Default;
        _decoder = encoding.GetDecoder();
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
            NumberOfBytesRead += numBytesRead;
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
            NumberOfCharactersRead += charsUsed;
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
