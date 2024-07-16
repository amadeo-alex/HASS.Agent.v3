using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using HASS.Agent.UI.Helpers;
using HASS.Agent.UI.ViewModels;
using HASS.Agent.UI.ViewModels.Settings;
using HASS.Agent.UI.Views.Dialogs;
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
public sealed partial class MqttSettingsPage : Page
{
    private readonly MqttSettingsPageViewModel _viewModel;

    public MqttSettingsPage()
    {
        _viewModel = App.GetService<MqttSettingsPageViewModel>();
        this.InitializeComponent();
        DataContext = _viewModel;
    }

    private async void ChangeAddressButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new InputContentDialog
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = LocalizerHelper.GetLocalizedString("Page_MqttSettings_AddressDialog_Title"),
            PrimaryButtonText = LocalizerHelper.GetLocalizedString("General_Save"),
            CloseButtonText = LocalizerHelper.GetLocalizedString("General_Cancel"),
            DefaultButton = ContentDialogButton.Primary,
            Content = new InputDialogContent("Page_MqttSettings_AddressDialog_Query", _viewModel.SettingsManager.Settings.Mqtt.Address)
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var newAddress = dialog.GetInputContent<string>();
            if (newAddress != null)
                _viewModel.SettingsManager.Settings.Mqtt.Address = newAddress;
        }
    }

    private async void ChangePortButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new InputContentDialog
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = LocalizerHelper.GetLocalizedString("Page_MqttSettings_PortDialog_Title"),
            PrimaryButtonText = LocalizerHelper.GetLocalizedString("General_Save"),
            CloseButtonText = LocalizerHelper.GetLocalizedString("General_Cancel"),
            DefaultButton = ContentDialogButton.Primary,
            Content = new InputDialogContent("Page_MqttSettings_PortDialog_Query", Convert.ToString(_viewModel.SettingsManager.Settings.Mqtt.Port), true)
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var newPort = dialog.GetInputContent<int>();
            if (newPort != default)
                _viewModel.SettingsManager.Settings.Mqtt.Port = newPort;
        }
    }

    private async void ChangeUsernameButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new InputContentDialog
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = LocalizerHelper.GetLocalizedString("Page_MqttSettings_UsernameDialog_Title"),
            PrimaryButtonText = LocalizerHelper.GetLocalizedString("General_Save"),
            CloseButtonText = LocalizerHelper.GetLocalizedString("General_Cancel"),
            DefaultButton = ContentDialogButton.Primary,
            Content = new InputDialogContent("Page_MqttSettings_UsernameDialog_Query", _viewModel.SettingsManager.Settings.Mqtt.Username)
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var newUsername = dialog.GetInputContent<string>();
            if (newUsername != null)
                _viewModel.SettingsManager.Settings.Mqtt.Username = newUsername;
        }
    }

    private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new InputContentDialog
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = LocalizerHelper.GetLocalizedString("Page_MqttSettings_PasswordDialog_Title"),
            PrimaryButtonText = LocalizerHelper.GetLocalizedString("General_Save"),
            CloseButtonText = LocalizerHelper.GetLocalizedString("General_Cancel"),
            DefaultButton = ContentDialogButton.Primary,
            Content = new InputDialogContent("Page_MqttSettings_PasswordDialog_Query", _viewModel.SettingsManager.Settings.Mqtt.Password)
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var newPassword = dialog.GetInputContent<string>();
            if (newPassword != null)
                _viewModel.SettingsManager.Settings.Mqtt.Password = newPassword;
        }
    }
}
