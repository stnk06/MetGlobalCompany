using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace MetGlobalCompany.WPF.ViewModels;

public partial class PriceTypesViewModel : BaseViewModel
{
    private readonly IRepository<PriceType> _priceTypeRepository;
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ObservableCollection<PriceType> _priceTypes = new();

    [ObservableProperty]
    private PriceType? _selectedPriceType;

    public PriceTypesViewModel(IRepository<PriceType> priceTypeRepository, IDialogService dialogService, IServiceProvider serviceProvider)
    {
        _priceTypeRepository = priceTypeRepository;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;
        Title = "Справочник: Типы цен";

        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var data = await _priceTypeRepository.GetAllAsync();
            PriceTypes = new ObservableCollection<PriceType>(data.OrderBy(p => p.Name));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task AddNewAsync()
    {
        var formVm = _serviceProvider.GetRequiredService<PriceTypeFormViewModel>();
        formVm.Initialize(new PriceType { CurrencyCode = "RUB" });

        if (_dialogService.ShowDialog(formVm) == true) await LoadDataAsync();
    }

    [RelayCommand]
    private async Task EditCurrentAsync()
    {
        if (SelectedPriceType == null) return;
        var formVm = _serviceProvider.GetRequiredService<PriceTypeFormViewModel>();
        formVm.Initialize(SelectedPriceType);

        if (_dialogService.ShowDialog(formVm) == true) await LoadDataAsync();
    }
}