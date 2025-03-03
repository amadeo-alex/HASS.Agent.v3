using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Managers.HomeAssistant;

namespace HASS.Agent.Base.Managers.HomeAssistant;
public class HomeAssistantEvent : IHomeAssistantEvent
{
    public string Type {  get; set; } = string.Empty;
    public object? EventData {  get; set; }
}
