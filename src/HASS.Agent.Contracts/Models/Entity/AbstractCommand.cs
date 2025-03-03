using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using HASS.Agent.Base.Models;
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

    protected readonly ILogger _logger;

    public abstract string DefaultEntityIdName { get; }

    public string ConfiguredAction => _configuration.GetParameter(_configuredActionParam);

    protected AbstractCommand(ILogger logger, IServiceProvider serviceProvider, ConfiguredEntity configuredEntity) : base(configuredEntity)
    {
        _logger = logger;

        UniqueId = configuredEntity.UniqueId.ToString();
        EntityIdName = configuredEntity.EntityIdName;
        Name = configuredEntity.Name;
        UpdateIntervalSeconds = configuredEntity.UpdateIntervalSeconds;
        Domain = HassDomain.Button.ToString().ToLower();
        UseAttributes = configuredEntity.UseAttributes;
    }

    public abstract Task TurnOn();
    public abstract Task TurnOn(string action);
    public abstract Task TurnOff();

    public override void ResetChecks()
    {
        LastUpdated = DateTime.MinValue;

        PreviousPublishedState = string.Empty;
        PreviousPublishedAttributes = string.Empty;
    }
}
