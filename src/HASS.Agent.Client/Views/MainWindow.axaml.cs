using Avalonia.Controls;
using FluentAvalonia.UI.Windowing;

namespace HASS.Agent.Client.Views;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
	    TitleBar.ExtendsContentIntoTitleBar = true;
        InitializeComponent();
    }
}