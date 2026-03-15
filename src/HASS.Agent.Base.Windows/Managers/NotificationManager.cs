using System.Net;
using System.Text;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models;
using HASS.Agent.Contracts.Models.Notifications;
using HASS.Agent.Contracts.Models.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using MQTTnet;
using Newtonsoft.Json;

namespace HASS.Agent.Base.Windows.Managers;

public class NotificationManager : INotificationManager, IMqttMessageHandler
{
    private const string ActionKey = "action";
    private const string UriKey = "uri";
    private const string ClickActionKey = "clickAction";

    private const string ActionPrefix = $"{ActionKey}=";
    private const string UriPrefix = $"{UriKey}=";
    private const string ClickActionPrefix = $"{ClickActionKey}=";

    private const string SpecialClear = "clear_notification";
    private const string HomeAssistantNotificationEvent = "hass_agent_notifications";

    public const string NotificationLaunchArgument = "----AppNotificationActivated:";

    private readonly ILogger _logger;

    private readonly ISettingsManager _settingsManager;
    private readonly IMqttManager _mqttManager;
    private readonly IHomeAssistantApiManager _homeAssistantApiManager;

    private NotificationSettings _notificationSettingsSnapshot;
    private ApplicationSettings _applicationSettingsSnapshot;
    private MqttSettings _mqttSettingsSnapshot;
    
    private readonly AppNotificationManager _notificationManager = AppNotificationManager.Default;

    private readonly Dictionary<string, INotificationActionHandler> _notificationActionHandlers = [];

    public bool Ready { get; private set; }

    public NotificationManager(ILogger<NotificationManager> logger, ISettingsManager settingsManager, IMqttManager mqttManager, IHomeAssistantApiManager homeAssistantApiManager)
    {
        _logger = logger;

        _settingsManager = settingsManager;
        _mqttManager = mqttManager;
        _homeAssistantApiManager = homeAssistantApiManager;

        _notificationSettingsSnapshot = settingsManager.GetSettingsSnapshot<NotificationSettings>();
        _applicationSettingsSnapshot = settingsManager.GetSettingsSnapshot<ApplicationSettings>(); //TODO(Amadeo): refresh on changes
        _mqttSettingsSnapshot = settingsManager.GetSettingsSnapshot<MqttSettings>();
    }

    public void Initialize()
    {
        try
        {
            if (!_notificationSettingsSnapshot.Enabled)
            {
                _logger.LogInformation("[NOTIFICATIONS] Disabled");
                return;
            }

            if (!_applicationSettingsSnapshot.LocalApiEnabled && !_mqttSettingsSnapshot.Enabled)
            {
                _logger.LogWarning("[NOTIFICATIONS] Both local API and MQTT are disabled, unable to receive notifications");
                return;
            }

            if (_mqttSettingsSnapshot.Enabled)
                _mqttManager.RegisterMessageHandler($"hass.agent/notifications/{_applicationSettingsSnapshot.DeviceName}", this);
            else
                _logger.LogWarning("[NOTIFICATIONS] MQTT is disabled, not all aspects of actions might work as expected");

            if (_notificationManager.Setting != AppNotificationSetting.Enabled)
                _logger.LogWarning("[NOTIFICATIONS] Showing notifications might fail, reason: {r}", _notificationManager.Setting.ToString());


            _notificationManager.NotificationInvoked += NotificationManager_NotificationInvoked;

            _notificationManager.Register();
            Ready = true;

            _logger.LogInformation("[NOTIFICATIONS] Ready");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[NOTIFICATIONS] Error while initializing: {err}", ex.Message);
        }
    }

