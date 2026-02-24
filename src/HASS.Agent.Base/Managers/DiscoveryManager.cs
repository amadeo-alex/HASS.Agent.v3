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
    private readonly Dictionary<string, ClientConfigModel> _nearbyDevicesTopicMap = new();
    
    public ObservableCollection<ClientConfigModel> NearbyDevices { get; set; } = [];

    public DiscoveryManager(ILogger<DiscoveryManager> logger, IMqttManager mqttManager)
    {
        _logger = logger;
        _mqttManager = mqttManager;

        _mqttManager.RegisterMessageHandler("hass.agent/devices/#", this); //TODO(Amadeo): change with project rename
    }

    public async Task ClearNearbyDevice(ClientConfigModel config)
    {
        var discoveredEntry = _nearbyDevicesTopicMap.FirstOrDefault(x => x.Value == config);
        if (discoveredEntry.Key == null)
        {
            _logger.LogWarning("[DISCOVERY] Cannot remove device '{name}' which is not discovered", config.DiscoveryModel.Name);
            return;
        }

        var clearMessage = new MqttApplicationMessageBuilder()
            .WithTopic(discoveredEntry.Key)
            .WithPayload([])
            .WithRetainFlag()
            .Build();

        await _mqttManager.PublishAsync(clearMessage);
    }

    public async Task HandleMqttMessage(MqttApplicationMessage message)
    {
        if (message.Payload.Length == 0)
        {
            if (!_nearbyDevicesTopicMap.Remove(message.Topic, out var removedDeviceConfig))
            {
                return;
            }

            _logger.LogDebug("[DISCOVERY] Discovered nearby device disappeared: {name}", removedDeviceConfig.DiscoveryModel.Name);
            NearbyDevices.Remove(removedDeviceConfig);

            return;
        }

        var payload = Encoding.UTF8.GetString(message.Payload);
        var deviceConfig = JsonConvert.DeserializeObject<ClientConfigModel>(payload);
        if (deviceConfig == null)
        {
            return;
        }
        
        if (!NearbyDevices.Contains(deviceConfig))
        {
            _nearbyDevicesTopicMap[message.Topic] = deviceConfig;
            NearbyDevices.Add(deviceConfig);

            _logger.LogDebug("[DISCOVERY] Discovered nearby device added: {name}", deviceConfig.DiscoveryModel.Name);
        }
    }
}