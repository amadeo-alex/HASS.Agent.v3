using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.Contracts.Models.Mqtt;
using HASS.Agent.Contracts.Enums;
using MQTTnet;

namespace HASS.Agent.Contracts.Managers;

public interface IMqttMessageHandler
{
    Task HandleMqttMessage(MqttApplicationMessage message);
}

public interface IMqttManager : INotifyPropertyChanged
{
    ManagerStatus Status { get; }
    
    HomeAssistantDeviceDiscoveryModel? DeviceConfigModel { get; }

    void RegisterMessageHandler(string topic, IMqttMessageHandler handler);
    void UnregisterMessageHandler(string topic);
    Task StartClientAsync();
    Task PublishAsync(MqttApplicationMessage message);
    Task AnnounceDeviceConfigModelAsync();
    Task ClearDeviceConfigModelAsync();
    Task StopClientAsync();
	Task RestartClientAsync();
}
