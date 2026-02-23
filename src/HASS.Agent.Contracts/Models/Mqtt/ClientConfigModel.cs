using Newtonsoft.Json;

namespace HASS.Agent.Contracts.Models.Mqtt;

//TODO(Amadeo): consider name change after general program name

/// <summary>
/// Class representing the agent's device config model used with the integration
/// </summary>
public class ClientConfigModel
{
	[JsonProperty("serial_number")]
	public string SerialNumber { get; set; } = string.Empty;

	[JsonProperty("discovery_model")]
	public HomeAssistantDeviceDiscoveryModel? DiscoveryModel { get; set; }

	[JsonProperty("features")]
	public ClientFeaturesModel? Features { get; set; }
}