using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models.Entity;
using HASS.Agent.Contracts.Models.Mqtt;
using HASS.Agent.Base.Models;
using HASS.Agent.Base.Models.Mqtt;
using Microsoft.Extensions.DependencyInjection;
using static System.Runtime.InteropServices.JavaScript.JSType;
using HASS.Agent.Contracts.Enums;
using Microsoft.Extensions.Logging;

namespace HASS.Agent.Base.Commands;
public class DummySwitch : AbstractCommand
{

    public override string DefaultEntityIdName { get; } = "dummyCommand";

    private MqttCommandDiscoveryConfigModel? _discoveryConfigModel;
    private string _state = StateOff;

    public DummySwitch(ILogger<DummySwitch> logger, IServiceProvider serviceProvider, ConfiguredEntity configuredSensor) : base(logger, serviceProvider, configuredSensor)
    {
        Domain = HassDomain.Switch.ToString().ToLower();
        var mqtt = serviceProvider.GetService<IMqttManager>();
    }

    public override AbstractMqttDiscoveryConfigModel ConfigureAutoDiscoveryConfig(string discoveryPrefix, AbstractMqttDeviceConfigModel deviceConfigModel)
    {
        _discoveryConfigModel = new MqttCommandDiscoveryConfigModel()
        {
            Name = Name,
            UniqueId = UniqueId,
            ObjectId = $"{deviceConfigModel.Name}_{EntityIdName}",
            Device = deviceConfigModel,
            StateTopic = $"{discoveryPrefix}/{Domain}/{deviceConfigModel.Name}/{EntityIdName}/state",
            AvailabilityTopic = $"{discoveryPrefix}/hass.agent/{deviceConfigModel.Name}/availability",
            CommandTopic = $"{discoveryPrefix}/{Domain}/{deviceConfigModel.Name}/{EntityIdName}/set",
            ActionTopic = $"{discoveryPrefix}/{Domain}/{deviceConfigModel.Name}/{EntityIdName}/action"
        };

        return _discoveryConfigModel;
    }

    public override MqttCommandDiscoveryConfigModel? GetAutoDiscoveryConfig() => _discoveryConfigModel;

    public async override Task<string> GetState() { return _state; }

    public async override Task TurnOn()
    {
        _logger.LogDebug("[{name}] turned on");
        _state = StateOn;
    }
    public async override Task TurnOn(string action)
    {
        _logger.LogDebug("[{name}] turned with action");
        _state = action;
    }
    public async override Task TurnOff()
    {
        _logger.LogDebug("[{name}] turned off");
        _state = StateOff;
    }

    public override ConfiguredEntity ToConfiguredEntity()
    {
        var configuredCommand = new ConfiguredEntity()
        {
            Type = typeof(DummySwitch).Name,
            EntityIdName = EntityIdName,
            Name = Name,
            UpdateIntervalSeconds = UpdateIntervalSeconds,
            UniqueId = Guid.Parse(UniqueId),
            Active = Active
        };

        configuredCommand.SetParameter("someSecretValueCMD", "secretParameterCMD");

        return configuredCommand;
    }
}
