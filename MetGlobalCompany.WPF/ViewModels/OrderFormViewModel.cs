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

public partial class OrderFormViewModel : BaseViewModel
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<OrderDetail> _detailRepository;
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;

    private readonly List<OrderDetail> _detailsToDelete = new();

    [ObservableProperty]
    private Order _model = new();

    [ObservableProperty]
    private ObservableCollection<OrderDetail> _currentDetails = new();

    [ObservableProperty]
    private OrderDetail? _selectedDetail;

    public bool CanEdit => !Model.IsPosted;

    public OrderFormViewModel(
        IRepository<Order> orderRepository,
        IRepository<OrderDetail> detailRepository,
        IDialogService dialogService,
        IServiceProvider serviceProvider)
    {
        _orderRepository = orderRepository;
        _detailRepository = detailRepository;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;
    }

    public Task InitializeAsync(Order order)
    {
        Model = order;
        Title = order.Id == 0 ? "Создание заказа клиента" : $"Заказ клиента № {order.Number} от {order.Date:dd.MM.yyyy}";

        CurrentDetails = new ObservableCollection<OrderDetail>(order.OrderDetails);
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
        Model.ContractId = 0;
        Model.Contract = null;
        OnPropertyChanged(nameof(Model));
    }

    [RelayCommand]
    private async Task AddDetailAsync()
    {
        if (!CanEdit) return;
        var vm = _serviceProvider.GetRequiredService<OrderDetailFormViewModel>();
        vm.Initialize(new OrderDetail { Quantity = 1, Price = 0 }, Model.ContractId, Model.Date);

        if (_dialogService.ShowDialog(vm) == true)
        {
            CurrentDetails.Add(vm.Model);
            RecalculateTotal();
        }
    }

    [RelayCommand]
    private async Task EditDetailAsync(OrderDetail detail)
    {
        if (!CanEdit || detail == null) return;

        var clone = new OrderDetail
        {
            Id = detail.Id,
            NomenclatureId = detail.NomenclatureId,
            Nomenclature = detail.Nomenclature,
            Quantity = detail.Quantity,
            Price = detail.Price,
            Sum = detail.Sum,
            OrderId = detail.OrderId
        };

        var vm = _serviceProvider.GetRequiredService<OrderDetailFormViewModel>();
        vm.Initialize(clone, Model.ContractId, Model.Date);

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
            MessageBox.Show("Укажите контрагента и договор.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            Model.OrderDetails = CurrentDetails.ToList();

            // ИЗОЛЯЦИЯ ОТСОЕДИНЕННОГО ГРАФА
            Model.Contractor = null!;
            Model.Contract = null;
            foreach (var detail in Model.OrderDetails)
            {
                detail.Nomenclature = null!;
                detail.Order = null!;
            }

            if (Model.Id == 0) await _orderRepository.AddAsync(Model);
            else await _orderRepository.UpdateAsync(Model);

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