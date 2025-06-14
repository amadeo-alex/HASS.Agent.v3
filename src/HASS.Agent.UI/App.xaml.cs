using System;
using HASS.Agent.UI.Contracts.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using HASS.Agent.UI.Services;
using HASS.Agent.UI.Activation;
using HASS.Agent.UI.ViewModels;
using HASS.Agent.UI.Views.Pages;
using HASS.Agent.Base.Models;
using HASS.Agent.Base.Managers;
using Serilog.Events;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using MQTTnet;
using HASS.Agent.Base.Sensors.SingleValue;
using System.Globalization;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using System.Xml.Linq;
using System.Xml;
using Windows.Storage;
using WinUI3Localizer;
using System.IO;
using HASS.Agent.UI.Contracts.Managers;
using HASS.Agent.UI.Managers;
using HASS.Agent.UI.Views.Pages.Settings;
using HASS.Agent.UI.Contracts.ViewModels;
using HASS.Agent.UI.ViewModels.Settings;
using H.NotifyIcon.Core;
using H.NotifyIcon;
using HASS.Agent.UI.Contracts;
using HASS.Agent.Base.Managers.HomeAssistant;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Helpers;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HASS.Agent.UI;
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private readonly Microsoft.Extensions.Logging.ILogger _logger;

    public bool DEBUG = false;

    public IHost Host
    {
        get; private set;
    }

    public static T GetService<T>() where T : class
    {
        return (Current as App)!.Host.Services.GetService(typeof(T)) is not T service
            ? throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.")
            : service;
    }

    public static object GetService(Type type)
    {
        var service = (Current as App)!.Host.Services.GetService(type);
        return service ?? throw new ArgumentException($"{type} needs to be registered in ConfigureServices within App.xaml.cs.");
    }

    public T GetAgentService<T>() where T : class => GetService<T>();
    public object GetAgentService(Type type) => GetService(type);

    public static UIElement? AppTitlebar
    {
        get; set;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
#if DEBUG
        DEBUG = true; //TODO(Amadeo): find better way to do this?
#endif

        Host = ConfigureServices();

        _logger = GetService<ILogger<App>>();

        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected async override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);
        await InitializeLocalizer();

        try
        {
            var launchArguments = Environment.GetCommandLineArgs();
            var settingsManager = GetService<ISettingsManager>();

            if (DEBUG)
            {
                _logger.LogInformation("[MAIN] DEBUG BUILD - TESTING PURPOSES ONLY");
                settingsManager.Settings.Application.ExtendedLogging = true;
            }

            if (settingsManager.Settings.Application.ExtendedLogging)
            {
                GetService<LoggingLevelSwitch>().MinimumLevel = LogEventLevel.Debug;
                _logger.LogDebug("[MAIN] Extended logging enabled");
                _logger.LogDebug("[MAIN] Started with arguments: {a}", launchArguments);

                var exceptionManager = GetService<IExceptionManager>();
                AppDomain.CurrentDomain.FirstChanceException += exceptionManager.OnFirstChanceExceptionHandler;
            }

            var variableManager = GetService<IVariableManager>();
            _logger.LogInformation("[MAIN] HASS.Agent version: {version}", variableManager.ClientVersion);

            InitializeComponent();

            var initializationTask = Task.Run(async () =>
            {
                var guidManager = GetService<IGuidManager>();
                guidManager.MarkAsUsed(settingsManager.Settings.Mqtt.ClientId);

                if (settingsManager.ConfiguredSensors.Count == 0)
                {
                    var ce = new ConfiguredEntity()
                    {
                        Type = typeof(DummySensor).Name,
                        EntityIdName = "DummySensor1",
                        Name = "Dummy Sensor 1",
                        UpdateIntervalSeconds = 5,
                        UniqueId = Guid.NewGuid(),
                        Active = true,
                    };
                    settingsManager.ConfiguredSensors.Add(ce);
                }

                var mqtt = GetService<IMqttManager>();
                await mqtt.StartClientAsync();

                var haapi = GetService<IHomeAssistantApiManager>();
                await haapi.InitializeAsync();

                var sensorManager = GetService<ISensorManager>();
                await sensorManager.InitializeAsync();
                _ = Task.Run(sensorManager.Process);

                var commandsManager = GetService<ICommandsManager>();
                await commandsManager.InitializeAsync();
                _ = Task.Run(commandsManager.Process);

                var notificationManager = GetService<INotificationManager>();
                notificationManager.Initialize();

                await Task.Run(async () =>
                {
                    while (!mqtt.Ready)
                        await Task.Delay(1000);

                    var testMsg = new MqttApplicationMessageBuilder()
                        .WithTopic("sumtest/sumimportantmsg")
                        .WithPayload("much importando")
                        .WithRetainFlag(false)
                        .Build();

                    await mqtt.PublishAsync(testMsg);
                });

                _logger.LogDebug("[MAIN] initialization completed");
            });
        }
        catch (Exception ex)
        {
            Debugger.Break(); //TODO(Amadeo): do something more intelligent
        }

        _logger.LogDebug("[MAIN] attempting to display main window");
        //App.GetService<IAppNotificationService>().Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));
        await GetService<IActivationService>().ActivateAsync(args);
    }

    private async Task InitializeLocalizer()
    {
        var stringsFolderPath = Path.Combine(AppContext.BaseDirectory, "Strings");
        var stringsFolder = await StorageFolder.GetFolderFromPathAsync(stringsFolderPath);

        var localizer = await new LocalizerBuilder()
            .AddStringResourcesFolderForLanguageDictionaries(stringsFolderPath)
            .SetOptions(options =>
            {
                options.DefaultLanguage = "en-US";
            })
            .Build();
    }

    private PageService GetPageService()
    {
        //TODO(Amadeo): localization
        var menuPages = new Dictionary<IMenuItem, Type?>()
        {
            { new MenuItem { NavigateTo = "main", ViewModelType = typeof(MainPageViewModel), Title = "Main", Glyph = "\uE80F" }, typeof(MainPage) },

            { new MenuItem { Type = MenuItemType.Separator }, null},
            { new MenuItem { Type = MenuItemType.Header, Title = "HASS.Agent" }, null },
            { new MenuItem { NavigateTo = "sensors", ViewModelType = typeof(SensorsPageViewModel), Title = "Sensors", Glyph = "\uE957" }, typeof(SensorsPage) },
            { new MenuItem { NavigateTo = "commands", ViewModelType = typeof(CommandsPageViewModel), Title = "Commands", Glyph = "\uE756" }, typeof(CommandsPage) },

            { new MenuItem { Type = MenuItemType.Separator }, null},
            { new MenuItem { Type = MenuItemType.Header, Title = "Satellite Service" }, null },
            { new MenuItem { NavigateTo = "sensors-sat", ViewModelType = typeof(SensorsPageViewModel), Title = "Sensors", Glyph = "\uE957"}, typeof(SatelliteSensorsPage) },
            { new MenuItem { NavigateTo = "commands-sat", ViewModelType = typeof(CommandsPageViewModel), Title = "Commands", Glyph = "\uE756" }, typeof(SatelliteCommandsPage) },
        };

        var footerPages = new Dictionary<IMenuItem, Type?>()
        {
            { new MenuItem { NavigateTo = "debug", ViewModelType = typeof(DebugPageViewModel), Title = "Debug", Glyph = "\uEBE8" }, typeof(DebugPage) },
            { new MenuItem { NavigateTo = "settings", ViewModelType = typeof(SettingsRootPageViewModel), Title = "Settings", Glyph = "\uE713" }, typeof(SettingsRootPage) }
        };

        return new PageService(menuPages, footerPages);
    }

    private IHost ConfigureServices()
    {
        var uiDispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        var host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder().UseContentRoot(AppContext.BaseDirectory).
            ConfigureServices((context, services) =>
            {
                services.AddSingleton(sp =>
                {
                    return new LoggingLevelSwitch
                    {
                        MinimumLevel = DEBUG ? LogEventLevel.Debug : LogEventLevel.Information
                    };
                });

                services.AddSerilog((sp, loggerConfiguration) =>
                {
                    var arguments = Environment.GetCommandLineArgs();
                    var logTag = arguments.Length > 1
                        ? $"{StringHelper.RemoveNonAlphanumericCharacters(arguments.First(x => !string.IsNullOrEmpty(x)))}_"
                        : string.Empty;

                    var elevationManager = sp.GetRequiredService<IElevationManager>();
                    var elevatedTag = elevationManager.RunningElevated ? "[E]" : "";

                    var variableManager = sp.GetRequiredService<IVariableManager>();
                    var logName = $"[{DateTime.Now:yyyy-MM-dd}]{elevatedTag} {variableManager.ApplicationName}_{logTag}.log";

                    loggerConfiguration.MinimumLevel.ControlledBy(sp.GetRequiredService<LoggingLevelSwitch>())
                        .WriteTo.Async(a =>
                            a.File(Path.Combine(variableManager.LogPath, logName),
                                rollingInterval: RollingInterval.Day,
                                fileSizeLimitBytes: 10000000,
                                retainedFileCountLimit: 10,
                                rollOnFileSizeLimit: true,
                                buffered: true,
                                flushToDiskInterval: TimeSpan.FromMilliseconds(150)));
                });

                services.AddSingleton<IExceptionManager, ExceptionManager>();

                services.AddSingleton<IGuidManager, GuidManager>();

                services.AddSingleton(new ApplicationInfo()
                {
                    Name = Assembly.GetExecutingAssembly().GetName().Name ?? "HASS.Agent",
                    Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? throw new Exception("cannot obtain application version"),
                    ExecutablePath = AppDomain.CurrentDomain.BaseDirectory,
                    Executable = Process.GetCurrentProcess().MainModule?.ModuleName ?? throw new Exception("cannot obtain application executable"),
                });

                services.AddSingleton<IElevationManager, ElevationManager>();

                services.AddSingleton<IVariableManager, VariableManager>();

                services.AddSingleton<IGuidManager, GuidManager>();
                services.AddSingleton<ISettingsManager, SettingsManager>();

                services.AddSingleton<IMqttManager, MqttManager>();

                services.AddSingleton<IEntityTypeRegistry, EntityTypeRegistry>();
                services.AddSingleton<ISensorManager, SensorManager>();
                services.AddSingleton<ICommandsManager, CommandsManager>();

                services.AddSingleton<INotificationManager, NotificationManager>();

                services.AddSingleton<IHomeAssistantApiManager, HomeAssistantApiManager>();

                services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();
                services.AddTransient<IActivationHandler, NotificationActivationHandler>();

                services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();

                services.AddSingleton<IPageService, PageService>(sp =>
                {
                    return GetPageService();
                });
                services.AddSingleton<INavigationViewService, NavigationViewService>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IActivationService, ActivationService>();

                services.AddSingleton<IEntityUiTypeRegistry, EntityUiTypeRegistry>();

                services.AddTransient<ShellPageViewModel>();
                services.AddTransient<ShellPage>();

                services.AddTransient<MainPageViewModel>();
                services.AddTransient<MainPage>();

                services.AddTransient<SensorsPageViewModel>();
                services.AddTransient<SensorsPage>();

                services.AddTransient<CommandsPageViewModel>();
                services.AddTransient<CommandsPage>();

                services.AddTransient<DebugPageViewModel>();
                services.AddTransient<DebugPage>();

                services.AddTransient<GeneralSettingsPage>();
                services.AddTransient<GeneralSettingsPageViewModel>();
                services.AddTransient<MqttSettingsPage>();
                services.AddTransient<MqttSettingsPageViewModel>();
                services.AddTransient<NotificationSettingsPage>();
                services.AddTransient<NotificationSettingsPageViewModel>();

                services.AddTransient<SettingsRootPageViewModel>();
                services.AddTransient<SettingsRootPage>();



                services.AddSingleton(uiDispatcherQueue);

                services.AddSingleton(sp => sp);
            })
            .Build();

        return host;
    }
}
