using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.Base.Models;
using HASS.Agent.Base.Models.Entity;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models;
using HASS.Agent.Contracts.Models.Entity;
using HASS.Agent.Contracts.Models.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace HASS.Agent.Base.Managers;

public class SettingsManager : ISettingsManager
{
    private readonly ILogger _logger;
    private readonly IVariableManager _variableManager;
    private readonly IGuidManager _guidManager;

    private readonly IConfiguration _configuration;

    public ObservableCollection<ConfiguredEntity> ConfiguredSensors { get; private set; }
    public ObservableCollection<ConfiguredEntity> ConfiguredCommands { get; private set; }
    public ObservableCollection<IQuickAction> ConfiguredQuickActions { get; private set; }

    public SettingsManager(ILogger<SettingsManager> logger, IVariableManager variableManager, IGuidManager guidManager)
    {
        _logger = logger;
        _variableManager = variableManager;
        _guidManager = guidManager;

        if (!Directory.Exists(_variableManager.ConfigPath))
        {
            _logger.LogDebug("[SETTINGS] Creating initial user config directory: {path}", _variableManager.ConfigPath);
            Directory.CreateDirectory(_variableManager.ConfigPath);
        }

        _configuration = new ConfigurationBuilder()
            .SetBasePath(_variableManager.StartupPath)
            .AddJsonFile("appsettings.json")
            .AddJsonFile("config/userappsettings.json", optional: true)
            .AddJsonFile("config/sensors.json")
            .AddJsonFile("config/commands.json")
            .AddJsonFile("config/quickactions.json")
            .Build();

        //Settings = GetSettings();
        ConfiguredSensors = new ObservableCollection<ConfiguredEntity>(GetConfiguredSensors());
        ConfiguredCommands = new ObservableCollection<ConfiguredEntity>(GetConfiguredCommands());
        ConfiguredQuickActions = new ObservableCollection<IQuickAction>(GetConfiguredQuickActions());

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

    public T GetSettings<T>() where T : new()
    {
        var sectionName = typeof(T).Name;
        return _configuration.GetSection(sectionName).Get<T>() ?? throw new InvalidOperationException($"no such settings exist: '{sectionName}'");
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
                    if (configured is ConfiguredEntity configuredEntity)
                    {
                        _guidManager.MarkAsUsed(configuredEntity.UniqueId);
                    }
                    else if (configured is IQuickAction configuredQuickAction)
                    {
                        _guidManager.MarkAsUsed(configuredQuickAction.UniqueId);
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
                    if (configured is ConfiguredEntity configuredEntity)
                    {
                        _guidManager.MarkAsUnused(configuredEntity.UniqueId);
                    }
                    else if (configured is IQuickAction configuredQuickAction)
                    {
                        _guidManager.MarkAsUnused(configuredQuickAction.UniqueId);
                    }
                }

                break;
        }
    }

    private List<QuickAction> GetConfiguredQuickActions()
    {
        _logger.LogDebug("[SETTINGS] Loading quick action configuration");

        var configuredQuickActions = _configuration.GetSection("QuickActions").Get<List<QuickAction>>() ?? [];

        if (configuredQuickActions?.Count > 0)
        {
            _logger.LogInformation("[SETTINGS] Quick actions configuration loaded");
        }
        else
        {
            _logger.LogDebug("[SETTINGS] Quick actions configuration not found");
        }

        return configuredQuickActions;
    }

    private List<ConfiguredEntity> GetConfiguredCommands()
    {
        _logger.LogDebug("[SETTINGS] Loading commands configuration");

        var configuredCommands = _configuration.GetSection("Commands").Get<List<ConfiguredEntity>>() ?? [];

        if (configuredCommands.Count > 0)
        {
            _logger.LogInformation("[SETTINGS] Commands configuration loaded");
        }
        else
        {
            _logger.LogDebug("[SETTINGS] Commands configuration not found");
        }

        return configuredCommands;
    }

    private List<ConfiguredEntity> GetConfiguredSensors()
    {
        _logger.LogDebug("[SETTINGS] Loading sensors configuration");

        var configuredSensors = _configuration.GetSection("Sensors").Get<List<ConfiguredEntity>>() ?? [];

        if (configuredSensors.Count > 0)
        {
            _logger.LogInformation("[SETTINGS] Sensors configuration loaded");
        }
        else
        {
            _logger.LogDebug("[SETTINGS] Sensors configuration not found");
        }

        return configuredSensors;
    }

