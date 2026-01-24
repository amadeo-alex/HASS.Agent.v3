using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using HASS.Agent.Contracts.Models;
using HASS.Agent.Contracts.Enums;
using Serilog;

namespace HASS.Agent.Contracts.Models.Entity;

/// <summary>
/// Base for all single-value sensors.
/// </summary>
public abstract class AbstractSingleValueSensor : AbstractDiscoverable
{
    public abstract string DefaultEntityIdName { get; }

    protected AbstractSingleValueSensor(IServiceProvider serviceProvider, ConfiguredEntity configuredEntity) : base(configuredEntity)
    {

    }


}
