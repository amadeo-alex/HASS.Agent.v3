using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.Contracts.Managers;
using Microsoft.Extensions.Logging;

namespace HASS.Agent.Base.Managers;
public class FileStorageManager : IFileStorageManager
{
    private readonly ILogger _logger;

    private readonly ISettingsManager _settingsManager;

    public FileStorageManager(ILogger<FileStorageManager> logger, ISettingsManager settingsManager)
    {
        _logger = logger;
        _settingsManager = settingsManager;       
    }

    public async Task<string> GetFile(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            _logger.LogWarning("[FILESTORAGE] Received empty file path");
            return string.Empty;
        }

        if (uri.ToLower().StartsWith("file://"))
        {
            _logger.LogInformation("[FILESTORAGE] Received 'file://' type URI, returning as provided");
            return uri;
        }

        if (!uri.ToLower().StartsWith("http"))
        {
            _logger.LogError("[FILESTORAGE] Unsupported URI, only 'http/s' and 'file://' URIs are allowed, got: {uri}", uri);
            return string.Empty;
        }
/*
        if (!Directory.Exists(_settingsManager.ApplicationSettings.))
            Directory.CreateDirectory(Variables.ImageCachePath);*/

        return "";
    }
}
