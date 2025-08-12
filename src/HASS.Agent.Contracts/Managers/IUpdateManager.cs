using HASS.Agent.Contracts.Models.Update;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HASS.Agent.Contracts.Managers;

public interface IUpdateManager
{
	Task InitializeAsync();
	Task<ReleaseInformation> GetLatestReleaseAsync();
}
