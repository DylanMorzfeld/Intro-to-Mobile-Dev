using System.Globalization;

namespace FitTrack.Converters;

/// <summary>
/// Converts a workout's IsCompleted flag into different visual values
/// depending on which property is binding to it, selected via
/// ConverterParameter. This avoids needing a separate converter class for
/// every UI property that should react to the same underlying boolean.
///
/// Usage:
///   Opacity="{Binding IsCompleted, Converter={StaticResource CompletionStyleConverter}, ConverterParameter=Opacity}"
///   BackgroundColor="{Binding IsCompleted, Converter={StaticResource CompletionStyleConverter}, ConverterParameter=Background}"
/// </summary>
public class CompletionStyleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isCompleted = value is true;
        string mode = parameter as string ?? string.Empty;

        return mode switch
        {
            "Opacity" => isCompleted ? 0.6 : 1.0,
            "Background" => isCompleted ? Color.FromArgb("#E8F5E9") : Colors.Transparent,
            _ => throw new ArgumentException($"Unknown ConverterParameter '{mode}' for CompletionStyleConverter.")
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}