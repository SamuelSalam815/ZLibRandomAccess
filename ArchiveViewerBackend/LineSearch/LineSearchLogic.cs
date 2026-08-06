using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace ArchiveViewerBackend.LineSearch;

public record LineSearchLogic(EncodedString EncodedString, Regex Regex)
{

    private int _currentCharacterOffset = 0;

    public LineSearchLogic(
        EncodedString encodedString,
        [StringSyntax(StringSyntaxAttribute.Regex)] string regex) : this(encodedString, new Regex(regex))
    {
    }

    [Pure]
    public LineSearchLogic NextMatch(out LineSearchResult? match)
    {
        var regexMatch = Regex.Match(EncodedString.String, _currentCharacterOffset);
        if (!regexMatch.Success)
        {
            match = null;
            return this;
        }
        match = new LineSearchResult(regexMatch.Value, EncodedString.ByteOffsetOf(regexMatch.Index));

        return this with {_currentCharacterOffset = regexMatch.Index + 1};
    }

    public LineSearchLogic SetSearchPattern(Regex searchPattern) => this with { Regex = searchPattern };
}
