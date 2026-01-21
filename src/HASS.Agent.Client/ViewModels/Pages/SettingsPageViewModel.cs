using System.Collections.Generic;
using Avalonia.Threading;

namespace HASS.Agent.Client.ViewModels.Pages;

public class SettingsPageViewModel : ViewModelBase, INavigationAware
{
    public List<AnchorItemViewModel> SettingsSections { get; }
    
    public SettingsPageViewModel(Dispatcher dispatcher) : base(dispatcher)
    {
        SettingsSections =
        [
            new AnchorItemViewModel() { AnchorId = "sectionGeneral", Header = "General" },
            new AnchorItemViewModel() { AnchorId = "sectionSecurity", Header = "Security" },
            new AnchorItemViewModel() { AnchorId = "sectionMqtt", Header = "MQTT" },
            new AnchorItemViewModel() { AnchorId = "sectionHA", Header = "Home Assistant" },
            new AnchorItemViewModel() { AnchorId = "sectionNotifications", Header = "Notifications" }
        ];
    }

    public void OnNavigatedTo()
    {
        
    }

    public void OnNavigatedFrom()
    {
        
    }
}