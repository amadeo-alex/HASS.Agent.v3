using Newtonsoft.Json;

namespace HASS.Agent.Contracts.Models.Mqtt;

public class ClientFeaturesModel
{
	[JsonProperty("media_player")]
	public bool MediaPlayerEnabled { get; set; } = true;
	
	[JsonProperty("notifications")]
	public bool NotificationsEnabled { get; set; } = true;
}