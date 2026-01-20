using System.Collections.ObjectModel;
using Avalonia.Threading;

namespace HASS.Agent.Client.ViewModels;

public class MenuViewModel : ViewModelBase
{
    public ObservableCollection<MenuItemViewModel> MenuItems { get; set; } = [];

    //TODO(Amadeo): reduce this class if not functionality is added 
}