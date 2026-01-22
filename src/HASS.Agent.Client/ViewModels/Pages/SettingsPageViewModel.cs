using System.Collections.Generic;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models;

namespace HASS.Agent.Client.ViewModels.Pages;

public partial class SettingsPageViewModel : ViewModelBase, INavigationAware
{
    private ISettingsManager _settingsManager { get; }

    private ApplicationSettings _applicationSettingsSnapshot { get; }

    public List<AnchorItemViewModel> SettingsSections { get; }

    public string DeviceName
    {
        get => _applicationSettingsSnapshot.DeviceName;
        set => _applicationSettingsSnapshot.DeviceName = value;
    }

    public SettingsPageViewModel(Dispatcher dispatcher, ISettingsManager settingsManager) : base(dispatcher)
    {
        _settingsManager = settingsManager;

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
}