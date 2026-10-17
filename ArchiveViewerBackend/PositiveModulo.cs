namespace ArchiveViewerBackend;

/// <summary>
/// Applies modulo operator to a value to keep it within the range [0, base - 1]
/// </summary>
public static class PositiveModulo
{
    public static long Fix(long value, long @base)
    {
        var newValue = value % @base;
        return newValue >= 0 ? newValue : newValue + @base;
    }

    public static int Fix(int value, int @base)
    {
        var newValue = value % @base;
        return newValue >= 0 ? newValue : newValue + @base;
    }
}
