using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.UI.Contracts.ViewModels;
using Microsoft.UI.Dispatching;

namespace HASS.Agent.UI.ViewModels.Settings;
public class NotificationSettingsPageViewModel : ViewModelBase, INavigationAware
{
	public ISettingsManager SettingsManager { get; private set; }

	public NotificationSettingsPageViewModel(DispatcherQueue dispatcherQueue, ISettingsManager settingsManager) : base(dispatcherQueue)
    {
        SettingsManager = settingsManager;
	}

    public void OnNavigatedFrom()
    {
    }
    public void OnNavigatedTo()
    {
    }
}
