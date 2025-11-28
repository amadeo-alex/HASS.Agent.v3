using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models.Notifications;

namespace HASS.Agent.Base.Linux.Managers;
internal class NotificationManager : INotificationManager
{
    public bool Ready => true;

    public Task HandleAppActivation(object activationArgumentsData)
    {
        return Task.CompletedTask;
    }

    public void Initialize()
    {

    }

    public void RegisterNotificationActionHandler(string handlerId, INotificationActionHandler handler)
    {

    }

    public Task ShowNotification(Notification notification)
    {
        return Task.CompletedTask;
    }

    public void UnregisterNotificationActionHandler(string handlerId)
    {

    }
}
