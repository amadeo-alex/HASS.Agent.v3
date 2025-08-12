using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.Base.Models;
using HASS.Agent.Contracts.Managers;
using HASS.Agent.Contracts.Models.Update;
using Microsoft.Extensions.Logging;
using Octokit;

namespace HASS.Agent.Base.Managers;
public class UpdateManager : IUpdateManager
{
    private readonly ILogger _logger;
    private readonly ISettingsManager _settingsManager;

    public event EventHandler<UpdateAvailableEventArgs>? UpdateAvailableEventHandler; //TODO(Amadeo): proper async approach?

    public UpdateManager(ILogger<UpdateManager> logger, ISettingsManager settingsManager)
    {
        _logger = logger;
        _settingsManager = settingsManager;
    }
    public Task InitializeAsync()
    {
        if (!_settingsManager.Settings.Update.PeriodicUpdateCheckEnabled)
        {
            _logger.LogInformation("[Update] Periodic update check have been disabled");
            return Task.CompletedTask;
        }

        _ = Task.Run(PeriodicUpdateCheckAsync);

        return Task.CompletedTask;
    }

    public async Task<ReleaseInformation> GetLatestReleaseAsync()
    {
        var ghClient = new GitHubClient(new ProductHeaderValue("HASS.Agent"));
        var latestRelease = await ghClient.Repository.Release.GetLatest("hass-agent", "HASS.Agent"); //TODO(Amadeo): change to proper repo

        return new ReleaseInformation(latestRelease);
    }

    private async Task<bool> CheckForUpdateAsync()
    {
        var latestRelease = await GetLatestReleaseAsync();
        if (latestRelease.Version.Tag != AgentVersion.BetaTag || !string.IsNullOrWhiteSpace(latestRelease.Version.Tag))
        {
            return false;
        }

        if (_settingsManager.Settings.Update.IgnoredVersions.Contains(latestRelease.Version.ToString()))
        {
            return false;
        }

        if (latestRelease.Version.IsBeta && !_settingsManager.Settings.Update.ShowBetaUpdates)
        {
            return false;
        }

        UpdateAvailableEventHandler?.Invoke(this, new UpdateAvailableEventArgs
        {
            Release = latestRelease
        });

        return true;
    }



    private async Task PeriodicUpdateCheckAsync()
    {
        //Note(Amadeo): initial update check?
        while (true) //TODO(Amadeo): cancellation token?
        {
            await Task.Delay(TimeSpan.FromMinutes(_settingsManager.Settings.Update.PeriodicUpdateIntervalMinutes));
            await CheckForUpdateAsync();
        }
    }
}
