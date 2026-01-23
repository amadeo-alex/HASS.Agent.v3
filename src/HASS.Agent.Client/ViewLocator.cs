using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using HASS.Agent.Client.ViewModels;

namespace HASS.Agent.Client;

public class ViewLocator : IDataTemplate
{
    public Type BaseViewModelType { get; set; } =  typeof(ViewModelBase);
    
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type == null)
        {
            return new TextBlock { Text = "Not Found: " + name };
        }

        var control = (Control)Activator.CreateInstance(type)!;
        control.DataContext = param;

        return control;
    }

    public bool Match(object? data)
    {
        return data?.GetType().IsSubclassOf(BaseViewModelType) == true;
    }
}