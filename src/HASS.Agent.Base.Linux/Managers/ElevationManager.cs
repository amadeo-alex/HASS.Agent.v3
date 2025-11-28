using System.Runtime.InteropServices;
using HASS.Agent.Contracts.Managers;

namespace HASS.Agent.Base.Linux.Managers;

public partial class ElevationManager : IElevationManager
{
    [LibraryImport("libc")]
    private static partial uint getuid();

    public bool RunningElevated => getuid() == 0;
}