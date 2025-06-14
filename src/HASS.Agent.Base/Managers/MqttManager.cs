using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models.Entity;
using HASS.Agent.Contracts.Models.MediaPlayer;
using HASS.Agent.Contracts.Models.Mqtt;
using HASS.Agent.Base.Models;
using HASS.Agent.Base.Models.Mqtt;
using HASS.Agent.Helpers;
using MQTTnet;
using MQTTnet.Adapter;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Windows.Globalization;
using HASS.Agent.Contracts.Models;
using HASS.Agent.Contracts.Models.Settings;
using HASS.Agent.Contracts.Enums;
using Microsoft.Extensions.Logging;

namespace HASS.Agent.Base.Managers;

public partial class MqttManager : ObservableObject, IMqttManager
{
	//public const string DefaultMqttDiscoveryPrefix = "homeassistant";
	public const string PayloadOnline = "online";
	public const string PayloadOffline = "offline";

    private readonly ILogger _logger;

	private readonly JsonSerializerSettings _jsonSerializerSettings = new()
	{
		Formatting = Formatting.Indented,
		ContractResolver = new DefaultContractResolver()
		{
			NamingStrategy = new CamelCaseNamingStrategy()
		},
		NullValueHandling = NullValueHandling.Ignore,
	};

	private readonly ISettingsManager _settingsManager;
	private readonly ApplicationInfo _applicationInfo;
	private readonly IGuidManager _guidManager;

	private readonly IManagedMqttClient _mqttClient = new MqttFactory().CreateManagedMqttClient();
	private ManagedMqttClientOptions _mqttClientOptions;

	private bool _connectionErrorLogged = false;

	private ApplicationSettings _applicationSettingsSnapshot;
	private MqttSettings _mqttSettingsSnapshot;

	private DateTime _lastAvailabilityAnnouncment = DateTime.MinValue;
	private DateTime _lastAvailabilityAnnouncmentFailed = DateTime.MinValue;

	private readonly Dictionary<string, IMqttMessageHandler> _mqttMessageHandlers = [];

	[ObservableProperty]
	public MqttStatus status = MqttStatus.NotInitialized;
	public bool Initialized { get; private set; } = false;
	public bool Ready
	{
		get => Initialized && Status == MqttStatus.Connected;
	}

	public AbstractMqttDeviceConfigModel DeviceConfigModel { get; set; }

	public MqttManager(ILogger<MqttManager> logger, ISettingsManager settingsManager, ApplicationInfo applicationInfo, IGuidManager guidManager)
	{
        _logger = logger;

		_logger.LogInformation("[MQTT] Initializing manager");

		_settingsManager = settingsManager;
		_applicationInfo = applicationInfo;
		_guidManager = guidManager;

		_mqttSettingsSnapshot = settingsManager.Settings.Mqtt.JsonClone<MqttSettings>();
		_applicationSettingsSnapshot = settingsManager.Settings.Application.JsonClone<ApplicationSettings>();


		// default initialization
		DeviceConfigModel = GetDeviceConfigModel();

		ConfigureMqttClient();
		_mqttClientOptions = GetMqttClientOptions();

        _logger.LogInformation("[MQTT] Manager initialized");
	}

	private MqttDeviceDiscoveryConfigModel GetDeviceConfigModel()
	{
		var deviceName = _applicationSettingsSnapshot.DeviceName;

		return new MqttDeviceDiscoveryConfigModel()
		{
			Name = deviceName,
			Identifiers = $"hass.agent-{deviceName}",
			Manufacturer = "HASS.Agent Team",
			Model = Environment.OSVersion.ToString(),
			SoftwareVersion = _applicationInfo.Version.ToString(),
		};
	}

	private void ConfigureMqttClient()
	{
		_logger.LogInformation("[MQTT] Initializing client");

		if (!_mqttSettingsSnapshot.Enabled)
		{
			_logger.LogInformation("[MQTT] Initialization stopped, disabled through settings");
            return;
		}

		_mqttClient.ConnectedAsync += OnConnectedAsync;
		_mqttClient.ConnectingFailedAsync += OnConnectingFailedAsync;
		_mqttClient.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;
		_mqttClient.DisconnectedAsync += OnDisconnectedAsync;
		_mqttClient.ApplicationMessageSkippedAsync += OnApplicationMessageSkippedAsync;

		_logger.LogDebug("[MQTT] Client initialized");
	}

