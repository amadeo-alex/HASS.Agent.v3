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
	private IMqttManager _mqttManager { get; set; }
	private readonly MqttSettings _mqttSettings;

	[ObservableProperty]
	private bool _restartRequired;
	[ObservableProperty]
	public bool _restarting;

	public MqttStatus Status => _mqttManager.Status;

	public bool Enabled
	{
		get {
			return _mqttSettings.Enabled;
		}
		set {
			if (_mqttSettings.Enabled != value)
			{
				_mqttSettings.Enabled = value;
				OnPropertyChanged(nameof(Enabled));
				RestartRequired = true;
			}
		}
	}

	public string Address
	{
		get {
			return _mqttSettings.Address;
		}
		set {
			if (_mqttSettings.Address != value)
			{
				_mqttSettings.Address = value;
				OnPropertyChanged(nameof(Address));
				RestartRequired = true;
			}
		}
	}

	public int Port
	{
		get {
			return _mqttSettings.Port;
		}
		set {
			if (_mqttSettings.Port != value)
			{
				_mqttSettings.Port = value;
				OnPropertyChanged(nameof(Port));
				RestartRequired = true;
			}
		}
	}

	public bool UseTls
	{
		get {
			return _mqttSettings.UseTls;
		}
		set {
			if (_mqttSettings.UseTls != value)
			{
				_mqttSettings.UseTls = value;
				OnPropertyChanged(nameof(UseTls));
				RestartRequired = true;
			}
		}
	}

	public bool AllowUntrustedCertificates
	{
		get {
			return _mqttSettings.AllowUntrustedCertificates;
		}
		set {
			if (_mqttSettings.AllowUntrustedCertificates != value)
			{
				_mqttSettings.AllowUntrustedCertificates = value;
				OnPropertyChanged(nameof(AllowUntrustedCertificates));
				RestartRequired = true;
			}
		}
	}

	public string Username
	{
		get {
			return _mqttSettings.Username;
		}
		set {
			if (_mqttSettings.Username != value)
			{
				_mqttSettings.Username = value;
				OnPropertyChanged(nameof(Username));
				RestartRequired = true;
			}
		}
	}

	public string Password
	{
		get {
			return _mqttSettings.Password;
		}
		set {
			if (_mqttSettings.Password != value)
			{
				_mqttSettings.Password = value;
				OnPropertyChanged(nameof(Password));
				RestartRequired = true;
			}
		}
	}

	public string DiscoveryPrefix
	{
		get {
			return _mqttSettings.DiscoveryPrefix;
		}
		set {
			if (_mqttSettings.DiscoveryPrefix != value)
			{
				_mqttSettings.DiscoveryPrefix = value;
				OnPropertyChanged(nameof(DiscoveryPrefix));
				RestartRequired = true;
			}
		}
	}

	public string ClientId
	{
		get {
			return _mqttSettings.ClientId;
		}
		set {
			if (_mqttSettings.ClientId != value)
			{
				_mqttSettings.ClientId = value;
				OnPropertyChanged(nameof(ClientId));
				RestartRequired = true;
			}
		}
	}

	public bool UseRetainFlag
	{
		get {
			return _mqttSettings.UseRetainFlag;
		}
		set {
			if (_mqttSettings.UseRetainFlag != value)
			{
				_mqttSettings.UseRetainFlag = value;
				OnPropertyChanged(nameof(UseRetainFlag));
				RestartRequired = true;
			}
		}
	}

	public string RootCertificatePath
	{
		get {
			return _mqttSettings.RootCertificatePath;
		}
		set {
			if (_mqttSettings.RootCertificatePath != value)
			{
				_mqttSettings.RootCertificatePath = value;
				OnPropertyChanged(nameof(RootCertificatePath));
				RestartRequired = true;
			}
		}
	}

	public string ClientCertificatePath
	{
		get {
			return _mqttSettings.ClientCertificatePath;
		}
		set {
			if (_mqttSettings.ClientCertificatePath != value)
			{
				_mqttSettings.ClientCertificatePath = value;
				OnPropertyChanged(nameof(ClientCertificatePath));
				RestartRequired = true;
			}
		}
	}

	public int GracePeriodSeconds
	{
		get {
			return _mqttSettings.GracePeriodSeconds;
		}
		set {
			if (_mqttSettings.GracePeriodSeconds != value)
			{
				_mqttSettings.GracePeriodSeconds = value;
				OnPropertyChanged(nameof(GracePeriodSeconds));
				RestartRequired = true;
			}
		}
	}

	public MqttSettingsPageViewModel(DispatcherQueue dispatcherQueue, IMqttManager mqttManager, ISettingsManager settingsManager) : base(dispatcherQueue)
	{
		_mqttManager = mqttManager;
		_mqttSettings = settingsManager.Settings.Mqtt;
	}

	private void OnMqttPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(_mqttManager.Status))
		{
			RaiseOnPropertyChanged(nameof(Status));
		}
	}

	public async Task RestartClientAsync()
	{
		Restarting = true;
		await _mqttManager.RestartClientAsync();
		Restarting = false;

		RestartRequired = false;
	}

	public void OnNavigatedTo(object parameter)
	{
		_mqttManager.PropertyChanged += OnMqttPropertyChanged;
	}

	public void OnNavigatedFrom()
	{
		_mqttManager.PropertyChanged -= OnMqttPropertyChanged;
	}
}
