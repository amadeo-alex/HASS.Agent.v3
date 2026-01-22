using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.Material;

namespace HASS.Agent.Client.ViewModels;

public partial class MenuItemViewModel : ViewModelBase
{
    public string Header { get; set; } = string.Empty;
    public PackIconMaterialKind IconKind { get; set; }
    public Type? DestinationType { get; set; }
    public bool IsSeparator { get; set; } = false;

    public ObservableCollection<MenuItemViewModel> SubMenu { get; set; } = [];
}