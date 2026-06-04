using System;
using System.Collections.Generic;
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

public partial class PurchaseInvoiceFormViewModel : BaseViewModel
{
    private readonly IRepository<PurchaseInvoice> _invoiceRepository;
    private readonly IRepository<PurchaseInvoiceDetail> _detailRepository;
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;

    private readonly List<PurchaseInvoiceDetail> _detailsToDelete = new();

    [ObservableProperty]
    private PurchaseInvoice _model = new();

    [ObservableProperty]
    private ObservableCollection<PurchaseInvoiceDetail> _currentDetails = new();

    [ObservableProperty]
    private PurchaseInvoiceDetail? _selectedDetail;

    public bool CanEdit => !Model.IsPosted;

    public PurchaseInvoiceFormViewModel(
        IRepository<PurchaseInvoice> invoiceRepository,
        IRepository<PurchaseInvoiceDetail> detailRepository,
        IDialogService dialogService,
        IServiceProvider serviceProvider)
    {
        _invoiceRepository = invoiceRepository;
        _detailRepository = detailRepository;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;
    }

    public Task InitializeAsync(PurchaseInvoice invoice)
    {
        Model = invoice;
        Title = invoice.Id == 0 ? "Создание Поступления товаров" : $"Поступление товаров № {invoice.Number} от {invoice.Date:dd.MM.yyyy}";

        CurrentDetails = new ObservableCollection<PurchaseInvoiceDetail>(invoice.Details);
        _detailsToDelete.Clear();

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
            Model.ContractId = 0;
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
        Model.ContractId = 0;
        Model.Contract = null;
        OnPropertyChanged(nameof(Model));
    }

    [RelayCommand]
    private async Task SelectContractAsync()
    {
        if (!CanEdit) return;
        if (Model.ContractorId == 0)
        {
            MessageBox.Show("Сначала выберите поставщика.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
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
        Model.ContractId = 0;
        Model.Contract = null;
        OnPropertyChanged(nameof(Model));
    }

    [RelayCommand]
    private async Task AddDetailAsync()
    {
        if (!CanEdit) return;
        var vm = _serviceProvider.GetRequiredService<PurchaseInvoiceDetailFormViewModel>();
        vm.Initialize(new PurchaseInvoiceDetail { Quantity = 1, Price = 0 });

        if (_dialogService.ShowDialog(vm) == true)
        {
            CurrentDetails.Add(vm.Model);
            RecalculateTotal();
        }
    }

    [RelayCommand]
    private async Task EditDetailAsync(PurchaseInvoiceDetail detail)
    {
        if (!CanEdit || detail == null) return;

        var clone = new PurchaseInvoiceDetail
        {
            Id = detail.Id,
            NomenclatureId = detail.NomenclatureId,
            Nomenclature = detail.Nomenclature,
            Quantity = detail.Quantity,
            Price = detail.Price,
            Sum = detail.Sum,
            PurchaseInvoiceId = detail.PurchaseInvoiceId
        };

        var vm = _serviceProvider.GetRequiredService<PurchaseInvoiceDetailFormViewModel>();
        vm.Initialize(clone);

        if (_dialogService.ShowDialog(vm) == true)
        {
            var index = CurrentDetails.IndexOf(detail);
            CurrentDetails.RemoveAt(index);
            CurrentDetails.Insert(index, vm.Model);
            SelectedDetail = vm.Model;
            RecalculateTotal();
        }
    }

    [RelayCommand]
    private void RemoveDetail()
    {
        if (!CanEdit) return;
        if (SelectedDetail != null)
        {
            if (SelectedDetail.Id != 0) _detailsToDelete.Add(SelectedDetail);
            CurrentDetails.Remove(SelectedDetail);
            RecalculateTotal();
        }
    }

    private void RecalculateTotal()
    {
        Model.TotalAmount = CurrentDetails.Sum(d => d.Sum);
        OnPropertyChanged(nameof(Model));
    }

    [RelayCommand]
    private async Task SaveAsync(Window window)
    {
        if (!CanEdit) return;
        if (Model.ContractorId == 0 || Model.ContractId == 0)
        {
            MessageBox.Show("Укажите поставщика и договор.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        RecalculateTotal();
        IsBusy = true;
        try
        {
            if (_detailsToDelete.Any())
            {
                await _detailRepository.DeleteRangeAsync(_detailsToDelete);
                _detailsToDelete.Clear();
            }

            Model.Details = CurrentDetails.ToList();

            // ИЗОЛЯЦИЯ ОТСОЕДИНЕННОГО ГРАФА
            Model.Contractor = null!;
            Model.Contract = null;
            foreach (var detail in Model.Details)
            {
                detail.Nomenclature = null!;
                detail.PurchaseInvoice = null!;
            }

            if (Model.Id == 0) await _invoiceRepository.AddAsync(Model);
            else await _invoiceRepository.UpdateAsync(Model);

            if (window != null) window.DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при сохранении: {ex.InnerException?.Message ?? ex.Message}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void Cancel(Window window)
    {
        if (window != null) window.DialogResult = false;
    }
}