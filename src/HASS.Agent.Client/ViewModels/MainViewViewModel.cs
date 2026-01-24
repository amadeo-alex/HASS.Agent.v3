using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HASS.Agent.Client.ViewModels.Pages;
using IconPacks.Avalonia.Material;
using Microsoft.Extensions.DependencyInjection;

namespace HASS.Agent.Client.ViewModels;

public partial class MainViewViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase? _content;

    public ObservableCollection<MenuItemViewModel> MenuItems { get; set; }
    public ObservableCollection<MenuItemViewModel> FooterMenuItems { get; set; }

    [ObservableProperty]
    private MenuItemViewModel? _selectedMenuItem;

    [ObservableProperty]
    private MenuItemViewModel? _selectedFooterMenuItem;

    public MainViewViewModel()
    {
        MenuItems = new ObservableCollection<MenuItemViewModel>()
        {
            new() { Header = "Home", DestinationType = typeof(HomePageViewModel), IconKind = PackIconMaterialKind.Cog },

            new() { Header = "Client", IsSeparator = true },
            new() { Header = "Sensors", DestinationType = typeof(SensorsPageViewModel), IconKind = PackIconMaterialKind.DatabaseOutline },
            new() { Header = "Commands", IconKind = PackIconMaterialKind.FileAlert },

            new() { Header = "Service", IsSeparator = true },
            new() { Header = "Sensors", IconKind = PackIconMaterialKind.DatabaseOutline },
            new() { Header = "Commands", IconKind = PackIconMaterialKind.FileAlert },
        };

        FooterMenuItems = new ObservableCollection<MenuItemViewModel>()
        {
            new() { Header = "Debug", IconKind = PackIconMaterialKind.Bug },
            new() { Header = "Settings", DestinationType = typeof(SettingsPageViewModel), IconKind = PackIconMaterialKind.Cog },
        };

        Content = App.XXX.Services.GetRequiredService<HomePageViewModel>();
        SelectedMenuItem = MenuItems[0];
    }

    [RelayCommand]
    private void MenuItemActivated(MenuItemViewModel? menuItemViewModel)
    {
        if (menuItemViewModel?.DestinationType == null)
        {
            return;
        }
        
        Content = (ViewModelBase)App.XXX.Services.GetRequiredService(menuItemViewModel.DestinationType);
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

    partial void OnSelectedMenuItemChanging(MenuItemViewModel? oldValue, MenuItemViewModel? newValue)
    {
        SelectedFooterMenuItem = null;
    }

    partial void OnSelectedFooterMenuItemChanging(MenuItemViewModel? oldValue, MenuItemViewModel? newValue)
    {
        SelectedMenuItem = null;
    }
}