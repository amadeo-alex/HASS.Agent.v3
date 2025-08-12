using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HASS.Agent.Contracts.Models.Settings;
public class UpdateSettings
{
    public bool PeriodicUpdateCheckEnabled { get; set; } = true;
    public string LastUpdateNotificationShown { get; set; } = string.Empty;
    public bool EnableExecuteUpdateInstaller { get; set; } = true;
    public bool ShowBetaUpdates { get; set; }
	public int PeriodicUpdateIntervalMinutes { get; set; } = 30;
	public List<string> IgnoredVersions { get; set; } = [];
}
