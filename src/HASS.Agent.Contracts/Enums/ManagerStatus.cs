namespace HASS.Agent.Contracts.Enums;

public enum ManagerStatus
{
	NotInitialized,
	Initializing,
	Initialized,
	
	Connecting,
	Connected,
	
	Running,
	Paused,
	
	Disconnecting,
	Disconnected,
	
	Stopping,
	Stopped,
	
	Error,
}