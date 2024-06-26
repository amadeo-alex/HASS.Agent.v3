﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HASS.Agent.Base.Contracts.Models.Mqtt;

namespace HASS.Agent.Base.Contracts.Models.Entity;
public interface IDiscoverable
{
    public string Domain { get; set; }
    public string EntityIdName { get; set; }
    public string Name { get; set; }
    public string TopicName { get; set; }

    public string UniqueId { get; set; }
    public bool UseAttributes { get; set; }
    public bool IgnoreAvailability { get; set; }
    public int UpdateIntervalSeconds { get; set; }
    public DateTime LastUpdated { get; }
    public string PreviousPublishedState { get; }
    public string PreviousPublishedAttributes { get; }

    public abstract Task<string> GetState();
    public abstract Task<string> GetAttributes();

    public AbstractMqttDiscoveryConfigModel ConfigureAutoDiscoveryConfig(string discoveryPrefix, AbstractMqttDeviceConfigModel deviceConfigModel);
    public AbstractMqttDiscoveryConfigModel? GetAutoDiscoveryConfig();
    //public void ClearAutoDiscoveryConfig();
    public void ResetChecks();
}