	public void RegisterMessageHandler(string topic, IMqttMessageHandler handler)
	{
		if (_mqttMessageHandlers.ContainsKey(topic))
			throw new ArgumentException($"handler for {topic} already registered");

		_mqttMessageHandlers[topic] = handler;
		_mqttClient.SubscribeAsync(topic);
	}

	public void UnregisterMessageHandler(string topic)
	{
		_mqttClient.UnsubscribeAsync(topic);
		_mqttMessageHandlers.Remove(topic);
	}

	public async Task StartClientAsync()
	{
        TakeSettingsSnapshot();

        _logger.LogDebug("[MQTT] Attempting to start the client");
        if (!_mqttSettingsSnapshot.Enabled)
        {
            _logger.LogError("[MQTT] Start has been aborted, MQTT is disabled");
            return;
        }

        try
		{
			DeviceConfigModel = GetDeviceConfigModel();

			//_mqttClient = GetMqttClient();
			_mqttClientOptions = GetMqttClientOptions();

			await _mqttClient.StartAsync(_mqttClientOptions);
			InitialRegistration();
		}
		catch (MqttConnectingFailedException e)
		{
			_logger.LogError("[MQTT] Unable to connect to broker: {msg}", e.Result.ToString());
		}
		catch (MqttCommunicationException e)
		{
			_logger.LogError("[MQTT] Unable to communicate with broker: {msg}", e.Message);
		}
		catch (Exception e)
		{
			_logger.LogError("[MQTT] Exception while connecting with broker: {msg}", e.ToString());
		}
	}

	public async Task StopClientAsync()
	{
		_logger.LogDebug("[MQTT] Attempting to stop the client");

		Initialized = false;

		Status = MqttStatus.Disconnecting;
		await _mqttClient.StopAsync();
		_logger.LogDebug("[MQTT] Attempting to stop the client - finished A");
		await _mqttClient.InternalClient.DisconnectAsync();

		_logger.LogDebug("[MQTT] Attempting to stop the client - finished B");
		Status = MqttStatus.Disconnected;

		return;
	}

	public async Task RestartClientAsync()
	{
		_logger.LogDebug("[MQTT] Restarting client");

		await StopClientAsync();
		while (Status == MqttStatus.Disconnecting)
		{
			await Task.Delay(100);
		}

		await StartClientAsync();
	}

	private void TakeSettingsSnapshot()
	{
		_mqttSettingsSnapshot = _settingsManager.Settings.Mqtt.JsonClone<MqttSettings>();
		_applicationSettingsSnapshot = _settingsManager.Settings.Application.JsonClone<ApplicationSettings>();
	}

	private async void InitialRegistration()
	{
		while (!_mqttClient.IsConnected || Status != MqttStatus.Connected)
			await Task.Delay(2000);

		await AnnounceAvailabilityAsync();
		Initialized = true;

		_logger.LogInformation("[MQTT] Initial registration completed");
	}

	private async Task AnnounceAvailabilityAsync(bool offline = false) //TODO(Amadeo): move to separate handler?
	{
		try
		{
			if (!offline)
			{
				if ((DateTime.Now - _lastAvailabilityAnnouncment).TotalSeconds < 30) //TODO(Amadeo): make configurable via UI
					return;
			}

			if (_mqttClient.IsConnected)
			{
				var topic = $"{_mqttSettingsSnapshot.DiscoveryPrefix}/hass.agent/{_applicationSettingsSnapshot.DeviceName}/availability";
				var availabilityMessage = new MqttApplicationMessageBuilder()
					.WithTopic(topic)
					.WithPayload(offline ? PayloadOffline : PayloadOnline)
					.WithRetainFlag(_mqttSettingsSnapshot.UseRetainFlag)
					.Build();

				await _mqttClient.EnqueueAsync(availabilityMessage);

				//TODO: integration message

			}
			else
			{
				if ((DateTime.Now - _lastAvailabilityAnnouncmentFailed).TotalMinutes < 5) //TODO(Amadeo): make configurable?
					return;

				_lastAvailabilityAnnouncmentFailed = DateTime.Now;
				_logger.LogWarning("[MQTT] Not connected, availability announcement dropped");
			}

			_lastAvailabilityAnnouncment = DateTime.Now;
		}
		catch (Exception e)
		{
			_logger.LogCritical(e, "[MQTT] Error while announcing availability: {err}", e.Message);
		}
	}

	private async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
	{
		Status = MqttStatus.Disconnected;
		_logger.LogInformation("[MQTT] Disconnected");
	}

