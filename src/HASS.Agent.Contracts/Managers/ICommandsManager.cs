using HASS.Agent.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.Contracts.Models.Entity;
using MQTTnet;
using System.ComponentModel;

namespace HASS.Agent.Contracts.Managers;
public interface ICommandsManager : INotifyPropertyChanged
{
	ManagerStatus Status { get; }
	
    ObservableCollection<AbstractDiscoverable> Commands { get; }

    bool Pause { get; set; }
    bool Exit { get; set; }

    Task InitializeAsync();
    Task PublishCommandsDiscoveryAsync(bool force);
    Task UnpublishCommandsDiscoveryAsync();
    Task PublishCommandsStateAsync();
    Task Process();
    void ResetAllCommandsChecks();
}
