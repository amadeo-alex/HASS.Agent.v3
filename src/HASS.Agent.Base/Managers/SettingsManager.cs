using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.Base.Models;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models;
using HASS.Agent.Contracts.Models.Entity;
using HASS.Agent.Contracts.Models.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HASS.Agent.Base.Managers;

public class SettingsManager : ISettingsManager
{
    private const string RelativeConfigPath = "./config";

    private const string DefaultSettingsFileName = "appsettings.default.json";
    private const string UserSettingsFileName = "appsettings.user.json";
    private const string SensorsConfigurationFileName = "sensors.json";
    private const string CommandsConfigurationFileName = "commands.json";
    private const string QuickActionsConfigurationFileName = "quickactions.json";

    private const string DefaultSettingsFilePath = $"./{DefaultSettingsFileName}";
    private const string UserSettingsFilePath = $"{RelativeConfigPath}/{UserSettingsFileName}";
    private const string SensorsConfigurationFilePath = $"{RelativeConfigPath}/{SensorsConfigurationFileName}";
    private const string CommandsConfigurationFilePath = $"{RelativeConfigPath}/{CommandsConfigurationFileName}";
    private const string QuickActionsConfigurationFilePath = $"{RelativeConfigPath}/{QuickActionsConfigurationFileName}";
    
    private readonly ILogger _logger;
    private readonly IGuidManager _guidManager;

    private readonly JObject _settingsObject;

    public ObservableCollection<ConfiguredEntity> ConfiguredSensors { get; private set; }
    public ObservableCollection<ConfiguredEntity> ConfiguredCommands { get; private set; }
    public ObservableCollection<IQuickAction> ConfiguredQuickActions { get; private set; }

    public SettingsManager(ILogger<SettingsManager> logger, IGuidManager guidManager)
    {
        _logger = logger;
        _guidManager = guidManager;

        if (!Directory.Exists(RelativeConfigPath))
        {
            _logger.LogDebug("[SETTINGS] Creating initial config directory: {path}", Path.GetFullPath(RelativeConfigPath));
            Directory.CreateDirectory(RelativeConfigPath);
        }

        _settingsObject = GetSettingsObject();

        ConfiguredSensors = GetConfiguredSensors();
        ConfiguredCommands = GetConfiguredCommands();
        ConfiguredQuickActions = GetConfiguredQuickActions();

        foreach (var configuredSensor in ConfiguredSensors)
        {
            _guidManager.MarkAsUsed(configuredSensor.UniqueId);
        }

        foreach (var configuredCommand in ConfiguredCommands)
        {
            _guidManager.MarkAsUsed(configuredCommand.UniqueId);
        }

        foreach (var configuredQuickAction in ConfiguredQuickActions)
        {
            _guidManager.MarkAsUsed(configuredQuickAction.UniqueId);
        }

        ConfiguredSensors.CollectionChanged += Configured_CollectionChanged;
        ConfiguredCommands.CollectionChanged += Configured_CollectionChanged;
        ConfiguredQuickActions.CollectionChanged += Configured_CollectionChanged;
    }

