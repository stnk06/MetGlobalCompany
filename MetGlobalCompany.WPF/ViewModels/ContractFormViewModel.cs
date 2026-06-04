using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace MetGlobalCompany.WPF.ViewModels;

public partial class ContractFormViewModel : BaseViewModel
{
    private readonly IRepository<Contract> _contractRepository;
    private readonly IRepository<PriceType> _priceTypeRepository;
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private Contract _model = new();

    [ObservableProperty]
    private ObservableCollection<PriceType> _priceTypes = new();

    [ObservableProperty]
    private ObservableCollection<string> _currencyCodes = new() { "RUB", "USD" };

    public ContractFormViewModel(
        IRepository<Contract> contractRepository,
        IRepository<PriceType> priceTypeRepository,
        IDialogService dialogService,
        IServiceProvider serviceProvider)
    {
        _contractRepository = contractRepository;
        _priceTypeRepository = priceTypeRepository;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;
    }

    public async Task InitializeAsync(Contract contract)
    {
        Model = contract;
        Title = contract.Id == 0 ? "Создание договора" : "Редактирование договора";
        PriceTypes = new ObservableCollection<PriceType>(await _priceTypeRepository.GetAllAsync());
    }

    [RelayCommand]
    private async Task SelectContractorAsync()
    {
        var vm = _serviceProvider.GetRequiredService<ContractorSelectViewModel>();
        await vm.InitializeAsync();
        if (_dialogService.ShowDialog(vm) == true && vm.ConfirmedSelection != null)
        {
            Model.ContractorId = vm.ConfirmedSelection.Id;
            Model.Contractor = vm.ConfirmedSelection;
            OnPropertyChanged(nameof(Model));
        }
    }

    [RelayCommand]
    private void ClearContractor()
    {
        Model.ContractorId = 0;
        Model.Contractor = null!;
        OnPropertyChanged(nameof(Model));
    }

    [RelayCommand]
    private async Task SaveAsync(Window window)
    {
        if (string.IsNullOrWhiteSpace(Model.Number) || Model.ContractorId == 0)
        {
            MessageBox.Show("Заполните номер договора и выберите контрагента.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            Model.Contractor = null!;
            Model.PriceType = null;
            if (Model.Id == 0) await _contractRepository.AddAsync(Model);
            else await _contractRepository.UpdateAsync(Model);

            if (window != null) window.DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при сохранении: {ex.InnerException?.Message ?? ex.Message}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel(Window window)
    {
        if (window != null) window.DialogResult = false;
    }
}