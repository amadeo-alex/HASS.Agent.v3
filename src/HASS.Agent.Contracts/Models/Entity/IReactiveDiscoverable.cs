namespace HASS.Agent.Contracts.Models.Entity;

public interface IReactiveDiscoverable
{
	event Func<AbstractDiscoverable ,Task> NewStateDetectedAsync;
}