using System.Text;

namespace ArchiveViewerBackend;

public class StreamReaderWithByteCount : TextReader
{

    private readonly CircularArray<byte> _byteBuffer = new(1024);
    private readonly CircularArray<char> _characterBuffer = new(1024);
    private readonly Stream _stream;
    private readonly Decoder _decoder;

    public StreamReaderWithByteCount(Stream stream, Encoding? encoding = null, bool leaveOpen = false)
    {
        _stream = stream;
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
        throw new NotImplementedException();
    }
}
