using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HASS.Agent.Contracts.Models.Update;
public enum VersionComparison
{
	Equal = 0,
	Newer = -1,
	Older = 1
}
