﻿// Ignore Spelling: Dpi Mqtt

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.Base.Contracts.Models;
using HASS.Agent.Base.Contracts.Models.Entity;
using HASS.Agent.Base.Models;
using HASS.Agent.Base.Models.Settings;

namespace HASS.Agent.Base.Contracts.Managers;
public interface ISettingsManager
{
    ISettings Settings { get; }

    ObservableCollection<ConfiguredEntity> ConfiguredSensors { get; }
    ObservableCollection<ConfiguredEntity> ConfiguredCommands { get; }
    ObservableCollection<IQuickAction> ConfiguredQuickActions { get; } //TODO(Amadeo): rethink

    bool StoreConfiguredEntities();
    bool StoreSettings();
    bool GetExtendedLoggingSetting();
    void SetExtendedLoggingSetting(bool enabled);
    bool GetDpiWarningShown();
    void SetDpiWarningShown(bool shown);
    //TODO(Amadeo): remove
    Task<bool> SendMqttSettingsToServiceAsync(bool sendNewClientId = false);
    string GetDeviceSerialNumber();
    void SetDeviceSerialNumber(string deviceSerialNumber);
    bool GetHideDonateButtonSetting();
    void SetHideDonateButtonSetting(bool hide);
}
