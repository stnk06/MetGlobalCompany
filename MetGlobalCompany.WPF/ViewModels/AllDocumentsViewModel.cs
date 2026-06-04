using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Bibliography;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Domain.Entities;
using MetGlobalCompany.Domain.Enums;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MetGlobalCompany.WPF.ViewModels;

public class DocumentJournalItem
{
    public DateTime Date { get; set; }
    public string DocType { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string ContractorName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsPosted { get; set; }
}

public partial class AllDocumentsViewModel : BaseViewModel
{
    private readonly IRepository<SalesInvoice> _salesRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<PurchaseInvoice> _purchaseRepository;
    private readonly IRepository<PaymentDocument> _paymentRepository;

    public ObservableCollection<DocumentJournalItem> DocumentsSource { get; set; } = new();
    public ICollectionView DocumentsView { get; private set; }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set { SetProperty(ref _searchText, value); DocumentsView?.Refresh(); }
    }

    public ObservableCollection<string> AvailableDocTypes { get; } = new();
    [ObservableProperty] private string? _selectedDocTypeFilter;
    partial void OnSelectedDocTypeFilterChanged(string? value) => DocumentsView?.Refresh();

    public ObservableCollection<string> AvailableContractors { get; } = new();
    [ObservableProperty] private string? _selectedContractorFilter;
    partial void OnSelectedContractorFilterChanged(string? value) => DocumentsView?.Refresh();

    public AllDocumentsViewModel(
        IRepository<SalesInvoice> salesRepository,
        IRepository<Order> orderRepository,
        IRepository<PurchaseInvoice> purchaseRepository,
        IRepository<PaymentDocument> paymentRepository)
    {
        _salesRepository = salesRepository;
        _orderRepository = orderRepository;
        _purchaseRepository = purchaseRepository;
        _paymentRepository = paymentRepository;

        Title = "Единый журнал документов";
        DocumentsView = CollectionViewSource.GetDefaultView(DocumentsSource);
        DocumentsView.Filter = FilterDocuments;

        _ = LoadDataAsync();
    }

    private bool FilterDocuments(object item)
    {
        if (item is not DocumentJournalItem doc) return false;

        if (!string.IsNullOrEmpty(SelectedDocTypeFilter) && doc.DocType != SelectedDocTypeFilter) return false;
        if (!string.IsNullOrEmpty(SelectedContractorFilter) && doc.ContractorName != SelectedContractorFilter) return false;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            return doc.Number.ToLower().Contains(search) ||
                   doc.ContractorName.ToLower().Contains(search) ||
                   doc.DocType.ToLower().Contains(search);
        }
        return true;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var sales = await _salesRepository.GetAllWithIncludesAsync(default, i => i.Contractor);
            var orders = await _orderRepository.GetAllWithIncludesAsync(default, i => i.Contractor);
            var purchases = await _purchaseRepository.GetAllWithIncludesAsync(default, i => i.Contractor);
            var payments = await _paymentRepository.GetAllWithIncludesAsync(default, i => i.Contractor);

            var allDocs = sales.Select(s => new DocumentJournalItem { Date = s.Date, DocType = "Реализация (УПД)", Number = s.Number, ContractorName = s.Contractor?.Name ?? "Не указан", TotalAmount = s.TotalAmount, IsPosted = s.IsPosted })
                .Concat(orders.Select(o => new DocumentJournalItem { Date = o.Date, DocType = "Заказ клиента", Number = o.Number, ContractorName = o.Contractor?.Name ?? "Не указан", TotalAmount = o.TotalAmount, IsPosted = o.IsPosted }))
                .Concat(purchases.Select(p => new DocumentJournalItem { Date = p.Date, DocType = "Поступление товаров", Number = p.Number, ContractorName = p.Contractor?.Name ?? "Не указан", TotalAmount = p.TotalAmount, IsPosted = p.IsPosted }))
                .Concat(payments.Select(pay => new DocumentJournalItem { Date = pay.Date, DocType = pay.Type == PaymentType.Incoming ? "Входящий платеж" : "Исходящий платеж", Number = pay.Number, ContractorName = pay.Contractor?.Name ?? "Не указан", TotalAmount = pay.Amount, IsPosted = pay.IsPosted }))
                .OrderByDescending(d => d.Date)
                .ToList();

            DocumentsSource.Clear();
            foreach (var doc in allDocs) DocumentsSource.Add(doc);

            AvailableDocTypes.Clear();
            foreach (var t in allDocs.Select(d => d.DocType).Distinct().OrderBy(t => t)) AvailableDocTypes.Add(t);

            AvailableContractors.Clear();
            foreach (var c in allDocs.Select(d => d.ContractorName).Distinct().OrderBy(c => c)) AvailableContractors.Add(c);

            DocumentsView.Refresh();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsBusy = false; }
    }
}