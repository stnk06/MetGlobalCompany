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

public partial class OrdersViewModel : BaseViewModel
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;
    private readonly MainViewModel _mainViewModel;

    private List<Order> _allOrders = new();

    [ObservableProperty]
    private ObservableCollection<Order> _orders = new();

    [ObservableProperty]
    private Order? _selectedOrder;

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) ApplyFilter(); }
    }

    private DateTime? _filterStartDate;
    public DateTime? FilterStartDate
    {
        get => _filterStartDate;
        set { if (SetProperty(ref _filterStartDate, value)) ApplyFilter(); }
    }

    private DateTime? _filterEndDate;
    public DateTime? FilterEndDate
    {
        get => _filterEndDate;
        set { if (SetProperty(ref _filterEndDate, value)) ApplyFilter(); }
    }

    // ВЫПАДАЮЩИЕ ФИЛЬТРЫ
    public ObservableCollection<string> AvailableContractors { get; } = new();

    [ObservableProperty]
    private string? _selectedContractorFilter;
    partial void OnSelectedContractorFilterChanged(string? value) => ApplyFilter();

    public OrdersViewModel(
        IRepository<Order> orderRepository,
        IDialogService dialogService,
        IServiceProvider serviceProvider,
        MainViewModel mainViewModel)
    {
        _orderRepository = orderRepository;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;
        _mainViewModel = mainViewModel;

        Title = "Журнал: Заказы клиентов";
        _ = LoadDataAsync();
    }

    private void ApplyFilter()
    {
        var filtered = _allOrders.AsEnumerable();

        if (FilterStartDate.HasValue)
            filtered = filtered.Where(o => o.Date.Date >= FilterStartDate.Value.Date);

        if (FilterEndDate.HasValue)
            filtered = filtered.Where(o => o.Date.Date <= FilterEndDate.Value.Date);

        if (!string.IsNullOrEmpty(SelectedContractorFilter))
            filtered = filtered.Where(o => o.Contractor?.Name == SelectedContractorFilter);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(o =>
                o.Number.ToLower().Contains(search) ||
                (o.Contractor != null && o.Contractor.Name.ToLower().Contains(search))
            );
        }

        Orders = new ObservableCollection<Order>(filtered.ToList());
    }

    [RelayCommand]
    private void ClearFilters()
    {
        FilterStartDate = null;
        FilterEndDate = null;
        SelectedContractorFilter = null;
        SearchText = string.Empty;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            _allOrders = (await _orderRepository.GetAllWithIncludesAsync(default, i => i.Contractor, i => i.OrderDetails)).OrderByDescending(i => i.Date).ToList();

            AvailableContractors.Clear();
            var contractors = _allOrders.Where(c => c.Contractor != null).Select(c => c.Contractor.Name).Distinct().OrderBy(n => n);
            foreach (var c in contractors) AvailableContractors.Add(c);

            ApplyFilter();
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
        try
        {
            var formVm = _serviceProvider.GetRequiredService<OrderFormViewModel>();
            await formVm.InitializeAsync(new Order { Date = DateTime.Now, Number = $"ЗК-{DateTime.Now:yyyyMMdd-HHmm}" });

            if (_dialogService.ShowDialog(formVm) == true) await LoadDataAsync();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    [RelayCommand]
    private async Task EditCurrentAsync()
    {
        if (SelectedOrder == null) return;
        try
        {
            var formVm = _serviceProvider.GetRequiredService<OrderFormViewModel>();
            await formVm.InitializeAsync(SelectedOrder);

            if (_dialogService.ShowDialog(formVm) == true) await LoadDataAsync();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    [RelayCommand]
    private async Task CreateInvoiceBasedOnOrderAsync()
    {
        if (SelectedOrder == null || SelectedOrder.Id == 0)
        {
            MessageBox.Show("Сначала выберите сохраненный заказ из списка.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SelectedOrder.Status = "Ожидает отгрузки";
        await _orderRepository.UpdateAsync(SelectedOrder);

        _mainViewModel.CreateInvoiceFromOrder(SelectedOrder);
        await LoadDataAsync();
    }
}