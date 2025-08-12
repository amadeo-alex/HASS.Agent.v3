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

        _ = Task.Run(PeriodicUpdateCheck);

        return Task.CompletedTask;
    }

    public Task<ReleaseInformation> CheckForUpdateAsync() => GetLatestRelease();

    private async Task<ReleaseInformation> GetLatestRelease()
    {
        var ghClient = new GitHubClient(new ProductHeaderValue("HASS.Agent"));
        var latestRelease = await ghClient.Repository.Release.GetLatest("hass-agent", "HASS.Agent"); //TODO(Amadeo): change to proper repo

        return new ReleaseInformation(latestRelease);
    }

    private async void PeriodicUpdateCheck()
    {
        //Note(Amadeo): initial update check?

        while (true) //TODO(Amadeo): cancellation token?
        {
            await Task.Delay(TimeSpan.FromMinutes(_settingsManager.Settings.Update.PeriodicUpdateIntervalMinutes));
        
            var latestRelease = GetLatestRelease();
        }


    }
}