	private async Task OnApplicationMessageSkippedAsync(ApplicationMessageSkippedEventArgs args)
	{
		_logger.LogInformation("[MQTT] Message skipped/dropped");
	}

	private async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
	{
		var applicationMessage = arg.ApplicationMessage;
		if (applicationMessage.PayloadSegment.Count == 0)
		{
			_logger.LogInformation("[MQTT] Received empty payload on {topic}", applicationMessage.Topic);
			return;
		}

		try
		{
			foreach (var (registeredTopic, handler) in _mqttMessageHandlers)
			{
				if (CheckTopicMatch(applicationMessage.Topic, registeredTopic))
					await handler.HandleMqttMessage(applicationMessage);
			}
		}
		catch (Exception ex)
		{
			_logger.LogCritical(ex, "[MQTT] Error while processing received message: {err}", ex.Message);
		}

		return;

		try
		{
			if (applicationMessage.Topic == $"hass.agent/notifications/{DeviceConfigModel.Name}")
			{
				var payload = Encoding.UTF8.GetString(applicationMessage.PayloadSegment).ToLower();
				if (payload == null)
					return;

				//var notification = JsonConvert.DeserializeObject<Notification>(payload, _jsonSerializerSettings);
				//_notificationManager.HandleReceivedNotification(notification);
				//TODO(Amadeo): event/observable to show notification

				return;
			}

			if (applicationMessage.Topic == $"hass.agent/media_player/{DeviceConfigModel.Name}/cmd")
			{
				var payload = Encoding.UTF8.GetString(applicationMessage.PayloadSegment).ToLower();
				if (payload == null)
					return;

				var command = JsonConvert.DeserializeObject<MediaPlayerCommand>(payload, _jsonSerializerSettings)!;
				//_mediaManager.HandleReceivedCommand(command);

				/*                switch (command.Type)
								{
									case MediaPlayerCommandType.PlayMedia:
										MediaManager.ProcessMedia(command.Data.GetString());
										break;
									case MediaPlayerCommandType.Seek:
										MediaManager.ProcessSeekCommand(TimeSpan.FromSeconds(command.Data.GetDouble()).Ticks);
										break;
									case MediaPlayerCommandType.SetVolume:
										MediaManagerCommands.SetVolume(command.Data.GetInt32());
										break;
									default:
										MediaManager.ProcessCommand(command.Command);
										break;
								}*/

				return;
			}

			//_commandsManager.HandleReceivedCommand(applicationMessage);

			/*            foreach (var command in Variables.Commands)
						{
							var commandConfig = (CommandDiscoveryConfigModel)command.GetAutoDiscoveryConfig();

							if (commandConfig.Command_topic == applicationMessage.Topic)
								HandleCommandReceived(applicationMessage, command);
							else if (commandConfig.Action_topic == applicationMessage.Topic)
								HandleActionReceived(applicationMessage, command);
						}*/
		}
		catch (Exception ex)
		{
			_logger.LogCritical(ex, "[MQTT] Error while processing received message: {err}", ex.Message);
		}
	}

	private async Task OnConnectedAsync(MqttClientConnectedEventArgs arg)
	{
		Status = MqttStatus.Connected;
		_logger.LogInformation("[MQTT] Connected");

		_connectionErrorLogged = false;

		return;
	}

	private async Task OnConnectingFailedAsync(ConnectingFailedEventArgs arg)
	{
		Status = MqttStatus.Error;
		_logger.LogInformation("[MQTT] Connecting failed");

		if (_connectionErrorLogged)
			return;

		_connectionErrorLogged = true;

		var exceptionMessage = arg.Exception.ToString();
		if (exceptionMessage.Contains("SocketException"))
			_logger.LogError("[MQTT] Error while connecting: {err}", arg.Exception.Message);
		else if (exceptionMessage.Contains("MqttCommunicationTimedOutException"))
			_logger.LogError("[MQTT] Error while connecting: {err}", "Connection timed out");
		else if (exceptionMessage.Contains("NotAuthorized"))
			_logger.LogError("[MQTT] Error while connecting: {err}", "Not authorized, check your credentials.");
		else
			_logger.LogCritical(arg.Exception, "[MQTT] Error while connecting: {err}", arg.Exception.Message);

		//TODO(Amadeo): event/observable and notify user
	}

