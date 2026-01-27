using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.Base.Commands;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models.Entity;
using HASS.Agent.Base.Models;
using HASS.Agent.Base.Sensors.SingleValue;
using Microsoft.Extensions.DependencyInjection;

namespace HASS.Agent.Base.Managers;

public class EntityTypeRegistry : IEntityTypeRegistry
{
    private readonly IServiceProvider _serviceProvider;

    public Dictionary<string, RegisteredEntity> SensorTypes { get; } = [];
    public EntityCategory SensorsCategories { get; } = new EntityCategory("sensorRoot", null);
    public Dictionary<string, RegisteredEntity> CommandTypes { get; } = [];
    public EntityCategory CommandsCategories { get; } = new EntityCategory("commandRoot", null);

    public Dictionary<string, RegisteredEntity> ClientSensorTypes => SensorTypes.Where(st => st.Value.ClientCompatible)
        .ToDictionary(st => st.Key, st => st.Value);

    public Dictionary<string, RegisteredEntity> SatelliteSensorTypes => SensorTypes.Where(st => st.Value.SatelliteCompatible)
        .ToDictionary(st => st.Key, st => st.Value);

    public EntityTypeRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        RegisterSensorType(typeof(DummySensor), "Other/Debug/Dummy", true, true, new ConfiguredEntity()
        {
            Type = nameof(DummySensor),
            EntityIdName = "dummy_sensor",
            Name = "DummySensor",
            UpdateIntervalSeconds = 10
        });

        RegisterCommandType(typeof(DummySwitch), "Other/Debug/Dummy", true, true, new ConfiguredEntity()
        {
            Type = nameof(DummySwitch),
            EntityIdName = "dummy_switch",
            Name = "DummySwitch",
            UpdateIntervalSeconds = 10
        });
    }

    public void RegisterSensorType(Type sensorType, string categoryString, bool clientCompatible, bool satelliteCompatible,
        ConfiguredEntity? defaultConfiguration = null)
    {
        if (!sensorType.IsAssignableTo(typeof(IDiscoverable)))
        {
            throw new ArgumentException($"{sensorType} is not derived from {nameof(IDiscoverable)}");
        }

        var typeName = sensorType.Name;

        if (SensorTypes.ContainsKey(typeName))
        {
            throw new ArgumentException($"sensor {sensorType} already registered");
        }

        SensorsCategories.Add(categoryString, sensorType);

        SensorTypes[typeName] = new RegisteredEntity
        {
            EntityType = sensorType,
            ClientCompatible = clientCompatible,
            SatelliteCompatible = satelliteCompatible,
            DefaultConfiguration = defaultConfiguration
        };
    }

    public void RegisterCommandType(Type commandType, string categoryString, bool clientCompatible, bool satelliteCompatible,
        ConfiguredEntity? defaultConfiguration = null)
    {
        if (!commandType.IsAssignableTo(typeof(IDiscoverable)))
        {
            throw new ArgumentException($"{commandType} is not derived from {nameof(IDiscoverable)}");
        }

        var typeName = commandType.Name;

        if (CommandTypes.ContainsKey(typeName))
        {
            throw new ArgumentException($"command {commandType} already registered");
        }

        CommandsCategories.Add(categoryString, commandType);

        CommandTypes[typeName] = new RegisteredEntity
        {
            EntityType = commandType,
            ClientCompatible = clientCompatible,
            SatelliteCompatible = satelliteCompatible,
            DefaultConfiguration = defaultConfiguration
        };
    }

    private IDiscoverable CreateDiscoverableInstance(Type discoverableType, ConfiguredEntity configuredEntity)
    {
        var sensor = ActivatorUtilities.CreateInstance(_serviceProvider, discoverableType, configuredEntity);

        /*        var constructorMethod = discoverableType.GetConstructor([typeof(ConfiguredEntity)])
                    ?? throw new MethodAccessException($"type {discoverableType} is missing required constructor accepting ConfiguredEntity");

                var obj = constructorMethod.Invoke(new object[] { configuredEntity })
                    ?? throw new Exception($"{discoverableType} instance cannot be created");*/

        return (IDiscoverable)sensor;
    }

    public IDiscoverable CreateSensorInstance(ConfiguredEntity configuredEntity)
    {
        if (!SensorTypes.TryGetValue(configuredEntity.Type, out var registeredEntity))
        {
            throw new ArgumentException($"sensor type {configuredEntity.Type} is not registered");
        }

        return CreateDiscoverableInstance(registeredEntity.EntityType, configuredEntity);
    }

    public IDiscoverable CreateCommandInstance(ConfiguredEntity configuredEntity)
    {
        if (!CommandTypes.TryGetValue(configuredEntity.Type, out var registeredEntity))
        {
            throw new ArgumentException($"command type {configuredEntity.Type} is not registered");
        }

        return CreateDiscoverableInstance(registeredEntity.EntityType, configuredEntity);
    }

    public ConfiguredEntity? GetDefaultConfiguration(string entityTypeName)
    {
        RegisteredEntity? registeredEntity = null;

        if (SensorTypes.TryGetValue(entityTypeName, out var registeredSensor))
        {
            registeredEntity = registeredSensor;
        }
        else if (!CommandTypes.TryGetValue(entityTypeName, out var registeredCommand))
        {
            registeredEntity = registeredCommand;
        }

        return registeredEntity?.DefaultConfiguration?.Clone() as ConfiguredEntity;
    }
}