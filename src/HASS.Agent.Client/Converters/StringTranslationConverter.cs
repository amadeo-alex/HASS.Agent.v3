using System;
using System.Globalization;
using System.Reflection;
using Avalonia.Data.Converters;
using Echoes;
using HASS.Agent.Translations;

namespace HASS.Agent.Client.Converters;

public class StringTranslationConverter : IValueConverter
{
    public string Prefix { get; set; } = string.Empty;
    public string Suffix { get; set; } = string.Empty;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string stringValue)
        {
            return null;
        }

        return TranslationHelper.Get(stringValue, Prefix, Suffix);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}