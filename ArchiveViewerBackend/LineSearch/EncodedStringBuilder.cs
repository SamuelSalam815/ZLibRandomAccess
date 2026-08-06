using System.Text;

namespace ArchiveViewerBackend.LineSearch;

public class EncodedStringBuilder
{
    private readonly string _string;
    private readonly Encoding _encoding;

    public EncodedStringBuilder(string @string) : this(@string, Encoding.Default)
    {
    }

    public EncodedStringBuilder(Encoding encoding) : this(string.Empty, encoding)
    {
    }

    public EncodedStringBuilder(string @string, Encoding encoding)
    {
        _string = @string;
        _encoding = encoding;
    }

    public EncodedStringBuilder SetEncoding(Encoding newEncoding) => new(_string, newEncoding);
    public EncodedStringBuilder SetString(string newString) => new(newString, _encoding);

    public EncodedString Create()
    {
        return new EncodedString(_encoding.GetBytes(_string), _encoding, _string);
    }

    public static implicit operator EncodedString(EncodedStringBuilder builder) => builder.Create();
}
