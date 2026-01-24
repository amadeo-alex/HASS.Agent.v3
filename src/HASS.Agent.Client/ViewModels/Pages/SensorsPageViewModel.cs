using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models.Entity;

namespace HASS.Agent.Client.ViewModels.Pages;

public class SensorsPageViewModel : ViewModelBase, INavigationAware
{
    private readonly ISettingsManager _settingsManager;

    public ObservableCollection<ConfiguredEntity> Sensors => _settingsManager.ConfiguredSensors;

    public SensorsPageViewModel(Dispatcher dispatcher, ISettingsManager settingsManager) : base(dispatcher)
    {
        _settingsManager = settingsManager;
    }

    public void OnNavigatedTo()
    {
    }

    public void OnNavigatedFrom()
    {
    }
}