using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HASS.Agent.Contracts.Models.Update;
public class UpdateAvailableEventArgs : EventArgs
{
	public ReleaseInformation? Release { get; set; }
}
