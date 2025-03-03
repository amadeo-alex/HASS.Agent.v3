using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace HASS.Agent.Contracts.Models.Settings;
public class Settings : ISettings
{
    private readonly ILogger _logger;

    public ApplicationSettings Application { get; }
    public HomeAssistantSettings HomeAssistant { get; }
    public MqttSettings Mqtt { get; }
    public NotificationSettings Notification { get; }
    public StorageCacheSettings StorageCache { get; }
    public TraySettings Tray { get; }
    public UpdateSettings Update { get; }
    public WebViewSettings WebView { get; }

    public Settings(ILogger log, IVariableManager variableManager)
    {
        _logger = log;

        Application = GetSettings<ApplicationSettings>(variableManager.ApplicationSettingsFile, "Application");
        HomeAssistant = GetSettings<HomeAssistantSettings>(variableManager.HomeAssistantSettingsFile, "HomeAssistant");
        Notification = GetSettings<NotificationSettings>(variableManager.NotificationSettingsFile, "Notification");
        Mqtt = GetSettings<MqttSettings>(variableManager.MqttSettingsSettingsFile, "MQTT");
        StorageCache = GetSettings<StorageCacheSettings>(variableManager.StorageCacheSettingsFile, "StorageCache");
        Tray = GetSettings<TraySettings>(variableManager.TraySettingsFile, "Tray");
        Update = GetSettings<UpdateSettings>(variableManager.UpdateSettingsFile, "Update");
        WebView = GetSettings<WebViewSettings>(variableManager.WebViewSettingsFile, "WebView");
    }

    private T GetSettings<T>(string configurationFilePath, string displayName) where T : new()
    {
        var settings = new T();

        if (File.Exists(configurationFilePath))
        {
            _logger.LogDebug("[SETTINGS] -{displayName}- configuration file found, loading", displayName);

            var settingsJson = File.ReadAllText(configurationFilePath);
            var loadedApplicationSettings = JsonConvert.DeserializeObject<T>(settingsJson);
            if (loadedApplicationSettings != null)
            {
                _logger.LogDebug("[SETTINGS] -{displayName}- settings loaded", displayName);
                settings = loadedApplicationSettings;
            }
            else
            {
                _logger.LogError("[SETTINGS] -{displayName}- file cannot be parsed, using default settings", displayName);
            }
        }
        else
        {
            _logger.LogDebug("[SETTINGS] -{displayName}- file does not exist, creating default one", displayName);
            var applicationSettingsJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(configurationFilePath, applicationSettingsJson);
        }

        return settings;
    }

    private bool StoreSettings(object settingsObject, string configurationFilePath, string displayName)
    {
        _logger.LogDebug("[SETTINGS] -{displayName}-  storing settings", displayName);

        var applicationSettingsJson = JsonConvert.SerializeObject(settingsObject, Formatting.Indented);
        File.WriteAllText(configurationFilePath, applicationSettingsJson);

        _logger.LogDebug("[SETTINGS] -{displayName}- stored", displayName);

        return true;
    }

    public bool Store(IVariableManager variableManager)
    {
        var result = true;

        result = result && StoreSettings(Application, variableManager.ApplicationSettingsFile, "Application");
        result = result && StoreSettings(HomeAssistant, variableManager.HomeAssistantSettingsFile, "HomeAssistant");
        result = result && StoreSettings(Notification, variableManager.NotificationSettingsFile, "Notification");
        result = result && StoreSettings(Mqtt, variableManager.MqttSettingsSettingsFile, "MQTT");
        result = result && StoreSettings(StorageCache, variableManager.StorageCacheSettingsFile, "StorageCache");
        result = result && StoreSettings(Tray, variableManager.TraySettingsFile, "Tray");
        result = result && StoreSettings(Update, variableManager.UpdateSettingsFile, "Update");
        result = result && StoreSettings(WebView, variableManager.WebViewSettingsFile, "WebView");

        return result;
    }
}
