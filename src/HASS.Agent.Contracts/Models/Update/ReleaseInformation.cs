using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HASS.Agent.Contracts.Models.Update;

public class ReleaseInformation
{
	public AgentVersion Version { get; set; } = new AgentVersion();
	public string ReleaseUrl { get; set; } = string.Empty;
	public string ReleaseNotes { get; set; } = string.Empty;
	public string InstallerUrl { get; set; } = string.Empty;
	public bool IsNewer { get; set; }
	public Release GithubRelease { get; set; }

	public ReleaseInformation(AgentVersion currentVersion, Release release)
	{
		
		GithubRelease = release;
	}
}
