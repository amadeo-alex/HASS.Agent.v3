using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using HASS.Agent.Base;
using HASS.Agent.Base.Models;
using HASS.Agent.Base.Sensors.SingleValue;
using HASS.Agent.Client.Models.Log;
using HASS.Agent.Client.ViewModels;
using HASS.Agent.Client.Views;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models;
using HASS.Agent.Contracts.Models.Entity;
using HASS.Agent.Contracts.Models.Settings;
using HASS.Agent.Contracts.Models.Update;
using LogViewer.Core;
using LogViewer.Core.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace HASS.Agent.Client;

public partial class App : Application
{
    private readonly HassAgentBase _applicationBase;
    private readonly ILogger _logger;


    public App()
    {
        _applicationBase = new HassAgentBase();

        _applicationBase.Initialize(LogEventLevel.Debug, ExternalServicesPreInitializer, ExternalServicesPostInitializer, AdditionalLoggerConfiguration);

        _logger = _applicationBase.GetService<ILogger<App>>();
        _logger.LogDebug("Application class constructed");
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ExternalServicesPreInitializer(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var informationalVersion =
                Assembly.GetExecutingAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ??
                throw new Exception("cannot obtain application version");
            var versionString = informationalVersion.Contains('+') ? informationalVersion.Split('+')[0] : informationalVersion;

            return new ApplicationInfo()
            {
                Name = Assembly.GetExecutingAssembly().GetName().Name ?? "HASS.Agent",
                Version = new AgentVersion(versionString),
                ExecutablePath = AppDomain.CurrentDomain.BaseDirectory,
                Executable = Process.GetCurrentProcess().MainModule?.ModuleName ?? throw new Exception("cannot obtain application executable"),
                OsVersion = Environment.OSVersion.ToString(),
                StartupPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) ?? throw new Exception("cannot get executable path directory name"),
            };
        });

        //TODO(Amadeo): add theme changing logic
        //services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
    }

    private void ExternalServicesPostInitializer(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<DataStoreLoggerConfiguration>(_ =>
        {
            var loggerConfiguration = new DataStoreLoggerConfiguration();
            loggerConfiguration.Colors[LogLevel.Information].Foreground = System.Drawing.Color.Gray;
            loggerConfiguration.Colors[LogLevel.Debug].Foreground = System.Drawing.Color.DarkGray;
            loggerConfiguration.Colors[LogLevel.Warning].Foreground = System.Drawing.Color.Orange;
            loggerConfiguration.Colors[LogLevel.Error].Foreground = System.Drawing.Color.DarkRed;
            loggerConfiguration.Colors[LogLevel.Critical].Foreground = System.Drawing.Color.Red;
            loggerConfiguration.Colors[LogLevel.Trace].Foreground = System.Drawing.Color.Gray;
            loggerConfiguration.MaxLogEntries = 256;
            
            return loggerConfiguration;
        });
        
        services.AddSingleton<ILogDataStore>(sp =>
        {
            var config = sp.GetService<IOptionsMonitor<DataStoreLoggerConfiguration>>();
            if (config == null)
            {
                return new LogDataStore();
            }
            
            var currentConfig = config.CurrentValue;
            return new LogDataStore(currentConfig.MaxLogEntries, currentConfig.DispatcherPriority);
        });

        services.AddSingleton<LogDataStoreSink>();
        
        services.AddSingleton<LogViewerControlViewModel>();
        services.AddSingleton<LoggerWindowViewModel>();
        services.AddSingleton<LoggerWindow>();
    }

    private void AdditionalLoggerConfiguration(LoggerConfiguration config, IServiceProvider sp)
    {
        var dataStoreSink = sp.GetRequiredService<LogDataStoreSink>();
        config.WriteTo.Sink(dataStoreSink);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var launchArguments = Environment.GetCommandLineArgs();
            var settingsManager = _applicationBase.GetService<ISettingsManager>();


            var testZ = settingsManager.GetConfiguration();
            var testY = _applicationBase._host.Services.GetService<IConfiguration>();
            var testX = _applicationBase._host.Services.GetService<IOptions<MqttSettings>>().Value;

            if (_applicationBase.Debug)
            {
                _logger.LogInformation("[MAIN] DEBUG BUILD - TESTING PURPOSES ONLY");
                //settingsManager.Settings.Application.ExtendedLogging = true; //TODO(Amadeo) fix this and saving of settings in general
            }

            if (settingsManager.GetSettings<ApplicationSettings>().ExtendedLogging)
            {
                _applicationBase.GetService<LoggingLevelSwitch>().MinimumLevel = LogEventLevel.Debug;
                _logger.LogDebug("[MAIN] Extended logging enabled");
                _logger.LogDebug("[MAIN] Started with arguments: {a}", launchArguments);

                var exceptionManager = _applicationBase.GetService<IExceptionManager>();
                AppDomain.CurrentDomain.FirstChanceException += exceptionManager.OnFirstChanceExceptionHandler;
            }

            var applicationInfo = _applicationBase.GetService<ApplicationInfo>();
            _logger.LogInformation("[MAIN] HASS.Agent version: '{version}' on '{os}'", applicationInfo.Version, applicationInfo.OsVersion);


            var initializationTask = Task.Run(async () =>
            {
                var guidManager = _applicationBase.GetService<IGuidManager>();
                guidManager.MarkAsUsed(settingsManager.GetSettings<MqttSettings>().ClientId);

                if (settingsManager.ConfiguredSensors.Count == 0)
                {
                    var ce = new ConfiguredEntity()
                    {
                        Type = nameof(DummySensor),
                        EntityIdName = "DummySensor1",
                        Name = "Dummy Sensor 1",
                        UpdateIntervalSeconds = 5,
                        UniqueId = Guid.NewGuid(),
                        Active = true,
                    };
                    settingsManager.ConfiguredSensors.Add(ce);
                }

                var mqtt = _applicationBase.GetService<IMqttManager>();
                await mqtt.StartClientAsync();

                var haapi = _applicationBase.GetService<IHomeAssistantApiManager>();
                await haapi.InitializeAsync();

                var sensorManager = _applicationBase.GetService<ISensorManager>();
                await sensorManager.InitializeAsync();
                _ = Task.Run(sensorManager.Process);

                var commandsManager = _applicationBase.GetService<ICommandsManager>();
                await commandsManager.InitializeAsync();
                _ = Task.Run(commandsManager.Process);

                var notificationManager = _applicationBase.GetService<INotificationManager>();
                notificationManager.Initialize();

                await Task.Run(async () =>
                {
                    while (!mqtt.Ready)
                    {
                        await Task.Delay(1000);
                    }

                    var testMsg = new MqttApplicationMessageBuilder()
                        .WithTopic("sumtest/sumimportantmsg")
                        .WithPayload("much importando")
                        .WithRetainFlag(false)
                        .Build();

                    await mqtt.PublishAsync(testMsg);
                });

                _logger.LogDebug("[MAIN] initialization completed");
            });

            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };

            Task.Run(async () =>
            {
                await Task.Delay(1000);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var lw = _applicationBase.GetService<LoggerWindow>();
                    lw.Show();
                    lw.Focus();
                });
            });
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}