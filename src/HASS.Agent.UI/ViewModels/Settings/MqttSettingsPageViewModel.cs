using CommunityToolkit.Mvvm.ComponentModel;
using HASS.Agent.Base.Contracts.Managers;
using HASS.Agent.Base.Enums;
using HASS.Agent.Base.Models.Settings;
using HASS.Agent.UI.Contracts.ViewModels;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HASS.Agent.UI.ViewModels.Settings;
public partial class MqttSettingsPageViewModel : ViewModelBase, INavigationAware
{
	private IMqttManager _mqttManager;
	private readonly MqttSettings _mqttSettings;

	public bool DetailsExpanded => Status != MqttStatus.Connected || RestartRequired;

	public MqttStatus Status => _mqttManager.Status;
	public bool RestartRequired
	{
		get => _mqttSettings.RestartRequired;
		set => _mqttSettings.RestartRequired = value;
	}

	public bool Enabled
	{
		get => _mqttSettings.Enabled;
		set => _mqttSettings.Enabled = value;
	}

	public string Address
	{
		get => _mqttSettings.Address;
		set => _mqttSettings.Address = value;
	}

	public int Port
	{
		get => _mqttSettings.Port;
		set => _mqttSettings.Port = value;
	}

	public bool UseTls
	{
		get => _mqttSettings.UseTls;
		set => _mqttSettings.UseTls = value;
	}

	public bool AllowUntrustedCertificates
	{
		get => _mqttSettings.AllowUntrustedCertificates;
		set => _mqttSettings.AllowUntrustedCertificates = value;
	}

	public string Username
	{
		get => _mqttSettings.Username;
		set => _mqttSettings.Username = value;
	}

	public string Password
	{
		get => _mqttSettings.Password;
		set => _mqttSettings.Password = value;
	}

	public string DiscoveryPrefix
	{
		get => _mqttSettings.DiscoveryPrefix;
		set => _mqttSettings.DiscoveryPrefix = value;
	}

	public string ClientId
	{
		get => _mqttSettings.ClientId;
		set => _mqttSettings.ClientId = value;
	}

	public bool UseRetainFlag
	{
		get => _mqttSettings.UseRetainFlag;
		set => _mqttSettings.UseRetainFlag = value;
	}

	public string RootCertificatePath
	{
		get => _mqttSettings.RootCertificatePath;
		set => _mqttSettings.RootCertificatePath = value;
	}

	public string ClientCertificatePath
	{
		get => _mqttSettings.ClientCertificatePath;
		set => _mqttSettings.ClientCertificatePath = value;
	}

	public int GracePeriodSeconds
	{
		get => _mqttSettings.GracePeriodSeconds;
		set => _mqttSettings.GracePeriodSeconds = value;
	}

	public MqttSettingsPageViewModel(DispatcherQueue dispatcherQueue, IMqttManager mqttManager, ISettingsManager settingsManager) : base(dispatcherQueue)
	{
		_mqttManager = mqttManager;
		_mqttSettings = settingsManager.Settings.Mqtt;

		AddPropertyListenerMap(nameof(_mqttManager.Status), new List<string>
		{
			nameof(Status),
			nameof(DetailsExpanded)
		});

		AddPropertyListenerMap(nameof(_mqttSettings.RestartRequired), new List<string>
		{
			nameof(RestartRequired),
			nameof(DetailsExpanded)
		});

		AddPropertyListenerMap(new List<string>{
			nameof(_mqttSettings.Enabled),
			nameof(_mqttSettings.Address),
			nameof(_mqttSettings.Port),
			nameof(_mqttSettings.UseTls),
			nameof(_mqttSettings.AllowUntrustedCertificates),
			nameof(_mqttSettings.Username),
			nameof(_mqttSettings.Password),
			nameof(_mqttSettings.DiscoveryPrefix),
			nameof(_mqttSettings.ClientId),
			nameof(_mqttSettings.UseRetainFlag),
			nameof(_mqttSettings.RootCertificatePath),
			nameof(_mqttSettings.ClientCertificatePath),
			nameof(_mqttSettings.GracePeriodSeconds)
		});
	}

	private void OnMqttPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		ParseSourcePropertyChanged(e);

		if (sender is MqttSettings && e.PropertyName != nameof(RestartRequired))
		{
			RestartRequired = true;
		}
	}

	public async Task RestartClientAsync()
	{
		await _mqttManager.RestartClientAsync();

		RestartRequired = false;
	}

	public void OnNavigatedTo(object parameter)
	{
		_mqttManager.PropertyChanged += OnMqttPropertyChanged;
		_mqttSettings.PropertyChanged += OnMqttPropertyChanged;
	}

	public void OnNavigatedFrom()
	{
		_mqttManager.PropertyChanged -= OnMqttPropertyChanged;
		_mqttSettings.PropertyChanged -= OnMqttPropertyChanged;
	}
}
