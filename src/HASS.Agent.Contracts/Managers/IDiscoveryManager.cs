using HASS.Agent.Contracts.Models.Mqtt;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace HASS.Agent.Contracts.Managers;

public interface IDiscoveryManager : INotifyPropertyChanged
{
	ObservableCollection<ClientConfigModel> NearbyDevices { get; set; }
	
	Task ClearNearbyDevice(ClientConfigModel config);
}