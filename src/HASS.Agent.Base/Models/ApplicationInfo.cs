using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.Contracts.Models.Update;

namespace HASS.Agent.Base.Models;
public class ApplicationInfo //TODO(Amadeo): readonly?
{
    public string Name { get; set; } = string.Empty;
    public AgentVersion Version { get; set; } = new AgentVersion();
    public string ExecutablePath { get; set; } = string.Empty;
    public string Executable { get; set; } = string.Empty;
}
