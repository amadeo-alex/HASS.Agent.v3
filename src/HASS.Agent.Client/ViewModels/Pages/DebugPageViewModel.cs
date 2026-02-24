using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HASS.Agent.Base.Models;
using HASS.Agent.Base.Models.Mqtt;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models;
using HASS.Agent.Contracts.Models.Mqtt;
using HASS.Agent.Contracts.Models.Settings;
using Microsoft.Extensions.Logging;
using MQTTnet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HASS.Agent.Client.ViewModels.Pages;

public partial class DebugPageViewModel : ViewModelBase, INavigationAware
{
    private readonly ILogger _logger;
    private readonly IDiscoveryManager _discoveryManager;

    public ObservableCollection<ClientConfigModel> DiscoveredDevices => _discoveryManager.NearbyDevices;

    public DebugPageViewModel(ILogger<DebugPageViewModel> logger, IDiscoveryManager discoveryManager)
    {
        _logger = logger;
        _discoveryManager = discoveryManager;
    }
    
    private void NearbyDevicesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RaiseOnPropertyChanged(nameof(DiscoveredDevices));
    }

    public void OnNavigatedTo()
    {
        _discoveryManager.NearbyDevices.CollectionChanged += NearbyDevicesOnCollectionChanged;
    }

    public void OnNavigatedFrom()
    {
        _discoveryManager.NearbyDevices.CollectionChanged -= NearbyDevicesOnCollectionChanged;
    }
}