    private Settings GetSettings()
    {
        _logger.LogDebug("[SETTINGS] Loading settings");

        try
        {
            return new Settings(_logger, _variableManager);
        }
        catch (Exception ex)
        {
            _logger.LogCritical("[SETTINGS] Exception loading application settings: {ex}", ex);
            throw;
        }
    }

    public bool StoreConfiguredEntities()
    {
        return StoreConfiguredSensors()
               && StoreConfiguredCommands()
               && StoreConfiguredQuickActions();
    }

    private bool StoreConfiguredQuickActions()
    {
        _logger.LogDebug("[SETTINGS] Storing configured quick actions");

        try
        {
            /*var configuredQuickActionsJson = JsonConvert.SerializeObject(ConfiguredQuickActions, Formatting.Indented);
            File.WriteAllText(_variableManager.QuickActionsFile, configuredQuickActionsJson);*/

            _logger.LogInformation("[SETTINGS] Quick actions configuration stored");
        }
        catch (Exception ex)
        {
            _logger.LogCritical("[SETTINGS] Exception storing quick actions configuration: {ex}", ex);
            return false;
        }

        return true;
    }

    private bool StoreConfiguredCommands()
    {
        _logger.LogDebug("[SETTINGS] Storing configured commands");

        try
        {
            /*var configuredCommandsJson = JsonConvert.SerializeObject(ConfiguredCommands, Formatting.Indented);
            File.WriteAllText(_variableManager.CommandsFile, configuredCommandsJson);*/

            _logger.LogInformation("[SETTINGS] Commands configuration stored");
        }
        catch (Exception ex)
        {
            _logger.LogCritical("[SETTINGS] Exception storing commands configuration: {ex}", ex);
            return false;
        }

        return true;
    }

    private bool StoreConfiguredSensors()
    {
        _logger.LogDebug("[SETTINGS] Storing configured sensors");

        try
        {
            /*var configuredSensorsJson = JsonConvert.SerializeObject(ConfiguredSensors, Formatting.Indented);
            File.WriteAllText(_variableManager.SensorsFile, configuredSensorsJson);*/

            _logger.LogInformation("[SETTINGS] Sensor configuration stored");
        }
        catch (Exception ex)
        {
            _logger.LogCritical("[SETTINGS] Exception storing sensor configuration: {ex}", ex);
            return false;
        }

        return true;
    }

    public bool StoreSettings()
    {
        _logger.LogDebug("[SETTINGS] Storing settings");

        try
        {
            //Settings.Store(_variableManager);

            _logger.LogInformation("[SETTINGS] Application settings stored");
        }
        catch (Exception ex)
        {
            _logger.LogCritical("[SETTINGS] Exception storing application settings: {ex}", ex);
            return false;
        }

        return true;
    }

    public void AddUpdateConfiguredCommand(ConfiguredEntity command)
    {
        var existingCommand = ConfiguredCommands.FirstOrDefault(c => c.UniqueId == command.UniqueId);
        if (existingCommand != null)
        {
            if (existingCommand.Type != command.Type)
            {
                throw new ArgumentException(
                    $"command with ID {existingCommand.UniqueId} of different type ({existingCommand.Type}) than {command.Type} already exists");
            }

            ConfiguredSensors.Remove(existingCommand);
        }

        ConfiguredCommands.Add(command);
    }

    public bool GetExtendedLoggingSetting()
    {
        try
        {
            var setting = Registry.GetValue(_variableManager.RootRegKey, "ExtendedLogging", "0") as string;
            if (string.IsNullOrEmpty(setting))
            {
                return false;
            }

            return setting == "1";
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[SETTINGS] Error retrieving extended logging setting: {ex}", ex);
            return false;
        }
    }

    public void SetExtendedLoggingSetting(bool enabled)
    {
        try
        {
            Registry.SetValue(_variableManager.RootRegKey, "ExtendedLogging", enabled ? "1" : "0", RegistryValueKind.String);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[SETTINGS] Error storing extended logging setting: {ex}", ex);
        }
    }

