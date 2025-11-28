using HASS.Agent.Contracts.Models.Notifications;

namespace HASS.Agent.Contracts.Managers;

public interface INotificationActionHandler
{
    Task HandleNotificationAction(Notification notification);
}

public interface INotificationManager
{
    bool Ready { get; }
    void Initialize();
    Task ShowNotification(Notification notification);
    void RegisterNotificationActionHandler(string handlerId, INotificationActionHandler handler);
    void UnregisterNotificationActionHandler(string handlerId);
    Task HandleAppActivation(object activationArgumentsData);
}
