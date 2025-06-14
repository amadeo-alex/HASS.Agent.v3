using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.Contracts.Managers;

namespace HASS.Agent.UI.Managers;

class ElevationManager : IElevationManager
{
    public bool RunningElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
}
