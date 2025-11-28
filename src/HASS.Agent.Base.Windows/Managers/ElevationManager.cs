using System.Security.Principal;
using HASS.Agent.Contracts.Managers;

namespace HASS.Agent.Base.Windows.Managers;

public class ElevationManager : IElevationManager
{
    public bool RunningElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
}