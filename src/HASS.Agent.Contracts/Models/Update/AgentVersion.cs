using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HASS.Agent.Contracts.Models.Update;
public partial class AgentVersion
{
	public const string BetaTag = "beta";
	public const string NightlyTag = "nightly";

	private const string _versionRegex = "^\\d+\\.\\d+\\.?\\d?(-(.+)(\\d+\\.\\d+\\.?\\d?))?$";

	[GeneratedRegex(_versionRegex)]
	private static partial Regex VersionRegex();


	public Version Base { get; set; } = new Version();
	public Version Additional { get; set; } = new Version();
	public bool IsBeta { get => Tag == BetaTag; }
	public bool IsNightly { get => Tag == NightlyTag; }
	public string Tag { get; set; } = string.Empty;

	public AgentVersion()
	{
		
	}

	public AgentVersion(string versionString)
	{
		Parse(versionString);
	}

	public bool Parse(string versionString)
	{
		if (VersionRegex().Matches(versionString).Count != 1)
		{
			return false;
		}

		var splitVersion = versionString.Split("-");

		Base = Version.Parse(splitVersion[0]);
		if (splitVersion.Length > 1)
		{
			var versionIndex = splitVersion[1].IndexOfAny("0123456789".ToCharArray());
			Tag = splitVersion[1][0..versionIndex];

			var additonalVersion = splitVersion[1][versionIndex..];
			Additional = Version.Parse(additonalVersion);
		}

		return true;
	}

	public VersionComparison CompareTo(AgentVersion otherVersion)
	{
		var baseComparison = (VersionComparison)Base.CompareTo(otherVersion.Base);
		if(baseComparison != VersionComparison.Equal)
		{
			return baseComparison;
		}

		var additionalComparison = (VersionComparison)Additional.CompareTo(otherVersion.Additional);
		return additionalComparison;
	}
}
