using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HASS.Agent.Base.Models.Settings;
public partial class MqttSettings : ObservableObject
{
    [ObservableProperty]
    private bool _enabled = true;
    [ObservableProperty]
    private string _address = "homeassistant.local";
    [ObservableProperty]
    private int _port  = 1883;
    [ObservableProperty]
    private bool _useTls = false;
    [ObservableProperty]
    private bool _allowUntrustedCertificates = true;
    [ObservableProperty]
    private string _username = string.Empty;
    [ObservableProperty]
    private string _password = string.Empty;
    [ObservableProperty]
    private string _discoveryPrefix = "homeassistant";
    [ObservableProperty]
    private string _clientId  = string.Empty;
    [ObservableProperty]
    private bool _useRetainFlag  = true;
    [ObservableProperty]
    private string _rootCertificatePath  = string.Empty;
    [ObservableProperty]
    private string _clientCertificatePath = string.Empty;
    [ObservableProperty]
    private int _gracePeriodSeconds  = 60;
}