	private ManagedMqttClientOptions GetMqttClientOptions()
	{
		if (string.IsNullOrWhiteSpace(_mqttSettingsSnapshot.Address))
		{
			_logger.LogWarning("[MQTT] Required configuration missing");

			return new ManagedMqttClientOptionsBuilder().Build();
		}

		// id can be random, but we'll store it for consistency (unless user-defined)
		if (string.IsNullOrWhiteSpace(_mqttSettingsSnapshot.ClientId))
		{
			_logger.LogInformation("[MQTT] ClientId is empty, generating new one");
			_mqttSettingsSnapshot.ClientId = _guidManager.GenerateShortGuid();
			//TODO(Amadeo): save settings to file
			//SettingsManager.StoreAppSettings();
		}

		var clientOptionsBuilder = new MqttClientOptionsBuilder()
			.WithClientId(_mqttSettingsSnapshot.ClientId)
			.WithTcpServer(_mqttSettingsSnapshot.Address, _mqttSettingsSnapshot.Port)
			.WithCleanSession()
			.WithWillTopic($"{_mqttSettingsSnapshot.DiscoveryPrefix}/sensor/{DeviceConfigModel.Name}/availability")
			.WithWillPayload(PayloadOffline)
			.WithWillRetain(_mqttSettingsSnapshot.UseRetainFlag)
			.WithKeepAlivePeriod(TimeSpan.FromSeconds(15));

		if (!string.IsNullOrEmpty(_mqttSettingsSnapshot.Username))
			clientOptionsBuilder.WithCredentials(_mqttSettingsSnapshot.Username, _mqttSettingsSnapshot.Password);

		var certificates = new List<X509Certificate>();
		if (_mqttSettingsSnapshot.UseCustomRootCertificate && !string.IsNullOrEmpty(_mqttSettingsSnapshot.RootCertificatePath))
		{
			if (!File.Exists(_mqttSettingsSnapshot.RootCertificatePath))
				_logger.LogError("[MQTT] Provided root certificate not found: {cert}", _mqttSettingsSnapshot.RootCertificatePath);
			else
				certificates.Add(new X509Certificate2(_mqttSettingsSnapshot.RootCertificatePath));
		}


		if (_mqttSettingsSnapshot.UseClientCertificate && !string.IsNullOrEmpty(_mqttSettingsSnapshot.ClientCertificatePath))
		{
			if (!File.Exists(_mqttSettingsSnapshot.ClientCertificatePath))
				_logger.LogError("[MQTT] Provided client certificate not found: {cert}", _mqttSettingsSnapshot.ClientCertificatePath);
			else
				certificates.Add(new X509Certificate2(_mqttSettingsSnapshot.ClientCertificatePath));
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
			clientTlsOptions.CertificateValidationHandler = delegate (MqttClientCertificateValidationEventArgs _)
			{
				return true;
			};
		}

		if (certificates.Count > 0)
			clientTlsOptions.ClientCertificatesProvider = new DefaultMqttCertificatesProvider(certificates);

		clientOptionsBuilder.WithTlsOptions(clientTlsOptions);
		clientOptionsBuilder.Build();

		return new ManagedMqttClientOptionsBuilder()
			.WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
			.WithClientOptions(clientOptionsBuilder).Build();
	}

	public async Task AnnounceDeviceConfigModelAsync()
	{

		return;
	}

	public async Task ClearDeviceConfigModelAsync()
	{

		return;
	}

	public async Task PublishAsync(MqttApplicationMessage message)
	{
		if (!Ready)
			return;

		await _mqttClient.EnqueueAsync(message);

		return;
	}

	/// Idea thanks to https://github.com/hobbyquaker/mqtt-wildcard implementation
	private bool CheckTopicMatch(string messageTopic, string registeredTopic)
	{
		if (messageTopic == registeredTopic || registeredTopic == "#")
			return true;

		var splitTopic = messageTopic.Split('/');
		var splitRegisteredTopic = registeredTopic.Split('/');
		if (splitTopic.Length > splitRegisteredTopic.Length && splitRegisteredTopic.Last() != "#")
			return false;

		var index = 0;
		for (; index < splitTopic.Length; index++)
		{
			if (splitRegisteredTopic[index] == "+")
				continue;
			else if (splitRegisteredTopic[index] == "#")
				return true;
			else if (splitRegisteredTopic[index] != splitTopic[index])
				return false;
		}

		if (splitRegisteredTopic.Length > index && splitRegisteredTopic[index] == "#")
			index += 1;

		return index == splitRegisteredTopic.Length;
	}
}
