using HASS.Agent.Contracts.Models.Entity;

namespace HASS.Agent.Client.ViewModels;

public class EntitySettingsViewModelBase : ViewModelBase
{
    public ConfiguredEntity Entity { get; set; } = new ConfiguredEntity();

    public EntitySettingsViewModelBase()
    {
        
    }

    public EntitySettingsViewModelBase(ConfiguredEntity entity)
    {
        Entity = entity;
    }
}