using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HASS.Agent.Contracts.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HASS.Agent.Contracts.Models;

public partial class ApplicationSettings : ObservableObject
{
	public bool ExtendedLogging { get; set; } = false;
	public bool SanitizeName  { get; set; } = true;
	
	public OnboardingStatus OnboardingStatus  { get; set; } = OnboardingStatus.NaverDone;
	

	public string DeviceName { get; set; } = string.Empty;
	public string SerialNumber { get; set; } = Guid.NewGuid().ToString();
	
	public string Language { get; set; } = string.Empty;
	
	public string ServiceAuthId { get; set; } = string.Empty;
	public string CustomExecutorName { get; set; } = string.Empty;
	public string CustomExecutorBinary { get; set; } = string.Empty;

	public bool LocalApiEnabled { get; set; } = false;
	public int LocalApiPort { get; set; } = 5115;
	
	public bool MediaPlayerEnabled { get; set; } = true;
	
	public bool QuickActionsHotKeyEnabled { get; set; } = true;
	public string QuickActionsHotKey { get; set; } = string.Empty;
}