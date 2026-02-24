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
using CommunityToolkit.Mvvm.Input;
using HASS.Agent.Base.Models;
using HASS.Agent.Base.Models.Mqtt;
using HASS.Agent.Client.ViewModels.Dialogs;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models;
using HASS.Agent.Contracts.Models.Mqtt;
using HASS.Agent.Contracts.Models.Settings;
using HASS.Agent.Contracts.Services;
using Microsoft.Extensions.Logging;
using MQTTnet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HASS.Agent.Client.ViewModels.Pages;

public partial class DebugPageViewModel : ViewModelBase, INavigationAware
{
    private readonly ILogger _logger;
    private readonly IDiscoveryManager _discoveryManager;
    private readonly IDialogService _dialogService;

    public ObservableCollection<ClientConfigModelViewModel> DiscoveredDevices { get; set; } = [];

    public List<AnchorItemViewModel> DebugSections { get; }

    public DebugPageViewModel(ILogger<DebugPageViewModel> logger, IDiscoveryManager discoveryManager, IDialogService dialogService)
    {
        _logger = logger;
        _discoveryManager = discoveryManager;
        _dialogService = dialogService;

        DebugSections =
        [
            new AnchorItemViewModel() { Header = Translations.Strings.DebugNearbyDevicesSection.CurrentValue },
        ];
    }

    [RelayCommand]
    private async Task Details(ClientConfigModelViewModel clientConfig)
    {
        var dialogViewModel = new ConfirmDialogViewModel()
        {
            Title = "Nearby device details",
            Query = JsonConvert.SerializeObject(clientConfig, Formatting.Indented)
        };

        await _dialogService.ShowDialogAsync(dialogViewModel);
    }

    [RelayCommand]
    private async Task Delete(ClientConfigModelViewModel clientConfig)
    {
    }

    private void NearbyDevicesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems == null)
                {
                    return;
                }

                foreach (ClientConfigModel newItem in e.NewItems)
                {
                    DiscoveredDevices.Add(new ClientConfigModelViewModel()
                    {
                        Config = newItem,
                    });
                }
                
                MarkDuplicateNearbyDevices();

                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems == null)
                {
                    return;
                }

                foreach (ClientConfigModel oldItem in e.OldItems)
                {
                    var viewModel = DiscoveredDevices.FirstOrDefault(vm => vm.Config == oldItem);
                    if (viewModel != null)
                    {
                        DiscoveredDevices.Remove(viewModel);
                    }
                }
                
                MarkDuplicateNearbyDevices();

                break;
                
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
            case NotifyCollectionChangedAction.Reset:
            default:
                break;
        }
    }

    private void MarkDuplicateNearbyDevices()
    {
        var grouped = DiscoveredDevices.GroupBy(item => item.Config.SerialNumber);
        foreach (var group in grouped)
        {
            if (group.Count() == 1)
            {
                continue;
            }

            foreach (var viewModel in group)
            {
                viewModel.Duplicate = true;
            }
        }
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