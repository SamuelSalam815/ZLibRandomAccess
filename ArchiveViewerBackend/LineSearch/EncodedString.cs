using System.Text;

namespace ArchiveViewerBackend.LineSearch;

public record EncodedString(byte[] Bytes, Encoding Encoding, string String)
{
    /// <summary>
    /// Given the index of a character in <see cref="String"/>, returns the corresponding byte offset.
    /// </summary>
    public int ByteOffsetOf(int characterIndex)
    {
        return Encoding.GetByteCount(String[..characterIndex]);
    }

    public override string ToString() => String;
};
