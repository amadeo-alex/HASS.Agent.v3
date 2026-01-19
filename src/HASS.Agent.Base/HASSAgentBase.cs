using System.Diagnostics;
using System.Reflection;
using HASS.Agent.Base.Helpers;
using HASS.Agent.Base.Managers;
using HASS.Agent.Contracts.Managers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using HASS.Agent.Base.Managers.HomeAssistant;
using HASS.Agent.Base.Models;
using HASS.Agent.Contracts.Models.Update;


#if WINDOWS
using HASS.Agent.Base.Windows.Managers;
#else
using HASS.Agent.Base.Linux.Managers;
#endif


namespace HASS.Agent.Base;

public class HassAgentBase
{
    public IHost? _host;

    public bool Debug { get; private set; } = false;

    public IHost Initialize(LogEventLevel logEventLevel,
        Action<HostBuilderContext, IServiceCollection> externalServicesPreInitializer,
        Action<HostBuilderContext, IServiceCollection> externalServicesPostInitializer,
        Action<LoggerConfiguration, IServiceProvider>? additionalLoggerConfiguration = null
    )
    {
        Debug = logEventLevel < LogEventLevel.Information;

        _host = Host.CreateDefaultBuilder().UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices((context, services) =>
            {
                externalServicesPreInitializer(context, services);

                services.AddSingleton(_ => new LoggingLevelSwitch
                {
                    MinimumLevel = logEventLevel
                });

                services.AddSerilog((sp, loggerConfiguration) =>
                {
                    var arguments = Environment.GetCommandLineArgs();
                    var logTag = arguments.Length > 1
                        ? $"{arguments.First(x => !string.IsNullOrEmpty(x)).RemoveNonAlphanumericCharacters()}_"
                        : string.Empty;

                    var elevationManager = sp.GetRequiredService<IElevationManager>();
                    var elevatedTag = elevationManager.RunningElevated ? "[E]" : "";

                    var applicationName = sp.GetRequiredService<ApplicationInfo>().Name;
                    var logName = $"[{DateTime.Now:yyyy-MM-dd}]{elevatedTag} {applicationName}_{logTag}.log";

                    var variableManager = sp.GetRequiredService<IVariableManager>();

                    loggerConfiguration.MinimumLevel.ControlledBy(sp.GetRequiredService<LoggingLevelSwitch>())
                        .WriteTo.Async(a =>
                            a.File(Path.Combine(variableManager.LogPath, logName),
                                rollingInterval: RollingInterval.Day,
                                fileSizeLimitBytes: 10000000,
                                retainedFileCountLimit: 10,
                                rollOnFileSizeLimit: true,
                                buffered: true,
                                flushToDiskInterval: TimeSpan.FromMilliseconds(150)));
                    
                    additionalLoggerConfiguration?.Invoke(loggerConfiguration, sp);
                });

                services.AddSingleton<IExceptionManager, ExceptionManager>();

                services.AddSingleton<IGuidManager, GuidManager>();

#if WINDOWS
                services.AddSingleton<IElevationManager, HASS.Agent.Base.Windows.Managers.ElevationManager>();
#else
                services.AddSingleton<IElevationManager, HASS.Agent.Base.Linux.Managers.ElevationManager>();
#endif

                services.AddSingleton<IVariableManager, VariableManager>();

                services.AddSingleton<IGuidManager, GuidManager>();
                services.AddSingleton<ISettingsManager, SettingsManager>();

                services.AddSingleton<IMqttManager, MqttManager>();

                services.AddSingleton<IEntityTypeRegistry, EntityTypeRegistry>();
                services.AddSingleton<ISensorManager, SensorManager>();
                services.AddSingleton<ICommandsManager, CommandsManager>();

                services.AddSingleton<IHomeAssistantApiManager, HomeAssistantApiManager>();

#if WINDOWS
                services.AddSingleton<INotificationManager, HASS.Agent.Base.Windows.Managers.NotificationManager>();
#else
                services.AddSingleton<INotificationManager, HASS.Agent.Base.Linux.Managers.NotificationManager>();
#endif

                externalServicesPostInitializer(context, services);

                services.AddSingleton((sp) =>
                {
                    var settingsManager = sp.GetRequiredService<ISettingsManager>();
                    return settingsManager.GetConfiguration();
                });


                services.AddSingleton(sp => sp);
            }).Build();

        return _host;
    }

    public T GetService<T>() where T : class
    {
        if (_host == null)
        {
            throw new InvalidOperationException("HASS.Agent Base is not initialized!");
        }

        return _host.Services.GetService(typeof(T)) is not T service
            ? throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.")
            : service;
    }

    public object GetService(Type type)
    {
        if (_host == null)
        {
            throw new InvalidOperationException("HASS.Agent Base is not initialized!");
        }

        var service = _host.Services.GetService(type);
        return service ?? throw new ArgumentException($"{type} needs to be registered in ConfigureServices within App.xaml.cs.");
    }
}