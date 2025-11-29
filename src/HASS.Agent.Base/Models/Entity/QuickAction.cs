using HASS.Agent.Contracts.Enums;
using HASS.Agent.Contracts.Models.Entity;

namespace HASS.Agent.Base.Models.Entity;

public class QuickAction : IQuickAction
{
    public Guid UniqueId { get; set; }
    public HassDomain Domain { get; set; }
    public string Entity { get; set; } = string.Empty;
    public HassAction Action { get; set; }
    public bool HotKeyEnabled { get; set; }
    public string HotKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}