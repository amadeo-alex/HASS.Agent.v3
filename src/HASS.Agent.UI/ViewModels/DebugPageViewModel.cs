﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Base.Models;
using HASS.Agent.Base.Sensors.SingleValue;
using HASS.Agent.UI.Contracts.ViewModels;
using HASS.Agent.UI.Views.Dialogs;
using HASS.Agent.UI.Views.Pages;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using HASS.Agent.Contracts.Enums;

namespace HASS.Agent.UI.ViewModels;
public partial class DebugPageViewModel : ViewModelBase, INavigationAware
{
    private ISettingsManager _settingsManager;
    private IMqttManager _mqttManager;
    private int x = 0;


    public RelayCommand ButtonCommand { get; set; }
    public RelayCommand ButtonCommand2 { get; set; }
    public RelayCommand ButtonCommand3 { get; set; }
    public RelayCommand ButtonCommand4 { get; set; }
    public RelayCommand ButtonCommand5 { get; set; }
	public RelayCommand ButtonCommand6 { get; set; }
	public MqttStatus MqttStatus => _mqttManager.Status;

    public List<SomeItem> SomeItems = new List<SomeItem>()
    {
        new() { Name = "a"},
        new() { Name = "b"},
        new() { Name = "c"},
    };

    public DebugPageViewModel(DispatcherQueue dispatcherQueue, ISettingsManager settingsManager, IMqttManager mqttManager) : base(dispatcherQueue)
    {
        _settingsManager = settingsManager;
        _mqttManager = mqttManager;

        ButtonCommand = new RelayCommand(() =>
        {
            _settingsManager.ConfiguredSensors.Add(new ConfiguredEntity
            {
                Type = typeof(DummySensor).Name,
                Name = $"Dummy Sensor {x}",
                EntityIdName = $"dummysensor{x}",
                UniqueId = Guid.NewGuid(),
                UpdateIntervalSeconds = 20
            });

            x++;
        });

        ButtonCommand2 = new RelayCommand(() =>
        {
            _mqttManager.StopClientAsync();
        });

		ButtonCommand6 = new RelayCommand(() =>
		{
			_mqttManager.StartClientAsync();
		});


		ButtonCommand3 = new RelayCommand(async () =>
        {
            Debug.WriteLine("GC collect");
            GC.Collect();

        });

        ButtonCommand4 = new RelayCommand(async () =>
        {
            _settingsManager.StoreConfiguredEntities();
            _settingsManager.StoreSettings();
        });

        ButtonCommand5 = new RelayCommand(async () =>
        {
            App.MainWindow.Close();
        });
    }

    private void OnMqttPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_mqttManager.Status))
        {
            RaiseOnPropertyChanged(nameof(MqttStatus));
        }
    }

	public void OnNavigatedTo(object parameter)
	{
		_mqttManager.PropertyChanged += OnMqttPropertyChanged;
	}

	public void OnNavigatedFrom()
	{
		_mqttManager.PropertyChanged -= OnMqttPropertyChanged; //Note(Amadeo): leaks
	}
}
