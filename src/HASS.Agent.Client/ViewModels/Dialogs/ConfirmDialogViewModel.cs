using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Irihi.Avalonia.Shared.Contracts;

namespace HASS.Agent.Client.ViewModels.Dialogs;

public partial class ConfirmDialogViewModel : DialogViewModelBase
{
    public string Title { get; set; } = "I'm a dialog";
    public string Query { get; set; } =  "Would you like to like to would?";
    public string Accept {get; set;} = Translations.Strings.Accept.CurrentValue;
    public string Deny { get; set; } = Translations.Strings.Deny.CurrentValue;
    
    public bool IsTitlePresent => !string.IsNullOrWhiteSpace(Title);
}