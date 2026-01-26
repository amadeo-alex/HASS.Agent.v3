using System;
using System.Collections.Generic;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HASS.Agent.Contracts.Models.Entity;

namespace HASS.Agent.Client.ViewModels.Dialogs;

public partial class SensorEditDialogViewModel : DialogViewModelBase
{
    private const string AdditionalSettingsBaseNamespace = "HASS.Agent.Client.ViewModels.Sensors";

    [ObservableProperty]
    private EntityCategory _selectedCategory;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(AdditionalSettingsPresent))] [NotifyPropertyChangedFor(nameof(DisplayName))]
    private EntitySettingsViewModelBase? _additionalSettingsViewModel;

    public EntityCategory EntityCategories { get; set; }
    public bool ShowEntityCategories { get; set; }

    public ConfiguredEntity Sensor { get; set; }

    public string DisplayName
    {
        get => Sensor.Type; //TODO(Amadeo): provide proper translated name
    }

    public string Description { get; set; } = "Some Description";

    public bool AdditionalSettingsPresent => AdditionalSettingsViewModel != null;

    public SensorEditDialogViewModel()
    {
    }

    public SensorEditDialogViewModel(ConfiguredEntity sensor)
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

        Sensor.Type = value.Name;
        EvaluateAdditionalSettingsViewModel();
    }
}