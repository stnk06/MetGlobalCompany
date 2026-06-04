using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Domain.Entities;
using MetGlobalCompany.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace MetGlobalCompany.WPF.ViewModels;

public partial class PaymentsViewModel : BaseViewModel
{
    private readonly IRepository<PaymentDocument> _paymentRepository;
    private readonly IDocumentPostingService _postingService;
    private readonly IBankStatementService _bankStatementService;
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;

    private List<PaymentDocument> _allPayments = new();

    [ObservableProperty]
    private ObservableCollection<PaymentDocument> _payments = new();

    [ObservableProperty]
    private PaymentDocument? _selectedPayment;

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

    public ObservableCollection<string> AvailableContractors { get; } = new();

    [ObservableProperty]
    private string? _selectedContractorFilter;
    partial void OnSelectedContractorFilterChanged(string? value) => ApplyFilter();

    public Array PaymentTypes => Enum.GetValues(typeof(PaymentType));

    [ObservableProperty]
    private PaymentType? _selectedTypeFilter;
    partial void OnSelectedTypeFilterChanged(PaymentType? value) => ApplyFilter();

    public PaymentsViewModel(
        IRepository<PaymentDocument> paymentRepository,
        IDocumentPostingService postingService,
        IBankStatementService bankStatementService,
        IDialogService dialogService,
        IServiceProvider serviceProvider)
    {
        _paymentRepository = paymentRepository;
        _postingService = postingService;
        _bankStatementService = bankStatementService;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;

        Title = "Журнал: Банковские выписки и Платежи";
        _ = LoadDataAsync();
    }

    private void ApplyFilter()
    {
        var filtered = _allPayments.AsEnumerable();

        if (FilterStartDate.HasValue) filtered = filtered.Where(p => p.Date.Date >= FilterStartDate.Value.Date);
        if (FilterEndDate.HasValue) filtered = filtered.Where(p => p.Date.Date <= FilterEndDate.Value.Date);
        if (!string.IsNullOrEmpty(SelectedContractorFilter)) filtered = filtered.Where(p => p.Contractor?.Name == SelectedContractorFilter);
        if (SelectedTypeFilter.HasValue) filtered = filtered.Where(p => p.Type == SelectedTypeFilter.Value);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(p =>
                p.Number.ToLower().Contains(search) ||
                (p.Contractor != null && p.Contractor.Name.ToLower().Contains(search))
            );
        }

        Payments = new ObservableCollection<PaymentDocument>(filtered.ToList());
    }

    [RelayCommand]
    private void ClearFilters()
    {
        FilterStartDate = null;
        FilterEndDate = null;
        SelectedContractorFilter = null;
        SelectedTypeFilter = null;
        SearchText = string.Empty;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            _allPayments = (await _paymentRepository.GetAllWithIncludesAsync(default, p => p.Contractor, p => p.Contract)).OrderByDescending(p => p.Date).ToList();

            AvailableContractors.Clear();
            var contractors = _allPayments.Where(c => c.Contractor != null).Select(c => c.Contractor.Name).Distinct().OrderBy(n => n);
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
            var formVm = _serviceProvider.GetRequiredService<PaymentFormViewModel>();
            await formVm.InitializeAsync(new PaymentDocument { Date = DateTime.Now, Type = PaymentType.Incoming });
            if (_dialogService.ShowDialog(formVm) == true) await LoadDataAsync();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    [RelayCommand]
    private async Task EditCurrentAsync()
    {
        if (SelectedPayment == null) return;
        try
        {
            var formVm = _serviceProvider.GetRequiredService<PaymentFormViewModel>();
            await formVm.InitializeAsync(SelectedPayment);
            if (_dialogService.ShowDialog(formVm) == true) await LoadDataAsync();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    [RelayCommand]
    private async Task ImportFrom1cAsync()
    {
        var dialog = new OpenFileDialog { Filter = "Текстовые файлы выписок (*.txt)|*.txt|Все файлы (*.*)|*.*", Title = "Выберите файл 1CClientBankExchange" };
        if (dialog.ShowDialog() == true)
        {
            IsBusy = true;
            try
            {
                var count = await _bankStatementService.ImportFrom1CFormatAsync(dialog.FileName);
                MessageBox.Show($"Успешно импортировано документов: {count}", "Импорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении файла выписки. Убедитесь, что это корректный файл 1С. Подробности: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsBusy = false; }
        }
    }

    [RelayCommand]
    private async Task PostDocumentAsync()
    {
        if (SelectedPayment == null || SelectedPayment.Id == 0 || SelectedPayment.IsPosted) return;
        IsBusy = true;
        try
        {
            if (await _postingService.PostPaymentAsync(SelectedPayment.Id))
            {
                MessageBox.Show("Документ проведен по регистрам взаиморасчетов.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadDataAsync();
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task UnpostDocumentAsync()
    {
        if (SelectedPayment == null || SelectedPayment.Id == 0 || !SelectedPayment.IsPosted) return;
        IsBusy = true;
        try
        {
            if (await _postingService.UnpostPaymentAsync(SelectedPayment.Id))
            {
                MessageBox.Show("Проведение отменено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadDataAsync();
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        finally { IsBusy = false; }
    }
}