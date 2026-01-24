using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using HASS.Agent.Contracts.Enums;
using Microsoft.Extensions.Logging;

namespace HASS.Agent.Contracts.Models.Entity;

/// <summary>
/// Base for all commands.
/// </summary>
public abstract class AbstractCommand : AbstractDiscoverable
{
    private const string _configuredActionParam = "configuredAction";

    public const string StateOn = "ON";
    public const string StateOff = "OFF";

    public abstract string DefaultEntityIdName { get; }

    public string ConfiguredAction => _configuration.GetParameter(_configuredActionParam);

    protected AbstractCommand(IServiceProvider serviceProvider, ConfiguredEntity configuredEntity) : base(configuredEntity)
    {

    }

    public abstract Task TurnOn();
    public abstract Task TurnOn(string action);
    public abstract Task TurnOff();
}
