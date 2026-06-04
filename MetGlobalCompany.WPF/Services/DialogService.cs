using System.Windows;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.WPF.Views;

namespace MetGlobalCompany.WPF.Services;

public class DialogService : IDialogService
{
    public bool? ShowDialog(object viewModel)
    {
        var window = new DialogWindow
        {
            DataContext = viewModel,
            Owner = System.Windows.Application.Current.MainWindow
        };

        return window.ShowDialog();
    }

    public void ShowMessage(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}