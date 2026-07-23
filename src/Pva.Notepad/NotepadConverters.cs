using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Pva.Notepad;

/// <summary>true → <see cref="TextWrapping.Wrap"/>، false → <see cref="TextWrapping.NoWrap"/>.</summary>
public sealed class BoolToTextWrappingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? TextWrapping.Wrap : TextWrapping.NoWrap;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is TextWrapping.Wrap;
}

/// <summary>true → <see cref="FlowDirection.RightToLeft"/>، false → <see cref="FlowDirection.LeftToRight"/>.</summary>
public sealed class BoolToFlowDirectionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is FlowDirection.RightToLeft;
}
