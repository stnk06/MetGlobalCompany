using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Domain.Entities;
using MetGlobalCompany.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace MetGlobalCompany.WPF.ViewModels;

public partial class PaymentFormViewModel : BaseViewModel
{
    private readonly IRepository<PaymentDocument> _paymentRepository;
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private PaymentDocument _model = new();

    public bool CanEdit => !Model.IsPosted;

    public Array PaymentTypes => Enum.GetValues(typeof(PaymentType));

    public PaymentFormViewModel(
        IRepository<PaymentDocument> paymentRepository,
        IDialogService dialogService,
        IServiceProvider serviceProvider)
    {
        _paymentRepository = paymentRepository;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;
    }

    public Task InitializeAsync(PaymentDocument payment)
    {
        Model = payment;
        Title = payment.Id == 0 ? "Создание платежного документа" : $"Платежное поручение № {payment.Number} от {payment.Date:dd.MM.yyyy}";
        OnPropertyChanged(nameof(CanEdit));
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SelectContractorAsync()
    {
        if (!CanEdit) return;
        var vm = _serviceProvider.GetRequiredService<ContractorSelectViewModel>();
        await vm.InitializeAsync();
        if (_dialogService.ShowDialog(vm) == true && vm.ConfirmedSelection != null)
        {
            Model.ContractorId = vm.ConfirmedSelection.Id;
            Model.Contractor = vm.ConfirmedSelection;
            Model.ContractId = null;
            Model.Contract = null;
            OnPropertyChanged(nameof(Model));
        }
    }

    [RelayCommand]
    private void ClearContractor()
    {
        if (!CanEdit) return;
        Model.ContractorId = 0;
        Model.Contractor = null;
        Model.ContractId = null;
        Model.Contract = null;
        OnPropertyChanged(nameof(Model));
    }

    [RelayCommand]
    private async Task SelectContractAsync()
    {
        if (!CanEdit) return;
        if (Model.ContractorId == 0)
        {
            MessageBox.Show("Сначала выберите контрагента.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var vm = _serviceProvider.GetRequiredService<ContractSelectViewModel>();
        await vm.InitializeAsync(Model.ContractorId);
        if (_dialogService.ShowDialog(vm) == true && vm.ConfirmedSelection != null)
        {
            Model.ContractId = vm.ConfirmedSelection.Id;
            Model.Contract = vm.ConfirmedSelection;
            OnPropertyChanged(nameof(Model));
        }
    }

    [RelayCommand]
    private void ClearContract()
    {
        if (!CanEdit) return;
        Model.ContractId = null;
        Model.Contract = null;
        OnPropertyChanged(nameof(Model));
    }

    [RelayCommand]
    private async Task SaveAsync(Window window)
    {
        if (!CanEdit) return;
        if (Model.ContractorId == 0 || Model.Amount <= 0)
        {
            MessageBox.Show("Укажите контрагента и корректную сумму.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            // ИЗОЛЯЦИЯ ОТСОЕДИНЕННОГО ГРАФА
            Model.Contractor = null!;
            Model.Contract = null;

            if (Model.Id == 0) await _paymentRepository.AddAsync(Model);
            else await _paymentRepository.UpdateAsync(Model);

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