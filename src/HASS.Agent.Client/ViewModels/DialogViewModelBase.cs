using System;
using CommunityToolkit.Mvvm.Input;
using Irihi.Avalonia.Shared.Contracts;

namespace HASS.Agent.Client.ViewModels;

public partial class DialogViewModelBase : ViewModelBase, IDialogContext
{
    public event EventHandler<object?>? RequestClose;
    public bool Confirmed { get; set; }
    
    [RelayCommand]
    private void Accepted()
    {
        Confirmed = true;
        RequestClose?.Invoke(this, this);
    }
    
    [RelayCommand]
    private void Denied()
    {
        Confirmed = false;
        RequestClose?.Invoke(this, this);
    }

    public void Close()
    {
        Confirmed = false;
        RequestClose?.Invoke(this, this);
    }
}