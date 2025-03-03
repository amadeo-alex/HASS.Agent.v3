using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using HASS.Agent.Contracts.Models;
using HASS.Agent.Base.Models;
using HASS.Agent.Contracts.Enums;

namespace HASS.Agent.Contracts.Models.Entity;

/// <summary>
/// Base for all single-value sensors.
/// </summary>
public abstract class AbstractSingleValueSensor : AbstractDiscoverable
{
    public abstract string DefaultEntityIdName { get; }

    /*    protected AbstractSingleValueSensor(string entityIdName, string name, int updateIntervalSeconds, string uniqueId, bool useAttributes)
        {
            UniqueId = uniqueId;
            EntityIdName = entityIdName;
            Name = name;
            UpdateIntervalSeconds = updateIntervalSeconds;
            Domain = HassDomain.Sensor.ToString();
            UseAttributes = useAttributes;
        }*/

    protected AbstractSingleValueSensor(IServiceProvider serviceProvider, ConfiguredEntity configuredEntity) : base(configuredEntity)
    {
        UniqueId = configuredEntity.UniqueId.ToString();
        EntityIdName = configuredEntity.EntityIdName;
        Name = configuredEntity.Name;
        UpdateIntervalSeconds = configuredEntity.UpdateIntervalSeconds;
        Domain = HassDomain.Sensor.ToString().ToLower();
        UseAttributes = configuredEntity.UseAttributes;
    }

    public override void ResetChecks()
    {
        LastUpdated = DateTime.MinValue;

        PreviousPublishedState = string.Empty;
        PreviousPublishedAttributes = string.Empty;
    }
}
