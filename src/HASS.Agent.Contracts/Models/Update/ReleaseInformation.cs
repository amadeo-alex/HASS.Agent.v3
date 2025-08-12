using Octokit;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HASS.Agent.Contracts.Models.Update;

public class ReleaseInformation
{
	public AgentVersion Version { get; set; } = new AgentVersion();
	public string ReleaseUrl { get => GithubRelease.HtmlUrl; }
	public string ReleaseNotes { get => GithubRelease.Body; }
	public string InstallerUrl { get; set; } = string.Empty;
	public Release GithubRelease { get; set; }

	public ReleaseInformation(Release release)
	{
		var installerAsset = release.Assets.FirstOrDefault(asset => asset.BrowserDownloadUrl.ToLower().EndsWith("installer.exe")); //TODO(Amadeo): better name verification
		if (installerAsset != null)
		{
			InstallerUrl = installerAsset.BrowserDownloadUrl;
		}

		GithubRelease = release;
		Version.Parse(release.TagName);
	}
}
