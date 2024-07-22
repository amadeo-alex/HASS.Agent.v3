using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using Windows.Storage.Pickers;

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
        var newValue = await GetNewSettingValueAsync<string>("Page_MqttSettings_AddressDialog_Title", "Page_MqttSettings_AddressDialog_Query", _viewModel.SettingsManager.Settings.Mqtt.Address);
        if (!string.IsNullOrEmpty(newValue))
        {
            _viewModel.SettingsManager.Settings.Mqtt.Address = newValue;
        }
    }

    private async void ChangePortButton_Click(object sender, RoutedEventArgs e)
    {
        var newValue = await GetNewSettingValueAsync<int>("Page_MqttSettings_PortDialog_Title", "Page_MqttSettings_PortDialog_Query", Convert.ToString(_viewModel.SettingsManager.Settings.Mqtt.Port), true);
        if (newValue != default)
        {
            _viewModel.SettingsManager.Settings.Mqtt.Port = newValue;
        }
    }

    private async void ChangeUsernameButton_Click(object sender, RoutedEventArgs e)
    {
        var newValue = await GetNewSettingValueAsync<string>("Page_MqttSettings_UsernameDialog_Title", "Page_MqttSettings_UsernameDialog_Query", _viewModel.SettingsManager.Settings.Mqtt.Username);
        if (!string.IsNullOrEmpty(newValue))
        {
            _viewModel.SettingsManager.Settings.Mqtt.Username = newValue;
        }
    }

    private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
    {
        var newValue = await GetNewSettingValueAsync<string>("Page_MqttSettings_PasswordDialog_Title", "Page_MqttSettings_PasswordDialog_Query", _viewModel.SettingsManager.Settings.Mqtt.Password);
        if (!string.IsNullOrEmpty(newValue))
        {
            _viewModel.SettingsManager.Settings.Mqtt.Password = newValue;
        }
    }

    private async void ChangeDiscoveryPrefixButton_Click(object sender, RoutedEventArgs e)
    {
        var newValue = await GetNewSettingValueAsync<string>("Page_MqttSettings_DiscoveryPrefixDialog_Title", "Page_MqttSettings_DiscoveryPrefixDialog_Query", _viewModel.SettingsManager.Settings.Mqtt.DiscoveryPrefix);
        if (!string.IsNullOrEmpty(newValue))
        {
            _viewModel.SettingsManager.Settings.Mqtt.DiscoveryPrefix = newValue;
        }
    }

    private async void ChangeClientIdButton_Click(object sender, RoutedEventArgs e)
    {
        var newValue = await GetNewSettingValueAsync<string>("Page_MqttSettings_ClientIdDialog_Title", "Page_MqttSettings_ClientIdDialog_Query", _viewModel.SettingsManager.Settings.Mqtt.ClientId);
        if (!string.IsNullOrEmpty(newValue))
        {
            _viewModel.SettingsManager.Settings.Mqtt.ClientId = newValue;
        }
    }

    private async void ChangeGracePeriodButton_Click(object sender, RoutedEventArgs e)
    {
        var newValue = await GetNewSettingValueAsync<string>("Page_MqttSettings_GracePeriodDialog_Title", "Page_MqttSettings_GracePeriodDialog_Query", Convert.ToString(_viewModel.SettingsManager.Settings.Mqtt.GracePeriodSeconds), true);
        if (!string.IsNullOrEmpty(newValue))
        {
            _viewModel.SettingsManager.Settings.Mqtt.ClientId = newValue;
        }
    }

    private async void ChangeClientCertificateButton_Click(object sender, RoutedEventArgs e)
    {
        var openPicker = new FileOpenPicker();
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        openPicker.ViewMode = PickerViewMode.Thumbnail;
        openPicker.FileTypeFilter.Add("*");

        var certificateFile = await openPicker.PickSingleFileAsync();
        if (certificateFile != null)
        {
            _viewModel.SettingsManager.Settings.Mqtt.ClientCertificatePath = certificateFile.Path;
        }
    }

    private async void ChangeRootCertificateButton_Click(object sender, RoutedEventArgs e)
    {
        var openPicker = new FileOpenPicker();
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        openPicker.ViewMode = PickerViewMode.Thumbnail;
        openPicker.FileTypeFilter.Add("*");

        var certificateFile = await openPicker.PickSingleFileAsync();
        if (certificateFile != null)
        {
            _viewModel.SettingsManager.Settings.Mqtt.RootCertificatePath = certificateFile.Path;
        }
    }

    private async Task<T?> GetNewSettingValueAsync<T>(string titleResourceKey, string queryResourceKey, string currentValue, bool numericOnly = false)
    {
        var dialog = new InputContentDialog
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = LocalizerHelper.GetLocalizedString(titleResourceKey),
            PrimaryButtonText = LocalizerHelper.GetLocalizedString("General_Save"),
            CloseButtonText = LocalizerHelper.GetLocalizedString("General_Cancel"),
            DefaultButton = ContentDialogButton.Primary,
            Content = new InputDialogContent(queryResourceKey, currentValue, numericOnly)
        };

        var result = await dialog.ShowAsync();

        return result == ContentDialogResult.Primary ? dialog.GetInputContent<T>() : default;
    }
}
