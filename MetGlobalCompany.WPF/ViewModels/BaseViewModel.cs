using CommunityToolkit.Mvvm.ComponentModel;

namespace MetGlobalCompany.WPF.ViewModels;

/// <summary>
/// Базовый класс для всех ViewModel в приложении. 
/// Предоставляет реализацию INotifyPropertyChanged через CommunityToolkit.Mvvm.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;
}