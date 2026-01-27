using System;
using System.Collections.Generic;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models.Entity;

namespace HASS.Agent.Client.ViewModels.Dialogs;

public partial class SensorEditDialogViewModel : DialogViewModelBase //TODO(Amadeo): ugly
{
    private const string AdditionalSettingsBaseNamespace = "HASS.Agent.Client.ViewModels.Sensors";

    private readonly IEntityTypeRegistry? _entityTypeRegistry;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Sensor))]
    private EntityCategory? _selectedCategory;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AdditionalSettingsPresent))]
    private EntitySettingsViewModelBase? _additionalSettingsViewModel;

    public EntityCategory EntityCategories { get; set; }
    public bool ShowEntityCategories { get; set; }

    public ConfiguredEntity Sensor { get; set; } = new();

    public string Description { get; set; } = "Some Description";

    public bool AdditionalSettingsPresent => AdditionalSettingsViewModel != null;

    public SensorEditDialogViewModel(IEntityTypeRegistry? entityTypeRegistry = null)
    {
        _entityTypeRegistry = entityTypeRegistry;

        EntityCategories = _entityTypeRegistry != null ? _entityTypeRegistry.SensorsCategories : new EntityCategory("blank", null);
    }

    public SensorEditDialogViewModel(IEntityTypeRegistry? entityTypeRegistry, ConfiguredEntity sensor) : this(entityTypeRegistry)
    {
        Sensor = sensor;
        EvaluateAdditionalSettingsViewModel();
    }

    private void EvaluateAdditionalSettingsViewModel()
    {
        //TODO(Amadeo): have a nice thought if this is not too ugly/cheeky?
        var additionalSettingsViewModelType = Type.GetType($"{AdditionalSettingsBaseNamespace}.{Sensor.Type}SettingsViewModel");
        if (additionalSettingsViewModelType == null)
        {
            return;
        }

        AdditionalSettingsViewModel = Activator.CreateInstance(additionalSettingsViewModelType) as EntitySettingsViewModelBase;
        AdditionalSettingsViewModel?.Entity = Sensor;
    }

    partial void OnSelectedCategoryChanged(EntityCategory? value)
    {
        if (value == null)
        {
            return;
        }

        var newSensor = _entityTypeRegistry?.GetDefaultConfiguration(value.Name);
        if (newSensor != null)
        {
            newSensor.UniqueId = Sensor.UniqueId;
            Sensor = newSensor;
        }

        EvaluateAdditionalSettingsViewModel();
    }
}