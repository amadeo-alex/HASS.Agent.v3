using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using HADotNet.Core;
using HADotNet.Core.Clients;
using HASS.Agent.Contracts.Managers;
using Microsoft.Extensions.Logging;

namespace HASS.Agent.Base.Managers.HomeAssistant;
public class HomeAssistantApiManager : IHomeAssistantApiManager
{
    private readonly ILogger _logger;

    private readonly ISettingsManager _settingsManager;

    private ServiceClient? _serviceClient;
    private EventClient? _eventClient;

    public HomeAssistantApiManager(ILogger<HomeAssistantApiManager> logger, ISettingsManager settingsManager)
    {
        _logger = logger;

        _settingsManager = settingsManager;
    }

    public async Task InitializeAsync()
    {
        _logger.LogDebug("[HAAPI] Initializing");

        try
        {
            var uri = new Uri(_settingsManager.Settings.HomeAssistant.HassUri);
            var httpClientHandler = new HttpClientHandler();

            if (_settingsManager.Settings.HomeAssistant.HassAutoClientCertificate)
            {
                httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Automatic;
            }
            else
            {
                //TODO(Amadeo): certificate loading error handling
                httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
                httpClientHandler.ClientCertificates.Add(new X509Certificate2(_settingsManager.Settings.HomeAssistant.HassClientCertificate));
            }

            if (_settingsManager.Settings.HomeAssistant.HassAllowUntrustedCertificates)
            {
                httpClientHandler.CheckCertificateRevocationList = false;
                httpClientHandler.ServerCertificateCustomValidationCallback += (_, _, _, _) => true;
            }

            ClientFactory.Initialize(uri, _settingsManager.Settings.HomeAssistant.HassToken, httpClientHandler);

            _serviceClient = ClientFactory.GetClient<ServiceClient>();
            _eventClient = ClientFactory.GetClient<EventClient>();

            _logger.LogInformation("[HAAPI] Initialized");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[HAAPI] Error during initialization: {err}", ex.Message);
        }
    }

    public async Task FireEventAsync(string eventName, object eventData)
    {
        if (_eventClient != null) { 

            var result = await _eventClient.FireEvent(eventName, eventData);
        }
    }
}