    private void Configured_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems == null)
                {
                    return;
                }

                foreach (var configured in e.NewItems)
                {
                    switch (configured)
                    {
                        case ConfiguredEntity configuredEntity:
                            _guidManager.MarkAsUsed(configuredEntity.UniqueId);
                            break;
                        case IQuickAction configuredQuickAction:
                            _guidManager.MarkAsUsed(configuredQuickAction.UniqueId);
                            break;
                    }
                }

                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems == null)
                {
                    return;
                }

                foreach (var configured in e.OldItems)
                {
                    switch (configured)
                    {
                        case ConfiguredEntity configuredEntity:
                            _guidManager.MarkAsUnused(configuredEntity.UniqueId);
                            break;
                        case IQuickAction configuredQuickAction:
                            _guidManager.MarkAsUnused(configuredQuickAction.UniqueId);
                            break;
                    }
                }

                break;
        }
    }

    private ObservableCollection<IQuickAction> GetConfiguredQuickActions()
    {
        _logger.LogDebug("[SETTINGS] Loading quick action configuration");

        var configuredQuickActions = new ObservableCollection<IQuickAction>();

        try
        {
            if (File.Exists(QuickActionsConfigurationFilePath))
            {
                _logger.LogDebug("[SETTINGS] Configuration file found, loading");

                var quickActionsConfigurationJson = File.ReadAllText(QuickActionsConfigurationFilePath);
                var quickActionConfiguration = JsonConvert.DeserializeObject<ObservableCollection<IQuickAction>>(quickActionsConfigurationJson);
                if (quickActionConfiguration == null)
                {
                    _logger.LogWarning("[SETTINGS] Configuration file cannot be parsed");
                    configuredQuickActions = [];
                }
                else
                {
                    _logger.LogInformation("[SETTINGS] Quick actions configuration loaded");
                    configuredQuickActions = quickActionConfiguration;
                }
            }
            else
            {
                _logger.LogDebug("[SETTINGS] Commands configuration not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical("[SETTINGS] Exception loading quick actions configuration: {ex}", ex);
            throw;
        }

        return configuredQuickActions;
    }

    private ObservableCollection<ConfiguredEntity> GetConfiguredCommands()
    {
        _logger.LogDebug("[SETTINGS] Loading commands configuration");

        var configuredCommands = new ObservableCollection<ConfiguredEntity>();

        try
        {
            if (File.Exists(CommandsConfigurationFilePath))
            {
                _logger.LogDebug("[SETTINGS] Configuration file found, loading");

                var commandsConfigurationJson = File.ReadAllText(CommandsConfigurationFilePath);
                var commandsConfiguration = JsonConvert.DeserializeObject<ObservableCollection<ConfiguredEntity>>(commandsConfigurationJson);
                if (commandsConfiguration == null)
                {
                    _logger.LogWarning("[SETTINGS] Configuration file cannot be parsed");
                    configuredCommands = [];
                }
                else
                {
                    _logger.LogInformation("[SETTINGS] Commands configuration loaded");
                    configuredCommands = commandsConfiguration;
                }
            }
            else
            {
                _logger.LogDebug("[SETTINGS] Commands configuration not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical("[SETTINGS] Exception loading commands configuration: {ex}", ex);
            throw;
        }

        return configuredCommands;
    }

    private ObservableCollection<ConfiguredEntity> GetConfiguredSensors()
    {
        _logger.LogDebug("[SETTINGS] Loading sensor configuration");

        var configuredCommands = new ObservableCollection<ConfiguredEntity>();

        try
        {
            if (File.Exists(SensorsConfigurationFilePath))
            {
                _logger.LogDebug("[SETTINGS] Configuration file found, loading");

                var sensorsConfigurationJson = File.ReadAllText(SensorsConfigurationFilePath);
                var sensorConfiguration = JsonConvert.DeserializeObject<ObservableCollection<ConfiguredEntity>>(sensorsConfigurationJson);
                if (sensorConfiguration == null)
                {
                    _logger.LogWarning("[SETTINGS] Configuration file cannot be parsed");
                    configuredCommands = [];
                }
                else
                {
                    _logger.LogInformation("[SETTINGS] Sensors configuration loaded");
                    configuredCommands = sensorConfiguration;
                }
            }
            else
            {
                _logger.LogDebug("[SETTINGS] Sensors configuration not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical("[SETTINGS] Exception loading sensor configuration: {ex}", ex);
            throw;
        }

        return configuredCommands;
    }

    private JObject GetSettingsObject()
    {
        _logger.LogDebug("[SETTINGS] Loading settings");

        try
        {
            if (!File.Exists(UserSettingsFilePath))
            {
                _logger.LogInformation("[SETTINGS] User settings file not present, creating default");
                File.Copy(DefaultSettingsFileName, UserSettingsFilePath);
            }

            var defaultSettingsJsonContent = File.ReadAllText(DefaultSettingsFilePath); //TODO(Amadeo): add safety checks
            var userSettingsJsonContent = File.ReadAllText(UserSettingsFilePath);

            var defaultSettings = JObject.Parse(defaultSettingsJsonContent);
            var userSettings = JObject.Parse(userSettingsJsonContent);

            defaultSettings.Merge(userSettings, new JsonMergeSettings()
            {
                MergeArrayHandling = MergeArrayHandling.Replace,
            });

            return defaultSettings;
        }
        catch (Exception ex)
        {
            _logger.LogCritical("[SETTINGS] Exception loading settings: {ex}", ex);
            throw;
        }
    }

    public T GetSettingsSnapshot<T>() where T : new()
    {
        var settingsSubsection = _settingsObject[typeof(T).Name];
        if (settingsSubsection == null)
        {
            _logger.LogCritical("[SETTINGS] Settings section '{section}' missing", typeof(T).Name);
            return new T();
        }

        var sectionObject = settingsSubsection.ToObject<T>();
        if (sectionObject == null)
        {
            _logger.LogCritical("[SETTINGS] Settings section '{section}' cannot be deserialized", typeof(T).Name);
            return new T();
        }

        return (T)sectionObject;
    }
    
    public bool SaveConfiguredSensors()
    {
        _logger.LogDebug("[SETTINGS] Saving configured sensors to configuration file");

        try
        {
            var configuredSensorsJson = JsonConvert.SerializeObject(ConfiguredSensors, Formatting.Indented);
            File.WriteAllText(SensorsConfigurationFilePath, configuredSensorsJson);

            _logger.LogInformation("[SETTINGS] Sensor configuration saved");
        }
        catch (Exception ex)
        {
            _logger.LogCritical("[SETTINGS] Exception saving sensor configuration: {ex}", ex);
            return false;
        }

        return true;
    }
    
    public bool SaveConfiguredCommands()
    {
        _logger.LogDebug("[SETTINGS] Saving configured commands to configuration file");

        try
        {
            var configuredCommandsJson = JsonConvert.SerializeObject(ConfiguredCommands, Formatting.Indented);
            File.WriteAllText(CommandsConfigurationFilePath, configuredCommandsJson);

            _logger.LogInformation("[SETTINGS] Commands configuration saved");
        }
        catch (Exception ex)
        {
            _logger.LogCritical("[SETTINGS] Exception saving commands configuration: {ex}", ex);
            return false;
        }

        return true;
    }
    
    public bool SaveConfiguredQuickActions()
    {
        _logger.LogDebug("[SETTINGS] Saving configured quick actions to configuration file");

        try
        {
            var configuredQuickActionsJson = JsonConvert.SerializeObject(ConfiguredQuickActions, Formatting.Indented);
            File.WriteAllText(QuickActionsConfigurationFilePath, configuredQuickActionsJson);

            _logger.LogInformation("[SETTINGS] Quick actions configuration saved");
        }
        catch (Exception ex)
        {
            _logger.LogCritical("[SETTINGS] Exception saving quick actions configuration: {ex}", ex);
            return false;
        }

        return true;
    }

    public bool SaveSettings<T>(T settings) where T : notnull, new()
    {
        _logger.LogDebug("[SETTINGS] Saving settings for section {section}", typeof(T).Name);

        try
        {
            if (!_settingsObject.ContainsKey(typeof(T).Name))
            {
                _logger.LogWarning("[SETTINGS] Settings section '{section}' was not present before", typeof(T).Name);
            }
            
            _settingsObject[typeof(T).Name] = JToken.FromObject(settings);
            
            var serializedSettingsObject = JsonConvert.SerializeObject(_settingsObject, Formatting.Indented);
            File.WriteAllText(UserSettingsFilePath, serializedSettingsObject);
            
            _logger.LogInformation("[SETTINGS] Settings for section {section} saved", typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogCritical("[SETTINGS] Exception saving setting for section: {ex}", ex);
            return false;
        }

        return true;
    }
}