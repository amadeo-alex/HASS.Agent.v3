using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.Material;

namespace HASS.Agent.Client.ViewModels;

public class MenuItemViewModel : ViewModelBase
{
    public string Test { get; set; } = "Cog";
    public string Header { get; set; } = string.Empty;
    public PackIconMaterialKind IconKind { get; set; }
    public string Key { get; set; } = string.Empty;

    public bool IsSeparator { get; set; } = false;

    public ObservableCollection<MenuItemViewModel> SubMenu { get; set; } = [];

    public ICommand? ActivateCommand { get; set; }

    public MenuItemViewModel()
    {
        ActivateCommand = new RelayCommand(OnActivated);
    }

    private void OnActivated()
    {
        if (IsSeparator || string.IsNullOrEmpty(Key))
        {
            return;
        }

        //TODO: handle navigation?
    }
}