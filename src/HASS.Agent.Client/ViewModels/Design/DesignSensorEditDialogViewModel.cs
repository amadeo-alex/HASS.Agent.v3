using System;
using HASS.Agent.Base.Sensors.SingleValue;
using HASS.Agent.Client.ViewModels.Dialogs;
using HASS.Agent.Contracts.Models.Entity;

namespace HASS.Agent.Client.ViewModels.Design;

public class DesignSensorEditDialogViewModel : SensorEditDialogViewModel
{
    public DesignSensorEditDialogViewModel()
    {
        EntityCategories = new EntityCategory("Design/Entity/Category/Dummies", typeof(DummySensor));
        Sensor = new ConfiguredEntity()
        {
            Type = typeof(DummySensor).Name,
            EntityIdName = "DesignDummySensor",
            Name = "Design Dummy Sensor",
            UpdateIntervalSeconds = 5,
            UniqueId = Guid.NewGuid(),
            Active = true,
        };
    }
}