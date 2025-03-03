using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.Base.Models;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models;
using HASS.Agent.Contracts.Models.Settings;

namespace HASS.Agent.Contracts.Models;
public interface ISettings
{
    ApplicationSettings Application { get; }
    HomeAssistantSettings HomeAssistant { get; }
    MqttSettings Mqtt { get; }
    NotificationSettings Notification { get; }
    StorageCacheSettings StorageCache { get; }
    TraySettings Tray { get; }
    UpdateSettings Update { get; }
    WebViewSettings WebView { get; }

    bool Store(IVariableManager variableManager);
}
