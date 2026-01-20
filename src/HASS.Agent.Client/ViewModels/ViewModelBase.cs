using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace HASS.Agent.Client.ViewModels;

public class ViewModelBase : ObservableObject
{
    private readonly Dispatcher? _dispatcher;

    private readonly Dictionary<string, List<string>> _propertyMap = [];

    public ViewModelBase()
    {
        
    }
    
    protected ViewModelBase(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    protected void RaiseOnPropertyChanged(string propertyName)
    {
        _dispatcher?.InvokeAsync(() => OnPropertyChanged(propertyName));
    }

    protected void RunOnDispatcher(Action handler)
    {
        _dispatcher?.InvokeAsync(handler);
    }

    //TODO(Amadeo): app proper function description so they can be reused by others
    protected void AddPropertyListenerMap(string sourcePropertyName, List<string> targetPropertyNames)
    {
        _propertyMap.Add(sourcePropertyName, targetPropertyNames);
    }

    protected void AddPropertyListenerMap(string sourcePropertyName, string targetPropertyName) =>
        AddPropertyListenerMap(sourcePropertyName, [targetPropertyName]);


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