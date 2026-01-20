using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Avalonia.Threading;

namespace HASS.Agent.Client.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
	public MainViewViewModel MainViewViewModel { get; set; }
	
	public MainWindowViewModel(Dispatcher dispatcher, MainViewViewModel mainViewViewModel) : base(dispatcher)
	{
		MainViewViewModel = mainViewViewModel;
	}

}