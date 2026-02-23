using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models.Mqtt;
using Microsoft.Extensions.Logging;
using MQTTnet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HASS.Agent.Base.Managers;

public partial class DiscoveryManager : ObservableObject, IDiscoveryManager, IMqttMessageHandler
{
    private readonly ILogger<DiscoveryManager> _logger;
    private readonly IMqttManager _mqttManager;

    //TODO(Amadeo): do it smarter instead of double reference
    public Dictionary<string, ClientConfigModel> _nearbyDevicesTopicMap { get; } = new();
    public ObservableCollection<ClientConfigModel> NearbyDevices { get; set; } = [];

    public ClientConfigModel ConfigModel { get; set; }

    public DiscoveryManager(ILogger<DiscoveryManager> logger, IMqttManager mqttManager)
    {
        _logger = logger;
        _mqttManager = mqttManager;
        
        _mqttManager.RegisterMessageHandler("hass.agent/devices/#", this); //TODO(Amadeo): change with project rename
    }

    public async Task ClearNearbyDevice(ClientConfigModel config)
    {
        throw new NotImplementedException();
    }

    public async Task HandleMqttMessage(MqttApplicationMessage message)
    {
        if (message.Payload.Length == 0)
        {
            if (_nearbyDevicesTopicMap.Remove(message.Topic, out var deviceConfig))
            {
                NearbyDevices.Remove(deviceConfig);
            }

            return;
        }
        
        var payload = Encoding.UTF8.GetString(message.Payload);
        var discoveryModel = JsonConvert.DeserializeObject<ClientConfigModel>(payload);
        if (discoveryModel == null)
        {
            return;
        }
        
        if (!NearbyDevices.Contains(discoveryModel))
        {
            _nearbyDevicesTopicMap[message.Topic] = discoveryModel;
            NearbyDevices.Add(discoveryModel);
        }
    }
}