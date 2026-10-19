namespace ArchiveViewerBackend;

internal static class Utf16
{
    /// <summary>
    /// A character above U+FFFF is a surrogate pair of two UTF-16 code units (<see cref="char"/>s). A
    /// <see cref="System.Text.Decoder"/> throws unless its output has room for a whole character, so a character
    /// buffer smaller than this can never decode one.
    /// </summary>
    public const int MaxCodeUnitsPerCharacter = 2;
}
