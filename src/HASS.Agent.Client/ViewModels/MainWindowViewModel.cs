using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace HASS.Agent.Client.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
	[ObservableProperty]
	private ViewModelBase _selectedPage;

	[ObservableProperty]
	private object? _selectedItem;
	
	public MainWindowViewModel()
	{
		SelectedPage = new TestWindowViewModel();
	}

	partial void OnSelectedItemChanging(object? oldValue, object? newValue)
	{
		throw new NotImplementedException();
	}

	
	partial void OnSelectedItemChanged(object? value)
	{
		Console.WriteLine();
	}
}