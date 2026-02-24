using HASS.Agent.Contracts.Models.Mqtt;

namespace HASS.Agent.Client.ViewModels;

public class ClientConfigModelViewModel : ViewModelBase
{
    public ClientConfigModel Config { get; set; } = new();
    public bool Duplicate { get; set; } = false;
}