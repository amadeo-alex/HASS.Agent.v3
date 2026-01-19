using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using HASS.Agent.Client.ViewModels;
using LogViewer.Core;
using Microsoft.Extensions.Logging;

namespace HASS.Agent.Client.Views;

public partial class LoggerWindow : Window
{
    public LoggerWindow(LoggerWindowViewModel viewModel, DataStoreLoggerConfiguration config)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}