    private async void NotificationManager_NotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args) => await HandleNotificationInvoked(args);


    private async Task HandleNotificationInvoked(AppNotificationActivatedEventArgs args)
    {
        try
        {
            var action = GetValueFromEventArgs(args, ActionPrefix);
            var input = GetInputFromEventArgs(args);
            var uri = GetValueFromEventArgs(args, UriPrefix);
            var clickAction = GetValueFromEventArgs(args, ClickActionPrefix);

            if (!string.IsNullOrWhiteSpace(uri))
                //BrowserHelper.OpenUrl(uri);
                throw new NotImplementedException();
            else if (!string.IsNullOrWhiteSpace(clickAction))
                //BrowserHelper.OpenUrl(clickAction);
                throw new NotImplementedException();

            await _homeAssistantApiManager.FireEventAsync(HomeAssistantNotificationEvent, new
            {
                device_name = _applicationSettingsSnapshot.DeviceName,
                action,
                input,
                uri
            });

        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[NOTIFICATIONS] Unable to process notification action: {err}", ex.Message);
        }
    }

    private string GetValueFromEventArgs(AppNotificationActivatedEventArgs args, string startText)
    {
        var start = args.Argument.IndexOf(startText) + startText.Length;
        if (start < startText.Length)
            return string.Empty;

        var separatorIndex = args.Argument.IndexOf(";", start);
        var end = separatorIndex < 0 ? args.Argument.Length : separatorIndex;
        return DecodeNotificationParameter(args.Argument[start..end]);
    }

    private static IDictionary<string, string> GetInputFromEventArgs(AppNotificationActivatedEventArgs args)
    {
        return args.UserInput.Count > 0 ? args.UserInput : new Dictionary<string, string>();
    }

    private static string EncodeNotificationParameter(string parameter)
    {
        var encodedParameter = Convert.ToBase64String(Encoding.UTF8.GetBytes(parameter));
        // for some reason, Windows App SDK URL encodes the arguments even if they are already encoded
        // this is the reason the WebUtility.UrlEncode is missing from here
        return encodedParameter;
    }

    private static string DecodeNotificationParameter(string encodedParameter)
    {
        var urlDecodedParameter = WebUtility.UrlDecode(encodedParameter);
        return Encoding.UTF8.GetString(Convert.FromBase64String(urlDecodedParameter));
    }

    public async Task ShowNotification(Notification notification)
    {
        if (!Ready)
            throw new Exception("NotificationManager is not initialized");

        try
        {
            if (!_notificationSettingsSnapshot.Enabled)
                return;

            var toastBuilder = new AppNotificationBuilder()
                .AddText(notification.Title)
                .AddText(notification.Message);

            if (notification.Data.ClickAction != NotificationData.NoAction)
                toastBuilder.AddArgument(ClickActionKey, EncodeNotificationParameter(notification.Data.ClickAction));

            //TODO(Amadeo): add configuration for optional hero image
            //TODO(Amadeo): add option to disable caching of downloaded files
            //TODO(Amadeo): finish storage manager and use it to retrieve the image
            if (!string.IsNullOrEmpty(notification.Data.Image))
                toastBuilder.SetInlineImage(new Uri(notification.Data.Image));

            if (notification.Data.Actions.Count > 0)
            {
                foreach (var action in notification.Data.Actions)
                {
                    if (string.IsNullOrEmpty(action.Action))
                        continue;

                    var button = new AppNotificationButton(action.Title)
                        .AddArgument(ActionKey, EncodeNotificationParameter(action.Action));

                    if (!string.IsNullOrWhiteSpace(action.Uri))
                        button.AddArgument(UriKey, EncodeNotificationParameter(action.Uri));

                    toastBuilder.AddButton(button);
                }
            }

            if (notification.Data.Inputs.Count > 0)
            {
                foreach (var input in notification.Data.Inputs)
                {
                    if (string.IsNullOrEmpty(input.Id))
                        continue;

                    toastBuilder.AddTextBox(input.Id, input.Text, input.Title);
                }
            }

            if (!string.IsNullOrWhiteSpace(notification.Data.Group))
                toastBuilder.SetGroup(notification.Data.Group);
            if (!string.IsNullOrWhiteSpace(notification.Data.Tag))
                toastBuilder.SetTag(notification.Data.Tag);

            if (notification.Data.Sticky)
            {
                toastBuilder.SetScenario(AppNotificationScenario.Reminder);
                if (notification.Data.Actions.Count == 0)
                    toastBuilder.AddButton(new AppNotificationButton("Dismiss FIXME")); //Note(Amadeo): required for reminder scenario
            }                           //TODO(Amadeo): fix hardcoded string

            if (AppNotificationBuilder.IsUrgentScenarioSupported() && notification.Data.Importance == NotificationData.ImportanceHigh)
            {
                toastBuilder.SetScenario(AppNotificationScenario.Urgent);
                if (notification.Data.Sticky)
                    _logger.LogWarning("[NOTIFICATIONS] Notification importance overrides sticky", notification.Title);
            }

            if (!string.IsNullOrWhiteSpace(notification.Data.IconUrl))
                toastBuilder.SetAppLogoOverride(new Uri(notification.Data.IconUrl));

            var toast = toastBuilder.BuildNotification();

            if (notification.Data.Duration > 0)
                toast.Expiration = DateTime.Now.AddSeconds(notification.Data.Duration);

            _notificationManager.Show(toast);

            if (toast.Id == 0)
                _logger.LogError("[NOTIFICATIONS] Notification '{err}' failed to show", notification.Title);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[NOTIFICATIONS] Error while showing notification: {err}\r\n{json}", ex.Message, JsonConvert.SerializeObject(notification, Formatting.Indented));
        }
    }

    public void RegisterNotificationActionHandler(string handlerId, INotificationActionHandler handler)
    {
        if (_notificationActionHandlers.ContainsKey(handlerId))
            throw new ArgumentException($"handler with id {handlerId} already registered");

        _notificationActionHandlers[handlerId] = handler;
    }

    public void UnregisterNotificationActionHandler(string handlerId)
    {
        _notificationActionHandlers.Remove(handlerId);
    }

    public async Task HandleAppActivation(object activationData)
    {
        var appNotificationArgs = activationData as AppNotificationActivatedEventArgs;
        if (appNotificationArgs == null || appNotificationArgs.Argument == null)
            return;

        await HandleNotificationInvoked(appNotificationArgs);
    }

    public async Task HandleMqttMessage(MqttApplicationMessage message)
    {
        try
        {
            var notification = JsonConvert.DeserializeObject<Notification>(Encoding.UTF8.GetString(message.Payload));
            if (notification == null)
                return;

            if (notification.Message == SpecialClear) //NOTE(Amadeo): consider gathering all "groups" of notifications
            {                                         // with given tag and then removing them sequentially.
                if (!string.IsNullOrWhiteSpace(notification.Data.Tag) && !string.IsNullOrWhiteSpace(notification.Data.Group))
                    await _notificationManager.RemoveByTagAndGroupAsync(notification.Data.Tag, notification.Data.Group);
                else if (!string.IsNullOrWhiteSpace(notification.Data.Tag))
                    await _notificationManager.RemoveByTagAsync(notification.Data.Tag);

                return;
            }

            await ShowNotification(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError("[NOTIFICATIONS] Error handling MQTT notification: {msg}", ex.Message);
        }

    }
}
