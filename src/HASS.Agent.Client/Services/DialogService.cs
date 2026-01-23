using System;
using System.Threading.Tasks;
using HASS.Agent.Client.ViewModels;
using HASS.Agent.Client.ViewModels.Dialogs;
using HASS.Agent.Client.Views.Dialogs;
using HASS.Agent.Contracts.Services;
using Ursa.Controls;

namespace HASS.Agent.Client.Services;

public class DialogService : IDialogService
{
    private readonly ViewLocator _viewLocator;

    public DialogService()
    {
        _viewLocator = new ViewLocator()
        {
            BaseViewModelType = typeof(DialogViewModelBase)
        };
    }
    
    public async Task<T> ShowDialogAsync<T>(T viewModel)
    {
        if (!_viewLocator.Match(viewModel))
        {
            throw new ArgumentException($"view model does not inherit from {nameof(DialogViewModelBase)}");
        }
        
        var view = _viewLocator.Build(viewModel);
        if (view is null)
        {
            throw new ArgumentException($"no view can be found for view model {typeof(T).Name}");
        }
        
        var result = await OverlayDialog.ShowCustomModal<T>(view, viewModel, null, new OverlayDialogOptions()
        {
            IsCloseButtonVisible = false,
        });
        
        return result ?? throw new InvalidOperationException("dialog result return value cannot be null");
    }
}