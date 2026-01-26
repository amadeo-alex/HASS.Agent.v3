using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using HASS.Agent.Base.Sensors.SingleValue;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models.Entity;
using Newtonsoft.Json;

namespace HASS.Agent.Client.ViewModels.Pages;

public partial class SensorsPageViewModel : ViewModelBase, INavigationAware
{
    private readonly ISettingsManager _settingsManager;
    private readonly ISensorManager _sensorManager;

    public ObservableCollection<ConfiguredEntity> Sensors => _settingsManager.ConfiguredSensors;

    public SensorsPageViewModel(Dispatcher dispatcher, ISettingsManager settingsManager, ISensorManager sensorManager) : base(dispatcher)
    {
        _settingsManager = settingsManager;
        _sensorManager = sensorManager;
    }

    [RelayCommand]
    private void AddSensor()
    {
        var nsen = new ConfiguredEntity()
        {
            Type = typeof(DummySensor).Name,
            EntityIdName = "DummySensorX1",
            Name = "Dummy Sensor X1",
            UpdateIntervalSeconds = 5,
            UniqueId = Guid.NewGuid(),
            Active = true,
        };
        
        _settingsManager.ConfiguredSensors.Add(nsen);
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
    private void EditSensor(ConfiguredEntity sensor)
    {
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