using HASS.Agent.Base.Sensors.SingleValue;
using HASS.Agent.Contracts.Models.Entity;

namespace HASS.Agent.Client.ViewModels.Sensors;

public class DummySensorSettingsViewModel : EntitySettingsViewModelBase
{
    public bool EnsureRandom
    {
        get => Entity.GetBoolParameter(DummySensor.EnsureRandomKey, false);
        set => Entity.SetBoolParameter(DummySensor.EnsureRandomKey, value);
    }

    public int MinValue
    {
        get => Entity.GetIntParameter(DummySensor.MinValueKey, 0);
        set => Entity.SetIntParameter(DummySensor.MinValueKey, value);
    }

    public int MaxValue
    {
        get => Entity.GetIntParameter(DummySensor.MaxValueKey, 100);
        set => Entity.SetIntParameter(DummySensor.MaxValueKey, value);
    }

    public int MaxRetries
    {
        get => Entity.GetIntParameter(DummySensor.MaxRetriesKey, 100);
        set => Entity.SetIntParameter(DummySensor.MaxRetriesKey, value);
    }

    public DummySensorSettingsViewModel()
    {
    }

    public DummySensorSettingsViewModel(ConfiguredEntity entity) : base(entity)
    {
    }
}