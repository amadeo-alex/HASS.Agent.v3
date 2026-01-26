using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using HASS.Agent.Base.Sensors.SingleValue;
using HASS.Agent.Client.ViewModels.Dialogs;
using HASS.Agent.Client.ViewModels.Sensors;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models.Entity;
using HASS.Agent.Contracts.Services;
using Newtonsoft.Json;
using Ursa.Controls;

namespace HASS.Agent.Client.ViewModels.Pages;

public partial class SensorsPageViewModel : ViewModelBase, INavigationAware
{
    private readonly ISettingsManager _settingsManager;
    private readonly ISensorManager _sensorManager;
    private readonly IEntityTypeRegistry _entityTypeRegistry;
    private readonly IDialogService _dialogService;
    private readonly IGuidManager _guidManager;

    public ObservableCollection<ConfiguredEntity> Sensors => _settingsManager.ConfiguredSensors;

    public SensorsPageViewModel(Dispatcher dispatcher, ISettingsManager settingsManager, ISensorManager sensorManager, IEntityTypeRegistry entityTypeRegistry,
        IDialogService dialogService, IGuidManager guidManager) : base(dispatcher)
    {
        _settingsManager = settingsManager;
        _sensorManager = sensorManager;
        _entityTypeRegistry = entityTypeRegistry;
        _dialogService = dialogService;
        _guidManager = guidManager;
    }

    [RelayCommand]
    private async Task AddSensor()
    {
        var guid = _guidManager.GenerateGuid();
        
        var dialogVm = new SensorEditDialogViewModel()
        {
            ShowEntityCategories = true,
            EntityCategories = _entityTypeRegistry.SensorsCategories,
            Sensor = new ConfiguredEntity()
            {
                UniqueId = guid
            }
        };

        var result = await _dialogService.ShowDialogAsync(dialogVm);
        if (result.Confirmed)
        {
            _settingsManager.ConfiguredSensors.Add(result.Sensor);
        }
        else
        {
            _guidManager.MarkAsUnused(guid);
        }
    }

    [RelayCommand]
    private void RemoveSensor(ConfiguredEntity sensor)
    {
        var configuredSensor = _settingsManager.ConfiguredSensors.FirstOrDefault(s => s.UniqueId == sensor.UniqueId);
        if (configuredSensor != null)
        {
            _settingsManager.ConfiguredSensors.Remove(configuredSensor);
        }
    }

    [RelayCommand]
    private async Task EditSensor(ConfiguredEntity sensor)
    {
        var editedSensor = (ConfiguredEntity)sensor.Clone();
        var dialogVm = new SensorEditDialogViewModel(editedSensor)
        {
            EntityCategories = _entityTypeRegistry.SensorsCategories
        };

        var result = await _dialogService.ShowDialogAsync(dialogVm);
        if (result.Confirmed)
        {
            _settingsManager.ConfiguredSensors.Remove(sensor);
            _settingsManager.ConfiguredSensors.Add(editedSensor);
        }
    }

    [RelayCommand]
    private void StartStopSensor(ConfiguredEntity sensor)
    {
        //TODO(Amadeo): consider enforcing the logic where only ConfiguredSensors are modified and others read only
        var sensorEntity = _sensorManager.Sensors.FirstOrDefault(s => s.UniqueId == sensor.UniqueId.ToString());
        if (sensorEntity == null)
        {
            return;
        }

        var active = !sensorEntity.Active;
        sensorEntity.Active = active;
        var configuredSensor = _settingsManager.ConfiguredSensors.FirstOrDefault(s => s.UniqueId == sensor.UniqueId);
        if (configuredSensor != null) //NOTE(Amadeo): technically impossible?
        {
            configuredSensor.Active = active;
        }
    }

    public void OnNavigatedTo()
    {
    }

    public void OnNavigatedFrom()
    {
    }
}