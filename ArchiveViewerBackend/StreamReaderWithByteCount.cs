using System.Text;

namespace ArchiveViewerBackend;

public class StreamReaderWithByteCount : TextReader
{

    private readonly CircularArray<byte> _byteBuffer = new(1024);
    private readonly CircularArray<byte> _characterBuffer = new(1024);
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
            _byteBuffer.Add((byte)_stream.ReadByte());
        }
        var byteArray = _byteBuffer.ToArray();
        // _decoder.Convert()
    }

    public override int Read()
    {
        throw new NotImplementedException();
    }
}
