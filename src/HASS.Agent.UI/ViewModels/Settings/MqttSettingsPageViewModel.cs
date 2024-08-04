using CommunityToolkit.Mvvm.ComponentModel;
using HASS.Agent.Base.Contracts.Managers;
using HASS.Agent.Base.Enums;
using HASS.Agent.Base.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HASS.Agent.UI.ViewModels.Settings;
public partial class MqttSettingsPageViewModel : ObservableObject
{
	private readonly IMqttManager _mqttManager;
	private readonly MqttSettings _mqttSettings;

	public bool Enabled
	{
		get {
			return _mqttSettings.Enabled;
		}
		set {
			_mqttSettings.Enabled = value;
			OnPropertyChanged(nameof(Enabled));
		}
	}

	public string Address
	{
		get {
			return _mqttSettings.Address;
		}
		set {
			_mqttSettings.Address = value;
			OnPropertyChanged(nameof(Address));
		}
	}

	public int Port
	{
		get {
			return _mqttSettings.Port;
		}
		set {
			_mqttSettings.Port = value;
			OnPropertyChanged(nameof(Port));
		}
	}

	public bool UseTls
	{
		get {
			return _mqttSettings.UseTls;
		}
		set {
			_mqttSettings.UseTls = value;
			OnPropertyChanged(nameof(UseTls));
		}
	}

	public bool AllowUntrustedCertificates
	{
		get {
			return _mqttSettings.AllowUntrustedCertificates;
		}
		set {
			_mqttSettings.AllowUntrustedCertificates = value;
			OnPropertyChanged(nameof(AllowUntrustedCertificates));
		}
	}

	public string Username
	{
		get {
			return _mqttSettings.Username;
		}
		set {
			_mqttSettings.Username = value;
			OnPropertyChanged(nameof(Username));
		}
	}

	public string Password
	{
		get {
			return _mqttSettings.Password;
		}
		set {
			_mqttSettings.Password = value;
			OnPropertyChanged(nameof(Password));
		}
	}

	public string DiscoveryPrefix
	{
		get {
			return _mqttSettings.DiscoveryPrefix;
		}
		set {
			_mqttSettings.DiscoveryPrefix = value;
			OnPropertyChanged(nameof(DiscoveryPrefix));
		}
	}

	public string ClientId
	{
		get {
			return _mqttSettings.ClientId;
		}
		set {
			_mqttSettings.ClientId = value;
			OnPropertyChanged(nameof(ClientId));
		}
	}

	public bool UseRetainFlag
	{
		get {
			return _mqttSettings.UseRetainFlag;
		}
		set {
			_mqttSettings.UseRetainFlag = value;
			OnPropertyChanged(nameof(UseRetainFlag));
		}
	}

	public string RootCertificatePath
	{
		get {
			return _mqttSettings.RootCertificatePath;
		}
		set {
			_mqttSettings.RootCertificatePath = value;
			OnPropertyChanged(nameof(RootCertificatePath));
		}
	}

	public string ClientCertificatePath
	{
		get {
			return _mqttSettings.ClientCertificatePath;
		}
		set {
			_mqttSettings.ClientCertificatePath = value;
			OnPropertyChanged(nameof(ClientCertificatePath));
		}
	}

	public int GracePeriodSeconds
	{
		get {
			return _mqttSettings.GracePeriodSeconds;
		}
		set {
			_mqttSettings.GracePeriodSeconds = value;
			OnPropertyChanged(nameof(GracePeriodSeconds));
		}
	}

	public MqttSettingsPageViewModel(IMqttManager mqttManager, ISettingsManager settingsManager)
	{
		_mqttManager = mqttManager;
		_mqttSettings = settingsManager.Settings.Mqtt;
	}

	public async Task StartClientAsync() => await _mqttManager.StartClientAsync();
	public async Task StopClientAsync()
	{
		await _mqttManager.StopClientAsync();
		while (_mqttManager.Status == MqttStatus.Connected)
			await Task.Delay(TimeSpan.FromMilliseconds(100)); //TODO(Amadeo): add hardcoded failure timeout 
	}

	public async Task RestartMqttClientAsync()
	{
		await _mqttManager.StopClientAsync();
		while (_mqttManager.Status != MqttStatus.Disconnected)
			await Task.Delay(TimeSpan.FromMilliseconds(100));

		await _mqttManager.StartClientAsync();
	}
}
