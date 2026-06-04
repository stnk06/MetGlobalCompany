using System.Windows;
using CommunityToolkit.Mvvm.Input;

namespace MetGlobalCompany.WPF.ViewModels;

public partial class HelpViewModel : BaseViewModel
{
    public HelpViewModel()
    {
        Title = "Руководство пользователя";
    }

    [RelayCommand]
    private void Close(Window window)
    {
        if (window != null) window.DialogResult = true;
    }
}