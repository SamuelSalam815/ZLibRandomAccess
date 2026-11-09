using System.Text;

namespace ArchiveViewerBackend;

public class StreamReaderWithByteCount : TextReader
{

    private readonly CompactingBuffer<byte> _byteBuffer;
    private readonly CompactingBuffer<char> _characterBuffer;
    private readonly Stream _stream;
    private readonly Encoding _encoding;
    private readonly Decoder _decoder;
    // The decoder throws unless its output has room for what one more byte can decode to:
    // two characters where a surrogate pair is possible, one for single-byte encodings.
    private readonly int _roomForOneCharacter;
    public int NumberOfBytesRead { get; private set; }
    private char _previousCharacter;
    private bool _isDisposed;
    private readonly bool _leaveInnerStreamOpen;

    public StreamReaderWithByteCount(Stream stream, int bufferSize = 1024, bool leaveOpen = false) : this(stream, Encoding.Default, bufferSize, leaveOpen)
    {
    }

    public StreamReaderWithByteCount(Stream stream, Encoding encoding, int bufferSize = 1024, bool leaveOpen = false)
    {
        _roomForOneCharacter = encoding.GetMaxCharCount(1);
        ArgumentOutOfRangeException.ThrowIfLessThan(bufferSize, _roomForOneCharacter);
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

        while (_byteBuffer.Length > 0 && _characterBuffer.Capacity - _characterBuffer.Length >= _roomForOneCharacter)
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
        NumberOfBytesRead += BytesToCount(result);
        _previousCharacter = result;
        _characterBuffer.DropFirst(1);
        return result;
    }

    /// <summary>
    /// A surrogate pair is encoded as one character, so neither half has a size alone. The pair is counted
    /// when its second half is read.
    /// </summary>
    private int BytesToCount(char character)
    {
        if (char.IsHighSurrogate(character))
        {
            return 0;
        }

        return char.IsLowSurrogate(character) && char.IsHighSurrogate(_previousCharacter)
            ? _encoding.GetByteCount([_previousCharacter, character])
            : _encoding.GetByteCount([character]);
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
