using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using HASS.Agent.UI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HASS.Agent.UI.Views.Pages.Settings;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingsPage : Page
{
    private readonly SettingsPageViewModel _viewModel;

    public SettingsPage()
    {
        _viewModel = App.GetService<SettingsPageViewModel>();
        this.InitializeComponent();
        DataContext = _viewModel;

        _viewModel.NavigationService.Frame = SettingsNavigationFrame;
        _viewModel.NavigationViewService.Initialize(SettingsNavigationViewControl);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        _viewModel.NavigationService.NavigateTo("settingsGeneral");
    }
}
