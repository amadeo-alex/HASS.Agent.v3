using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using HASS.Agent.Base;
using HASS.Agent.Base.Models;
using HASS.Agent.Client.ViewModels;
using HASS.Agent.Client.Views;
using HASS.Agent.Contracts.Models.Update;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HASS.Agent.Client;

public partial class App : Application
{
    private readonly ILogger _logger;
    private readonly HASSAgentBase _applicationBase;

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

    public App()
    {
        _applicationBase = new HASSAgentBase();

        Host = _applicationBase.Initialize(Serilog.Events.LogEventLevel.Debug, (context, services) =>
        {
            services.AddSingleton(sp =>
            {
                var informationalVersion = Assembly.GetExecutingAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? throw new Exception("cannot obtain application version");
                var versionString = informationalVersion.Contains('+') ? informationalVersion.Split('+')[0] : informationalVersion;

                return new ApplicationInfo()
                {
                    Name = Assembly.GetExecutingAssembly().GetName().Name ?? "HASS.Agent",
                    Version = new AgentVersion(versionString),
                    ExecutablePath = AppDomain.CurrentDomain.BaseDirectory,
                    Executable = Process.GetCurrentProcess().MainModule?.ModuleName ?? throw new Exception("cannot obtain application executable"),
                };
            });
        });

        _logger = Host.Services.GetRequiredService<ILogger<App>>(); //TODO(Amadeo): fix this
        _logger.LogInformation("App class constructed");
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
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