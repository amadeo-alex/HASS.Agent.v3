// Ignore Spelling: Dpi Mqtt

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.Contracts.Models;
using HASS.Agent.Contracts.Models.Entity;
using Microsoft.Extensions.Configuration;

namespace HASS.Agent.Contracts.Managers;

public interface ISettingsManager
{
	ObservableCollection<ConfiguredEntity> ConfiguredSensors { get; }
	ObservableCollection<ConfiguredEntity> ConfiguredCommands { get; }
	ObservableCollection<IQuickAction> ConfiguredQuickActions { get; } //TODO(Amadeo): rethink

	public T GetSettings<T>() where T : new();
	
	bool SaveSettings<T>(T settings) where T : notnull, new();
	
	bool SaveConfiguredSensors();
	bool SaveConfiguredCommands();
	bool SaveConfiguredQuickActions();
}