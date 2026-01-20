using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HASS.Agent.Contracts.Enums;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models.Settings;

namespace HASS.Agent.Client.ViewModels.Pages;

public partial class HomePageViewModel : ViewModelBase, INavigationAware
{
    private readonly IMqttManager _mqttManager;
    private readonly ISensorManager _sensorManager;
    private readonly ICommandsManager _commandsManager;

    [ObservableProperty]
    private bool _refreshingServiceInformation = false;

    public ManagerStatus MqttStatus => _mqttManager.Status;
    public ManagerStatus SensorsManagerStatus => _sensorManager.Status;
    public ManagerStatus CommandsManagerStatus => _commandsManager.Status;

    public IAsyncRelayCommand RefreshSatelliteInformationCommand { get; set; }

    public IAsyncRelayCommand KillMqtt { get; set; }
    public IRelayCommand KillSensors { get; set; }
    public IRelayCommand KillCommands { get; set; }

    public IAsyncRelayCommand StartMqtt { get; set; }

    public HomePageViewModel(Dispatcher dispatcher, IMqttManager mqttManager, ISensorManager sensorManager, ICommandsManager commandsManager) : base(dispatcher)
    {
        _mqttManager = mqttManager;
        _sensorManager = sensorManager;
        _commandsManager = commandsManager;

        RefreshSatelliteInformationCommand = new AsyncRelayCommand(async () =>
        {
            RefreshingServiceInformation = true;
            await Task.Delay(2000);
            RefreshingServiceInformation = false;
        });

        KillMqtt = new AsyncRelayCommand(async () => { await _mqttManager.StopClientAsync(); });

        KillSensors = new RelayCommand(() => { _sensorManager.Exit = true; });

        KillCommands = new RelayCommand(() => { _commandsManager.Exit = true; });

        StartMqtt = new AsyncRelayCommand(async () => { await _mqttManager.StartClientAsync(); });

        AddPropertyListenerMap(nameof(_mqttManager.Status),
        [
            nameof(MqttStatus),
            nameof(SensorsManagerStatus),
            nameof(CommandsManagerStatus)
        ]);
    }

    public void OnNavigatedTo()
    {
        _mqttManager.PropertyChanged += OnListenedPropertyChanged;
        _sensorManager.PropertyChanged += OnListenedPropertyChanged;
        _commandsManager.PropertyChanged += OnListenedPropertyChanged;
    }

    public void OnNavigatedFrom()
    {
        _mqttManager.PropertyChanged -= OnListenedPropertyChanged;
        _sensorManager.PropertyChanged -= OnListenedPropertyChanged;
        _commandsManager.PropertyChanged -= OnListenedPropertyChanged;
    }

    private void OnListenedPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        ParseSourcePropertyChanged(e);
    }
}