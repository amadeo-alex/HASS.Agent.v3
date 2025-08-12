using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using HASS.Agent.UI.Contracts;
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
public sealed partial class GeneralSettingsPage : Page, IManagedPage
{
	private GeneralSettingsPageViewModel ViewModel { get => (GeneralSettingsPageViewModel)DataContext; }
	public GeneralSettingsPage()
	{
		this.InitializeComponent();
	}

	public void OnDataContextChange()
	{

	}

	private async void ChangeNameButton_Click(object sender, RoutedEventArgs e)
	{
		var dialog = new InputContentDialog
		{
			XamlRoot = XamlRoot,
			Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
			Title = LocalizerHelper.GetLocalizedString("Page_GeneralSettings_NameDialog_Title"),
			PrimaryButtonText = LocalizerHelper.GetLocalizedString("General_Save"),
			CloseButtonText = LocalizerHelper.GetLocalizedString("General_Cancel"),
			DefaultButton = ContentDialogButton.Primary,
			Content = new InputDialogContent("Page_GeneralSettings_NameDialog_Query", ViewModel.SettingsManager.Settings.Application.ConfiguredDeviceName)
		};

		var result = await dialog.ShowAsync();
		if (result == ContentDialogResult.Primary)
		{
			var newDeviceName = dialog.GetInputContent<string>();
			if (newDeviceName != null)
				ViewModel.SettingsManager.Settings.Application.ConfiguredDeviceName = newDeviceName;
		}
	}
}
