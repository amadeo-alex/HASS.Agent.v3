using CommunityToolkit.Mvvm.ComponentModel;
using HASS.Agent.Base.Contracts.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HASS.Agent.UI.ViewModels.Settings;
public class MqttSettingsPageViewModel : ObservableObject
{
    public ISettingsManager SettingsManager { get; private set; }

    public MqttSettingsPageViewModel(ISettingsManager settingsManager)
    {
        SettingsManager = settingsManager;
    }
}
