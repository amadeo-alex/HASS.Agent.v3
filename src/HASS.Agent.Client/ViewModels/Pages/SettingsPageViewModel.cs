using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HASS.Agent.Client.ViewModels.Dialogs;
using HASS.Agent.Client.Views.Dialogs;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models;
using HASS.Agent.Contracts.Services;
using Ursa.Controls;

namespace HASS.Agent.Client.ViewModels.Pages;

public partial class SettingsPageViewModel : ViewModelBase, INavigationAware
{
    private readonly ISettingsManager _settingsManager;
    private readonly IDialogService _dialogService;
    
    private ApplicationSettings _applicationSettingsSnapshot;

    public List<AnchorItemViewModel> SettingsSections { get; }

    public string DeviceName
    {
        get => _applicationSettingsSnapshot.DeviceName;
        set
        {
            _applicationSettingsSnapshot.DeviceName = value;
            RaiseOnPropertyChanged(nameof(DeviceName));
        }
    }

    public string SerialNumber
    {
        get => _applicationSettingsSnapshot.SerialNumber;
        set
        {
            _applicationSettingsSnapshot.SerialNumber = value;
            RaiseOnPropertyChanged(nameof(DeviceName));
        }
    }

    public SettingsPageViewModel(Dispatcher dispatcher, ISettingsManager settingsManager, IDialogService dialogService) : base(dispatcher)
    {
        _settingsManager = settingsManager;
        _dialogService = dialogService;

        _applicationSettingsSnapshot = settingsManager.GetSettingsSnapshot<ApplicationSettings>();

        SettingsSections =
        [
            new AnchorItemViewModel() { Header = Translations.Strings.SettingsGeneralSection.CurrentValue },
            new AnchorItemViewModel() { Header = Translations.Strings.SettingsMqttSection.CurrentValue },
            new AnchorItemViewModel() { Header = Translations.Strings.SettingsHASection.CurrentValue },
            new AnchorItemViewModel() { Header = Translations.Strings.SettingsNotificationsSection.CurrentValue }
        ];
    }

    public void OnNavigatedTo()
    {
    }

    public void OnNavigatedFrom()
    {
    }

    [RelayCommand]
    private async Task ChangeDeviceName()
    {
        var vm = new TextInputDialogViewModel()
        {
            Title = string.Empty,
            Query = Translations.Strings.SettingsGeneralDeviceNameChangeDialogQuery.CurrentValue,
            Accept = Translations.Strings.Save.CurrentValue,
            Deny = Translations.Strings.Cancel.CurrentValue,
            UserText = DeviceName
        };

        var result = await _dialogService.ShowDialogAsync(vm);

        if (!result.Confirmed)
        {
            return;
        }

        if (result.UserText != DeviceName)
        {
            DeviceName = result.UserText;
            var saved = _settingsManager.SaveSettings(_applicationSettingsSnapshot);

            //TODO(Amadeo): notify user when not saved?
        }
    }
}