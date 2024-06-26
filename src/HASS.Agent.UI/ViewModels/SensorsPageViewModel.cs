﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HASS.Agent.Base.Contracts.Managers;
using HASS.Agent.Base.Contracts.Models.Entity;
using HASS.Agent.Base.Models;
using HASS.Agent.Base.Models.Entity;
using HASS.Agent.UI.Contracts.ViewModels;
using Microsoft.UI.Dispatching;
using Windows.AI.MachineLearning;

namespace HASS.Agent.UI.ViewModels;

public partial class SensorsPageViewModel : ViewModelBase, IInfoBadgeAware, INavigationAware
{
    private readonly ISensorManager _sensorManager;
    private readonly ISettingsManager _settingsManager;
    private readonly IEntityTypeRegistry _entityTypeRegistry;
    private readonly IGuidManager _guidManager;

    private IInfoBadge _badge = new InfoBadge()
    {
        Type = InfoBadgeType.Success,
        Value = 0
    };

    public ObservableCollection<AbstractDiscoverableViewModel> Sensors { get; } = [];
    public List<EntityCategory> SensorsCategories => _entityTypeRegistry.SensorsCategories.SubCategories;

    public RelayCommand<AbstractDiscoverableViewModel> EditCommand { get; set; }
    public RelayCommand<AbstractDiscoverableViewModel> StartStopCommand { get; set; }
    public RelayCommand<AbstractDiscoverableViewModel> DeleteCommand { get; set; }
    public RelayCommand NewCommand { get; set; }

    public event EventHandler<ConfiguredEntity>? SensorEditEventHandler;
    public event EventHandler<ConfiguredEntity>? NewSensorEventHandler;
    public SensorsPageViewModel(DispatcherQueue dispatcherQueue, ISensorManager sensorManager, ISettingsManager settingsManager, IEntityTypeRegistry entityTypeRegistry, IGuidManager guidManager) : base(dispatcherQueue)
    {
        Debug.WriteLine("constructor");

        _sensorManager = sensorManager;
        _settingsManager = settingsManager;
        _entityTypeRegistry = entityTypeRegistry;
        _guidManager = guidManager;

        foreach (var sensor in _sensorManager.Sensors)
            Sensors.Add(new AbstractDiscoverableViewModel
            {
                Active = sensor.Active,
                Name = sensor.Name,
                Type = sensor.GetType().Name,
                UniqueId = sensor.UniqueId
            });

        _badge.Value = Sensors.Count;

        EditCommand = new RelayCommand<AbstractDiscoverableViewModel>((sensorViewModel) =>
        {
            if (sensorViewModel == null)
                return;

            var configuredSensor = _settingsManager.ConfiguredSensors.FirstOrDefault(s => s.UniqueId.ToString() == sensorViewModel.UniqueId);
            if (configuredSensor == null)
                return;

            SensorEditEventHandler?.Invoke(this, (ConfiguredEntity)configuredSensor.Clone());
        });

        StartStopCommand = new RelayCommand<AbstractDiscoverableViewModel>((sensorViewModel) =>
        {
            if (sensorViewModel == null)
                return;

            var sensor = _sensorManager.Sensors.FirstOrDefault(s => s.UniqueId == sensorViewModel.UniqueId);
            if (sensor != null)
            {
                sensor.Active = !sensor.Active;
                sensorViewModel.Active = sensor.Active;

                var configuredSensor = _settingsManager.ConfiguredSensors.First(cs=>cs.UniqueId.ToString() == sensorViewModel.UniqueId);
                configuredSensor.Active = sensor.Active;
            }
        });

        DeleteCommand = new RelayCommand<AbstractDiscoverableViewModel>((sensorViewModel) =>
        {
            if (sensorViewModel == null)
                return;

            var configuredEntity = _settingsManager.ConfiguredSensors.FirstOrDefault(cs => cs.UniqueId.ToString() == sensorViewModel.UniqueId);
            if (configuredEntity != null)
                _settingsManager.ConfiguredSensors.Remove(configuredEntity);
        });

        NewCommand = new RelayCommand(() =>
        {
            var newSensor = new ConfiguredEntity
            {
                UniqueId = _guidManager.GenerateGuid(),
            };
            NewSensorEventHandler?.Invoke(this, newSensor);
        });

        RaiseOnPropertyChanged(nameof(InfoBadge));
    }

    public void AddUpdateConfiguredSensor(ConfiguredEntity sensor)
    {
        var existingSensor = _settingsManager.ConfiguredSensors.FirstOrDefault(s => s.UniqueId == sensor.UniqueId);
        if (existingSensor != null)
        {
            if(existingSensor == sensor)
                return;

            if (existingSensor.Type != sensor.Type)
                throw new ArgumentException($"sensor with ID {existingSensor.UniqueId} of different type ({existingSensor.Type}) than {sensor.Type} already exists");

            _settingsManager.ConfiguredSensors.Remove(existingSensor);
        }

        _settingsManager.ConfiguredSensors.Add(sensor);
    }

    private void Sensors_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems == null)
                    return;

                foreach (AbstractDiscoverable newSensor in e.NewItems)
                    RunOnDispatcher(() => Sensors.Add(new AbstractDiscoverableViewModel
                    {
                        Active = true,
                        Name = newSensor.Name,
                        Type = newSensor.GetType().Name,
                        UniqueId = newSensor.UniqueId
                    }));

                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems == null)
                    return;

                foreach (AbstractDiscoverable oldSensor in e.OldItems)
                {
                    var sensorViewModel = Sensors.FirstOrDefault(s => s.UniqueId == oldSensor.UniqueId);
                    if (sensorViewModel == null)
                        continue;

                    RunOnDispatcher(() => Sensors.Remove(sensorViewModel));
                }

                break;
        }


        _badge.Value = Sensors.Count;
        RaiseOnPropertyChanged(nameof(InfoBadge));
        RaiseOnPropertyChanged(nameof(Sensors));
    }

    public IInfoBadge InfoBadge
    {
        get => _badge;
        set
        {
            _badge = value;
            RaiseOnPropertyChanged(nameof(InfoBadge));
        }
    }
    public void OnNavigatedFrom()
    {
        _sensorManager.Sensors.CollectionChanged -= Sensors_CollectionChanged;
    }
    public void OnNavigatedTo(object parameter)
    {
        _sensorManager.Sensors.CollectionChanged += Sensors_CollectionChanged;
    }

    ~SensorsPageViewModel()
    {
        Debug.WriteLine("desctructor");
    }
}