    //TODO(Amadeo): verify if necessary
    public bool GetDpiWarningShown()
    {
        try
        {
            var setting = Registry.GetValue(_variableManager.RootRegKey, "DpiWarningShown", "0") as string;
            if (string.IsNullOrEmpty(setting))
            {
                return false;
            }

            return setting == "1";
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[SETTINGS] Error retrieving DPI-warning-shown setting: {ex}", ex);
            return false;
        }
    }

    public void SetDpiWarningShown(bool shown)
    {
        try
        {
            Registry.SetValue(_variableManager.RootRegKey, "DpiWarningShown", shown ? "1" : "0", RegistryValueKind.String);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[SETTINGS] Error storing DPI-warning-shown setting: {ex}", ex);
        }
    }

    //TODO(Amadeo): remove
    /// <summary>
    /// Sends the current MQTT appsettings to the satellite service, optionally with a new client ID
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SendMqttSettingsToServiceAsync(bool sendNewClientId = false)
    {
        try
        {
            // create settings obj
            /*            var config = new ServiceMqttSettings
                        {
                            MqttAddress = Variables.AppSettings.MqttAddress,
                            MqttPort = Variables.AppSettings.MqttPort,
                            MqttUseTls = Variables.AppSettings.MqttUseTls,
                            MqttUsername = Variables.AppSettings.MqttUsername,
                            MqttPassword = Variables.AppSettings.MqttPassword,
                            MqttDiscoveryPrefix = Variables.AppSettings.MqttDiscoveryPrefix,
                            MqttClientId = sendNewClientId ? Guid.NewGuid().ToString()[..8] : string.Empty,
                            MqttRootCertificate = Variables.AppSettings.MqttRootCertificate,
                            MqttClientCertificate = Variables.AppSettings.MqttClientCertificate,
                            MqttAllowUntrustedCertificates = Variables.AppSettings.MqttAllowUntrustedCertificates,
                            MqttUseRetainFlag = Variables.AppSettings.MqttUseRetainFlag
                        };

                        // store
                        var (storedOk, _) = await Task.Run(async () => await Variables.RpcClient.SetServiceMqttSettingsAsync(config).WaitAsync(Variables.RpcConnectionTimeout));
                        if (!storedOk)
                        {
                            _logger.LogError("[SETTINGS] Sending MQTT settings to service failed");
                            return false;
                        }*/

            // done
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[SETTINGS] Error sending MQTT settings to service: {err}", ex.Message);
            return false;
        }
    }

    public string GetDeviceSerialNumber()
    {
        var serialNumber = string.Empty;
        try
        {
            serialNumber = Registry.GetValue(_variableManager.RootRegKey, "DeviceSerialNumber", string.Empty) as string;
            if (string.IsNullOrEmpty(serialNumber))
            {
                _logger.LogDebug("[SETTINGS] Generating new device serial number");
                serialNumber = Guid.NewGuid().ToString();
                SetDeviceSerialNumber(serialNumber);
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[SETTINGS] Error retrieving DPI-warning-shown setting: {err}", ex.Message);
        }

        return serialNumber ?? string.Empty;
    }

    public void SetDeviceSerialNumber(string deviceSerialNumber)
    {
        try
        {
            Registry.SetValue(_variableManager.RootRegKey, "DeviceSerialNumber", deviceSerialNumber, RegistryValueKind.String);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[SETTINGS] Error storing device serial number setting: {ex}", ex);
        }
    }

    public bool GetHideDonateButtonSetting()
    {
        try
        {
            var setting = Registry.GetValue(_variableManager.RootRegKey, "HideDonateButton", "0") as string;
            if (string.IsNullOrEmpty(setting))
            {
                return false;
            }

            return setting == "1";
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[SETTINGS] Error retrieving 'hide donate button from the main window' setting: {err}", ex.Message);
            return false;
        }
    }

    public void SetHideDonateButtonSetting(bool hide)
    {
        try
        {
            Registry.SetValue(_variableManager.RootRegKey, "HideDonateButton", hide ? "1" : "0", RegistryValueKind.String);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[SETTINGS] Error storing 'hide donate button from the main window' setting: {err}", ex.Message);
        }
    }
}