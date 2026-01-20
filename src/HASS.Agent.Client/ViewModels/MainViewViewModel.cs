using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using HASS.Agent.Client.ViewModels.Pages;
using IconPacks.Avalonia.Material;
using Microsoft.Extensions.DependencyInjection;

namespace HASS.Agent.Client.ViewModels;

public partial class MainViewViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase? _content;

    public MenuViewModel MenuViewModel { get; set; }

    public string Test { get; set; } = "Cog";

    public MainViewViewModel()
    {
        MenuViewModel = new MenuViewModel()
        {
            MenuItems = new ObservableCollection<MenuItemViewModel>()
            {
                new() { Header = "Home", Key = "home", IconKind = PackIconMaterialKind.Cog },

                new() { Header = "Client", IsSeparator = true },
                new() { Header = "Sensors", Key = "clientSensors", IconKind = PackIconMaterialKind.DatabaseOutline },
                new() { Header = "Commands", Key = "clientCommands", IconKind = PackIconMaterialKind.Cog },

                new() { Header = "Service", IsSeparator = true },
                new() { Header = "Sensors", Key = "serviceSensors", IconKind = PackIconMaterialKind.DatabaseOutline },
                new() { Header = "Commands", Key = "serviceCommands", IconKind = PackIconMaterialKind.Cog },
            }
        };

        Content = App.XXX.Services.GetRequiredService<HomePageViewModel>();
    }

    partial void OnContentChanging(ViewModelBase? oldValue, ViewModelBase? newValue)
    {
        if (oldValue is INavigationAware navigatedFrom)
        {
            navigatedFrom.OnNavigatedFrom();
        }
    }

    partial void OnContentChanged(ViewModelBase? oldValue, ViewModelBase? newValue)
    {
        if (newValue is INavigationAware navigatedTo)
        {
            navigatedTo.OnNavigatedTo();
        }
    }
}