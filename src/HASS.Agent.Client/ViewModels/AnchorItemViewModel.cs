using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace HASS.Agent.Client.ViewModels;

public partial class AnchorItemViewModel : ObservableObject
{
    [ObservableProperty] private string? _anchorId;
    [ObservableProperty] private string? _header;
    public ObservableCollection<AnchorItemViewModel>? Children { get; set; }
    
    partial void OnHeaderChanged(string? value)
    {
        if (string.IsNullOrWhiteSpace(AnchorId) && !string.IsNullOrWhiteSpace(value))
        {
            AnchorId = value;
        }
    }
}