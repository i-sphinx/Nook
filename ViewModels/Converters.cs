using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Nook.ViewModels;

/// <summary>
/// Static value converters used inline in AXAML via x:Static.
/// </summary>
public static class Converters
{
    /// <summary>Returns true when an int is greater than zero (shows badge counts).</summary>
    public static readonly IValueConverter IsPositive =
        new FuncValueConverter<int, bool>(v => v > 0);

    /// <summary>Returns true when an int is zero (shows empty-state placeholder).</summary>
    public static readonly IValueConverter IsZero =
        new FuncValueConverter<int, bool>(v => v == 0);

    /// <summary>Converts a 0‒100 progress value to a pixel width scaled to ConverterParameter.</summary>
    public static readonly IValueConverter ProgressToWidth =
        new FuncValueConverter<double, double>(v => v / 100.0 * 560);

    /// <summary>
    /// Converts a bool (ShowPreviewMode) to the Organize button label.
    /// true  → "Preview"
    /// false → "Organize"
    /// </summary>
    public static readonly IValueConverter BoolToPreviewLabel =
        new FuncValueConverter<bool, string>(v => v ? "Preview" : "Organize");
}
