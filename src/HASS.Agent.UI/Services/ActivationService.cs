﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.UI.Contracts.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using HASS.Agent.UI.Views.Pages;
using HASS.Agent.UI.Activation;
using HASS.Agent.UI.Contracts;

namespace HASS.Agent.UI.Services;
public class ActivationService : IActivationService
{
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private UIElement? _shell = null;

    public bool HandleClosedEvents { get; set; }

    public ActivationService(ActivationHandler<LaunchActivatedEventArgs> defaultHandler, IEnumerable<IActivationHandler> activationHandlers)
    {
        _defaultHandler = defaultHandler;
        _activationHandlers = activationHandlers;
    }

    public async Task ActivateAsync(object activationArgs)
    {
        // Execute tasks before activation.
        await InitializeAsync();

        // Set the MainWindow Content.
        if (App.MainWindow.Content == null)
        {
            _shell = App.GetService<ShellPage>();
            App.MainWindow.Content = _shell ?? new Frame();
        }

        // Handle activation via ActivationHandlers.
        await HandleActivationAsync(activationArgs);

        App.MainWindow.Closed += (sender, args) =>
         {
             if (HandleClosedEvents)
             {
                 args.Handled = true;
                 App.MainWindow.Hide();
             }
         };

        // Activate the MainWindow.
        App.MainWindow.Activate();

        // Execute tasks after activation.
        await StartupAsync();
    }

    private async Task HandleActivationAsync(object activationArgs)
    {
        var activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));
        if (activationHandler != null)
        {
            await activationHandler.HandleAsync(activationArgs);
        }

        if (_defaultHandler.CanHandle(activationArgs))
        {
            await _defaultHandler.HandleAsync(activationArgs);
        }
    }

    private async Task InitializeAsync()
    {
        //await _themeSelectorService.InitializeAsync().ConfigureAwait(false); //TODO(Amadeo): cleanup
        //await Task.CompletedTask;
    }

    private async Task StartupAsync()
    {
        //await _themeSelectorService.SetRequestedThemeAsync(); //TODO(Amadeo): cleanup
        //await Task.CompletedTask;
    }

    public void Shutdown()
    {

    }
}
