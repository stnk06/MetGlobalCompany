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
using Microsoft.Win32;

namespace MetGlobalCompany.WPF.ViewModels;

public partial class SalesInvoicesViewModel : BaseViewModel
{
    private readonly IRepository<SalesInvoice> _invoiceRepository;
    private readonly IDocumentPostingService _postingService;
    private readonly IExportService _exportService;
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;

    private List<SalesInvoice> _allInvoices = new();

    [ObservableProperty]
    private ObservableCollection<SalesInvoice> _invoices = new();

    [ObservableProperty]
    private SalesInvoice? _selectedInvoice;

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

    public SalesInvoicesViewModel(
        IRepository<SalesInvoice> invoiceRepository,
        IDocumentPostingService postingService,
        IExportService exportService,
        IDialogService dialogService,
        IServiceProvider serviceProvider)
    {
        _invoiceRepository = invoiceRepository;
        _postingService = postingService;
        _exportService = exportService;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;

        Title = "Журнал: Реализация товаров и услуг";
        _ = LoadDataAsync();
    }

    private void ApplyFilter()
    {
        var filtered = _allInvoices.AsEnumerable();

        if (FilterStartDate.HasValue) filtered = filtered.Where(o => o.Date.Date >= FilterStartDate.Value.Date);
        if (FilterEndDate.HasValue) filtered = filtered.Where(o => o.Date.Date <= FilterEndDate.Value.Date);
        if (!string.IsNullOrEmpty(SelectedContractorFilter)) filtered = filtered.Where(o => o.Contractor?.Name == SelectedContractorFilter);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(o =>
                o.Number.ToLower().Contains(search) ||
                (o.Contractor != null && o.Contractor.Name.ToLower().Contains(search))
            );
        }

        Invoices = new ObservableCollection<SalesInvoice>(filtered.ToList());
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
            _allInvoices = (await _invoiceRepository.GetAllWithStringIncludesAsync(default, "Contractor", "Contract", "BaseOrder", "Details.Nomenclature.Unit")).OrderByDescending(i => i.Date).ToList();

            AvailableContractors.Clear();
            var contractors = _allInvoices.Where(c => c.Contractor != null).Select(c => c.Contractor.Name).Distinct().OrderBy(n => n);
            foreach (var c in contractors) AvailableContractors.Add(c);

            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsBusy = false; }
    }

    public async void InitializeFromBaseOrder(Order baseOrder)
    {
        var newInvoice = new SalesInvoice
        {
            Date = DateTime.Now,
            Number = $"РТУ-{DateTime.Now:yyyyMMdd-HHmm}",
            ContractorId = baseOrder.ContractorId,
            ContractId = baseOrder.ContractId,
            BaseOrderId = baseOrder.Id,
            TotalAmount = baseOrder.TotalAmount
        };

        foreach (var detail in baseOrder.OrderDetails)
        {
            newInvoice.Details.Add(new SalesInvoiceDetail { NomenclatureId = detail.NomenclatureId, Quantity = detail.Quantity, Price = detail.Price, Sum = detail.Sum });
        }

        try
        {
            var formVm = _serviceProvider.GetRequiredService<SalesInvoiceFormViewModel>();
            await formVm.InitializeAsync(newInvoice);
            if (_dialogService.ShowDialog(formVm) == true) await LoadDataAsync();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    [RelayCommand]
    private async Task AddNewAsync()
    {
        try
        {
            var formVm = _serviceProvider.GetRequiredService<SalesInvoiceFormViewModel>();
            await formVm.InitializeAsync(new SalesInvoice { Date = DateTime.Now, Number = $"РТУ-{DateTime.Now:yyyyMMdd-HHmm}" });
            if (_dialogService.ShowDialog(formVm) == true) await LoadDataAsync();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    [RelayCommand]
    private async Task EditCurrentAsync()
    {
        if (SelectedInvoice == null) return;
        try
        {
            var formVm = _serviceProvider.GetRequiredService<SalesInvoiceFormViewModel>();
            await formVm.InitializeAsync(SelectedInvoice);
            if (_dialogService.ShowDialog(formVm) == true) await LoadDataAsync();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    [RelayCommand]
    private async Task PostDocumentAsync()
    {
        if (SelectedInvoice == null || SelectedInvoice.Id == 0 || SelectedInvoice.IsPosted) return;
        IsBusy = true;
        try
        {
            if (await _postingService.PostSalesInvoiceAsync(SelectedInvoice.Id))
            {
                MessageBox.Show("Документ проведен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadDataAsync();
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task UnpostDocumentAsync()
    {
        if (SelectedInvoice == null || SelectedInvoice.Id == 0 || !SelectedInvoice.IsPosted) return;
        IsBusy = true;
        try
        {
            if (await _postingService.UnpostSalesInvoiceAsync(SelectedInvoice.Id))
            {
                MessageBox.Show("Проведение отменено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadDataAsync();
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task PrintToExcelAsync()
    {
        if (SelectedInvoice == null || SelectedInvoice.Id == 0) return;
        var doc = _allInvoices.FirstOrDefault(i => i.Id == SelectedInvoice.Id);
        if (doc == null) return;

        var dialog = new SaveFileDialog { FileName = $"УПД_{doc.Number.Replace("/", "_")}.xlsx", DefaultExt = ".xlsx", Filter = "Excel Files|*.xlsx" };
        if (dialog.ShowDialog() == true)
        {
            IsBusy = true;
            try
            {
                await _exportService.ExportSalesInvoiceToExcelAsync(doc, dialog.FileName);
                MessageBox.Show("Успешно экспортировано!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
            finally { IsBusy = false; }
        }
    }

    [RelayCommand]
    private async Task PreviewTorg12Async()
    {
        if (SelectedInvoice == null || SelectedInvoice.Id == 0) return;
        var doc = _allInvoices.FirstOrDefault(i => i.Id == SelectedInvoice.Id);
        if (doc == null) return;

        try
        {
            var previewVm = _serviceProvider.GetRequiredService<Torg12PreviewViewModel>();
            await previewVm.InitializeAsync(doc);
            _dialogService.ShowDialog(previewVm);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}