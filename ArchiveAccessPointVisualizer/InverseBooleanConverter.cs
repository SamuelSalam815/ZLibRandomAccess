using System.Windows.Data;

namespace ArchiveAccessPointVisualizer;

// Source - https://stackoverflow.com/a/1039681
// Posted by Chris Nicol, modified by community. See post 'Timeline' for change history
// Retrieved 2026-09-24, License - CC BY-SA 4.0

[ValueConversion(typeof(bool), typeof(bool))]
public class InverseBooleanConverter : IValueConverter
{
    #region IValueConverter Members

    public object Convert(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        if (targetType != typeof(bool))
        {
            throw new InvalidOperationException("The target must be a boolean");
        }

        return !(bool)value!;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    #endregion
}
