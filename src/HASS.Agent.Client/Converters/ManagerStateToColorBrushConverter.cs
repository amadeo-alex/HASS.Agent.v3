using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using HASS.Agent.Contracts.Enums;

namespace HASS.Agent.Client.Converters;

public class ManagerStateToColorBrushConverter : IValueConverter
{
    //TODO(Amadeo): get dynamic resources based on button class foreground brushes
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ManagerStatus status)
        {
            return null;
        }

        return status switch
        {
            ManagerStatus.Stopped or ManagerStatus.NotInitialized or ManagerStatus.Disconnected or ManagerStatus.Error => Brushes.Red,
            ManagerStatus.Running => Brushes.Green,
            ManagerStatus.Initializing or ManagerStatus.Connecting => Brushes.Orange,
            _ => BindingOperations.DoNothing
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}