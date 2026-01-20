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
    
    [ObservableProperty]
    private bool _refreshingServiceInformation = false;
    
    public ManagerStatus MqttStatus => _mqttManager.Status;
    public ManagerStatus SensorsManagerStatus { get; set; }
    public ManagerStatus CommandsManagerStatus { get; set; } 
    
    public IAsyncRelayCommand RefreshSatelliteInformationCommand { get; set; }
    
    public HomePageViewModel(Dispatcher dispatcher, IMqttManager mqttManager) : base(dispatcher)
    {
        _mqttManager = mqttManager;
        
        RefreshSatelliteInformationCommand = new AsyncRelayCommand(async () =>
        {
            RefreshingServiceInformation = true;
            await Task.Delay(2000);
            RefreshingServiceInformation = false;
        });
        
        AddPropertyListenerMap(nameof(_mqttManager.Status), nameof(MqttStatus));
    }
    
    public void OnNavigatedTo()
    {
        _mqttManager.PropertyChanged += OnMqttPropertyChanged;
    }

    public void OnNavigatedFrom()
    {
        _mqttManager.PropertyChanged -= OnMqttPropertyChanged;
    }
    
    private void OnMqttPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        ParseSourcePropertyChanged(e);
    }
}