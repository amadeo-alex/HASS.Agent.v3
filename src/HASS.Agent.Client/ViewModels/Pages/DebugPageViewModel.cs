using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HASS.Agent.Base.Models;
using HASS.Agent.Base.Models.Mqtt;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models;
using HASS.Agent.Contracts.Models.Mqtt;
using HASS.Agent.Contracts.Models.Settings;
using Microsoft.Extensions.Logging;
using MQTTnet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HASS.Agent.Client.ViewModels.Pages;

public partial class DebugPageViewModel : ViewModelBase, INavigationAware
{
    private const string DevicesTopic = "hass.agent/devices/";
    private const string DevicesWildcardTopic = $"{DevicesTopic}#";
    
    private readonly ILogger _logger;

    private readonly IMqttClient _mqttClient = new MqttClientFactory().CreateMqttClient();
    private MqttClientOptions _mqttClientOptions;

    private readonly ISettingsManager _settingsManager;
    private readonly ApplicationInfo _applicationInfo;
    
    private readonly Dictionary<string, HomeAssistantDeviceDiscoveryModel> _discoveredDevicesTopicMap = new();

    private ApplicationSettings _applicationSettingsSnapshot;
    private MqttSettings _mqttSettingsSnapshot;
    
    public ObservableCollection<HomeAssistantDeviceDiscoveryModel> DiscoveredDevices { get; } = [];

    public DebugPageViewModel(ILogger<DebugPageViewModel> logger, ISettingsManager settingsManager, ApplicationInfo applicationInfo)
    {
        _logger = logger;
        _settingsManager = settingsManager;
        _applicationInfo = applicationInfo;
        
        TakeSettingsSnapshot();

        _mqttClientOptions = GetMqttClientOptions();

        _mqttClient.ConnectedAsync += OnConnectedAsync;
        //_mqttClient.ConnectingFailedAsync += OnConnectingFailedAsync;
        _mqttClient.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;
    }

    private async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        var applicationMessage = arg.ApplicationMessage;
        if (applicationMessage.Payload.Length == 0)
        {
            if (_discoveredDevicesTopicMap.Remove(arg.ApplicationMessage.Topic, out var deviceConfig))
            {
                DiscoveredDevices.Remove(deviceConfig);
            }

            return;
        }

        var payload = Encoding.UTF8.GetString(applicationMessage.Payload);
        var jsonObject = JObject.Parse(payload)["device"]; //TODO(Amadeo): fix this for the love of god, proper classes etc.
        var discoveryModel = jsonObject?.ToObject<HomeAssistantDeviceDiscoveryModel>();
        if (discoveryModel == null)
        {
            return;
        }
        
        if (!DiscoveredDevices.Contains(discoveryModel))
        {
            _discoveredDevicesTopicMap[arg.ApplicationMessage.Topic] = discoveryModel;
            DiscoveredDevices.Add(discoveryModel);
        }
    }

    private async Task OnConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        _discoveredDevicesTopicMap.Clear();
        DiscoveredDevices.Clear();
    }

    private MqttClientOptions GetMqttClientOptions()
    {
        if (string.IsNullOrWhiteSpace(_mqttSettingsSnapshot.Address))
        {
            return new MqttClientOptionsBuilder().Build();
        }

        _mqttSettingsSnapshot.ClientId = "hassAgent-debug-client-id"; //TODO(Amadeo): change with rename

        var clientOptionsBuilder = new MqttClientOptionsBuilder()
            .WithClientId(_mqttSettingsSnapshot.ClientId)
            .WithCleanSession()
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(1));

        if (_mqttSettingsSnapshot.UseWebSocket)
        {
            clientOptionsBuilder.WithWebSocketServer(o => o.WithUri($"{_mqttSettingsSnapshot.Address}:{_mqttSettingsSnapshot.Port}"));
        }
        else
        {
            clientOptionsBuilder.WithTcpServer(_mqttSettingsSnapshot.Address, _mqttSettingsSnapshot.Port);
        }

        if (!string.IsNullOrEmpty(_mqttSettingsSnapshot.Username))
        {
            clientOptionsBuilder.WithCredentials(_mqttSettingsSnapshot.Username, _mqttSettingsSnapshot.Password);
        }

        var certificates = new List<X509Certificate>();
        if (_mqttSettingsSnapshot.UseCustomRootCertificate && !string.IsNullOrEmpty(_mqttSettingsSnapshot.RootCertificatePath))
        {
            if (File.Exists(_mqttSettingsSnapshot.RootCertificatePath))
            {
                certificates.Add(new X509Certificate2(_mqttSettingsSnapshot.RootCertificatePath));
            }
        }

        if (_mqttSettingsSnapshot.UseClientCertificate && !string.IsNullOrEmpty(_mqttSettingsSnapshot.ClientCertificatePath))
        {
            if (File.Exists(_mqttSettingsSnapshot.ClientCertificatePath))
            {
                certificates.Add(new X509Certificate2(_mqttSettingsSnapshot.ClientCertificatePath));
            }
        }

        var clientTlsOptions = new MqttClientTlsOptions()
        {
            UseTls = _mqttSettingsSnapshot.UseTls,
            AllowUntrustedCertificates = _mqttSettingsSnapshot.AllowUntrustedCertificates,
            SslProtocol = _mqttSettingsSnapshot.UseTls ? SslProtocols.Tls12 : SslProtocols.None, //TODO(Amadeo): TLS1.3?
        };

        //TODO(Amadeo): verify the validation handler and options working as expected
        if (_mqttSettingsSnapshot.AllowUntrustedCertificates)
        {
            clientTlsOptions.IgnoreCertificateChainErrors = _mqttSettingsSnapshot.AllowCertificateChainErrors;
            clientTlsOptions.IgnoreCertificateRevocationErrors = _mqttSettingsSnapshot.AllowCertificationRevokationErrors;
            clientTlsOptions.CertificateValidationHandler = delegate(MqttClientCertificateValidationEventArgs _) { return true; };
        }

        if (certificates.Count > 0)
        {
            clientTlsOptions.ClientCertificatesProvider = new DefaultMqttCertificatesProvider(certificates);
        }

        clientOptionsBuilder.WithTlsOptions(clientTlsOptions);

        return clientOptionsBuilder.Build();
    }

    private void TakeSettingsSnapshot()
    {
        _mqttSettingsSnapshot = _settingsManager.GetSettingsSnapshot<MqttSettings>();
        _applicationSettingsSnapshot = _settingsManager.GetSettingsSnapshot<ApplicationSettings>();
    }

    public void OnNavigatedTo()
    {
        Task.Run(async () =>
        {
            await _mqttClient.ConnectAsync(_mqttClientOptions);
            await _mqttClient.SubscribeAsync(DevicesWildcardTopic);
        });
    }

    public void OnNavigatedFrom()
    {
        Task.Run(async () => { await _mqttClient.DisconnectAsync(); });
    }
}