using HASS.Agent.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HASS.Agent.UI.Views.Dialogs
{
	public class InputContentDialog : ContentDialog
	{
		public T? GetInputContent<T>()
		{
			if (Content is not Page contentPage)
				throw new InvalidOperationException($"Content is not of type {typeof(Page)}");

			if (contentPage.DataContext is not InputDialogContentViewModel viewModel)
				throw new InvalidOperationException($"DataContext is not assignable to {typeof(InputDialogContentViewModel)}");

			if (viewModel.Content is T content)
				return content;

			return viewModel.Content == null ? default : (T)Convert.ChangeType(viewModel.Content, typeof(T));
		}
	}
}
