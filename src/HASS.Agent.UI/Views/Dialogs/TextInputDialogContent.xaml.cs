using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using HASS.Agent.UI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HASS.Agent.UI.Views.Dialogs;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class TextInputDialogContent : Page
{
    private readonly TextInputDialogContentViewModel _viewModel;

    public TextInputDialogContent(string queryTextResourceKey, string initialTextBoxContent, bool numericOnly = false)
    {
        _viewModel = new TextInputDialogContentViewModel()
        {
            QueryTextResourceKey = queryTextResourceKey,
            TextBoxContent = initialTextBoxContent,
            NumericOnly = numericOnly
        };
        this.InitializeComponent();
        DataContext = _viewModel;
    }

    private void TextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        if (_viewModel.NumericOnly)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }
    }
}
