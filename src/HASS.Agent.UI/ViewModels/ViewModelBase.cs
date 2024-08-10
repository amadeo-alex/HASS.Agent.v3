using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Newtonsoft.Json.Bson;

namespace HASS.Agent.UI.ViewModels;
public abstract class ViewModelBase : ObservableRecipient
{
	protected DispatcherQueue _dispatcherQueue;

	private Dictionary<string, List<string>> _propertyMap = [];

	protected ViewModelBase(DispatcherQueue dispatcherQueue)
	{
		_dispatcherQueue = dispatcherQueue;
	}

	protected void RaiseOnPropertyChanged(string propertyName)
	{
		_dispatcherQueue.TryEnqueue(() => OnPropertyChanged(propertyName));
	}

	protected void RunOnDispatcher(DispatcherQueueHandler handler)
	{
		_dispatcherQueue.TryEnqueue(handler);
	}

	//TODO(Amadeo): app proper function description so they can be reused by others
	protected void AddPropertyListenerMap(string sourcePropertyName, List<string> targetPropertyNames)
	{
		_propertyMap.Add(sourcePropertyName, targetPropertyNames);
	}

	protected void AddPropertyListenerMap(string sourcePropertyName, string targetPropertyName) => AddPropertyListenerMap(sourcePropertyName, [targetPropertyName]);


	protected void AddPropertyListenerMap(string sourceTargetPropertyName) => AddPropertyListenerMap(sourceTargetPropertyName, sourceTargetPropertyName);

	protected void AddPropertyListenerMap(List<string> sourceTargetProperyNames)
	{
		foreach (var name in sourceTargetProperyNames)
		{
			AddPropertyListenerMap(name);
		}
	}

	protected void ParseSourcePropertyChanged(PropertyChangedEventArgs e)
	{
		if (e.PropertyName != null && _propertyMap.TryGetValue(e.PropertyName, out var targetPropertyNames))
		{
			foreach (var name in targetPropertyNames)
			{
				RaiseOnPropertyChanged(name);
			}
		}
	}
}
