using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HASS.Agent.Contracts.Enums;
using HASS.Agent.Contracts.Models.Mqtt;

namespace HASS.Agent.Contracts.Models.Entity;
public abstract partial class AbstractDiscoverable : IDiscoverable
{
    protected readonly ConfiguredEntity _configuration;

    public EntityCategory? Category { get; set; }
    public HassDomain Domain { get; set; }
    public string EntityIdName { get; set; }
    public string Name { get; set; } 
    public string TopicName { get; set; } = string.Empty;
    public string UniqueId { get; set; }
    public bool UseAttributes { get; set; } = false;
    public bool IgnoreAvailability { get; set; }
    public int UpdateIntervalSeconds { get; set; }
    public bool Active { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.MinValue;
    public string PreviousPublishedState { get; set; } = string.Empty;
    public string PreviousPublishedAttributes { get; set; } = string.Empty;

    public abstract Task<string> GetState();
    public async virtual Task<string> GetAttributes() { return string.Empty; }

    protected AbstractDiscoverable(ConfiguredEntity configuredEntity) //TODO(Amadeo): order properties in two sections, ones from ConfiguredEntity and others
    {
        _configuration = configuredEntity;

        UniqueId = configuredEntity.UniqueId.ToString();
        EntityIdName = configuredEntity.EntityIdName;
        Name = configuredEntity.Name;
        UpdateIntervalSeconds = configuredEntity.UpdateIntervalSeconds;
        UseAttributes = configuredEntity.UseAttributes;
        Active = configuredEntity.Active;
        IgnoreAvailability = configuredEntity.IgnoreAvailability;
        
        var parsed = Enum.TryParse<HassDomain>(configuredEntity.Domain, true, out var domain);
        Domain = parsed ? domain : throw new InvalidOperationException($"cannot convert {configuredEntity.Domain} to HassDomain");
    }

    public abstract AbstractMqttDiscoveryConfigModel ConfigureAutoDiscoveryConfig(string discoveryPrefix, AbstractMqttDeviceConfigModel deviceConfigModel);
    public abstract AbstractMqttDiscoveryConfigModel? GetAutoDiscoveryConfig();
    //public abstract void ClearAutoDiscoveryConfig();
    public abstract ConfiguredEntity ToConfiguredEntity();
    
    public void ResetChecks()
    {
	    LastUpdated = DateTime.MinValue;

	    PreviousPublishedState = string.Empty;
	    PreviousPublishedAttributes = string.Empty;
    }
}
