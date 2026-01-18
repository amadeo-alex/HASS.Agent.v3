using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Base.Models;
using static System.Net.Mime.MediaTypeNames;

namespace HASS.Agent.Base.Managers;
public class VariableManager : IVariableManager
{
    public string RootRegKey { get; } = @"HKEY_CURRENT_USER\SOFTWARE\HASSAgent\Client";
    public string CertificateHash { get; } = "F5E2C6F0BB7C78E82BFFE33ABC98A689BE690D10";
    
    public string CachePath { get; }
    public string ImageCachePath { get; }
    public string AudioCachePath { get; }
    public string WebViewCachePath { get; }
    public string LogPath { get; }
    public string ConfigPath { get; }
    public string ApplicationSettingsFile { get; }
    public string QuickActionsFile { get; }
    public string CommandsFile { get; }
    public string SensorsFile { get; }

    public VariableManager(ApplicationInfo applicationInfo)
    {
        CachePath = Path.Combine(applicationInfo.StartupPath, "cache");
        ImageCachePath = Path.Combine(CachePath, "images");
        AudioCachePath = Path.Combine(CachePath, "audio");
        WebViewCachePath = Path.Combine(CachePath, "webview");
        LogPath = Path.Combine(applicationInfo.StartupPath, "logs");
        ConfigPath = Path.Combine(applicationInfo.StartupPath, "config");

        ApplicationSettingsFile = "userappsettings.json";
        QuickActionsFile = "quickactions.json";
        CommandsFile = "commands.json";
        SensorsFile = "sensors.json";
    }
}
