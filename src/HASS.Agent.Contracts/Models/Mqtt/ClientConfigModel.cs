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

	[JsonProperty("device")]
	public HomeAssistantDeviceDiscoveryModel DiscoveryModel { get; set; } = new();

	[JsonProperty("apis")]
	public ClientFeaturesModel? Features { get; set; }

	public override string ToString()
	{
		return JsonConvert.SerializeObject(this);
	}

	public override bool Equals(object? obj)
	{
		if (obj is not ClientConfigModel clientConfig)
		{
			return false;
		}

		if (SerialNumber != clientConfig.SerialNumber)
		{
			return false;
		}

		return ToString() == clientConfig.ToString();
	}

	public override int GetHashCode()
	{
		return ToString().GetHashCode();
	}
}