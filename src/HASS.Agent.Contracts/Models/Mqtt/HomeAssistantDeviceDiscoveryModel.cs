using Newtonsoft.Json;

namespace HASS.Agent.Contracts.Models.Mqtt;

/// <summary>
/// Class representing the HomeAssistant MQTT entity discovery device model
/// </summary>
public class HomeAssistantDeviceDiscoveryModel
{
	[JsonProperty("configuration_url")]
	public string ConfigurationUrl { get; set; } = string.Empty;

	[JsonProperty("connections")]
	public string Connections { get; set; } = string.Empty;

	[JsonProperty("hw_version")]
	public string HardwareVersion { get; set; } = string.Empty;

	[JsonProperty("identifiers")]
	public string Identifiers { get; set; } = string.Empty;

	[JsonProperty("manufacturer")]
	public string Manufacturer { get; set; } = string.Empty;

	[JsonProperty("model")]
	public string Model { get; set; } = string.Empty;

	[JsonProperty("model_id")]
	public string ModelId { get; set; } = string.Empty;

	[JsonProperty("name")]
	public string Name { get; set; } = string.Empty;

	[JsonProperty("serial_number")]
	public string SerialNumber { get; set; } = string.Empty;

	[JsonProperty("suggested_area")]
	public string SuggestedArea { get; set; } = string.Empty;

	[JsonProperty("sw_version")]
	public string SoftwareVersion { get; set; } = string.Empty;

	[JsonProperty("via_device")]
	public string ViaDevice { get; set; } = string.Empty;

	public override string ToString()
	{
		return JsonConvert.SerializeObject(this);
	}

	public override bool Equals(object? obj)
	{
		return ToString() == obj?.ToString();
	}

	public override int GetHashCode()
	{
		return ToString().GetHashCode();
	}
}