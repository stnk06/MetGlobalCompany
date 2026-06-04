using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MetGlobalCompany.Application.DTOs.Analytics;
using MetGlobalCompany.Application.Interfaces;
using Microsoft.Win32;

namespace MetGlobalCompany.WPF.ViewModels;

public class ChartItem
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public double NormalizedHeight { get; set; }
    public string TooltipText { get; set; } = string.Empty;
}

public partial class SalesDashboardViewModel : BaseViewModel
{
    private readonly ISalesAnalyticsService _analyticsService;
    private readonly IExportService _exportService;

    [ObservableProperty] private decimal _totalRevenue;
    [ObservableProperty] private int _totalInvoices;
    [ObservableProperty] private decimal _averageReceipt;
    [ObservableProperty] private decimal _totalItemsSold;

    [ObservableProperty] private ObservableCollection<TopNomenclatureDto> _topNomenclatures = new();
    [ObservableProperty] private ObservableCollection<TopContractorDto> _topContractors = new();
    [ObservableProperty] private ObservableCollection<ChartItem> _salesChartItems = new();
    [ObservableProperty] private ObservableCollection<ChartItem> _topProductsChartItems = new();

    [ObservableProperty] private SalesReportDto? _salesReport;

    private DateTime? _filterStartDate;
    public DateTime? FilterStartDate
    {
        get => _filterStartDate;
        set { if (SetProperty(ref _filterStartDate, value)) _ = LoadDashboardDataAsync(); }
    }

    private DateTime? _filterEndDate;
    public DateTime? FilterEndDate
    {
        get => _filterEndDate;
        set { if (SetProperty(ref _filterEndDate, value)) _ = LoadDashboardDataAsync(); }
    }

    private DateTime? _reportStartDate;
    public DateTime? ReportStartDate
    {
        get => _reportStartDate;
        set => SetProperty(ref _reportStartDate, value);
    }

    private DateTime? _reportEndDate;
    public DateTime? ReportEndDate
    {
        get => _reportEndDate;
        set => SetProperty(ref _reportEndDate, value);
    }

    public SalesDashboardViewModel(ISalesAnalyticsService analyticsService, IExportService exportService)
    {
        _analyticsService = analyticsService;
        _exportService = exportService;
        Title = "BI Аналитика: Панель управления";

        var now = DateTime.Now;
        _filterStartDate = new DateTime(now.Year, now.Month, 1);
        _filterEndDate = now;
        _reportStartDate = new DateTime(now.Year, now.Month, 1);
        _reportEndDate = now;

        _ = LoadDashboardDataAsync();
    }

    [RelayCommand]
    private void ClearDashboardFilters()
    {
        FilterStartDate = null;
        FilterEndDate = null;
    }

    [RelayCommand]
    private void ClearReportFilters()
    {
        ReportStartDate = null;
        ReportEndDate = null;
    }

    [RelayCommand]
    private async Task LoadDashboardDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var dashboardData = await _analyticsService.GetSalesDashboardAsync(FilterStartDate, FilterEndDate);

            TotalRevenue = dashboardData.TotalRevenue;
            TotalInvoices = dashboardData.TotalInvoices;
            AverageReceipt = dashboardData.AverageReceipt;
            TotalItemsSold = dashboardData.TotalItemsSold;

            TopNomenclatures.Clear();
            TopProductsChartItems.Clear();
            var maxProductRevenue = dashboardData.TopNomenclatures.Any() ? dashboardData.TopNomenclatures.Max(x => x.Revenue) : 1;

            foreach (var item in dashboardData.TopNomenclatures)
            {
                TopNomenclatures.Add(item);
                TopProductsChartItems.Add(new ChartItem
                {
                    Label = item.NomenclatureName,
                    Value = item.Revenue,
                    NormalizedHeight = (double)(item.Revenue / maxProductRevenue) * 150.0,
                    TooltipText = $"{item.NomenclatureName}\nВыручка: {item.Revenue:N2} ₽\nКол-во: {item.QuantitySold:N0}"
                });
            }

            TopContractors.Clear();
            foreach (var item in dashboardData.TopContractors)
            {
                TopContractors.Add(item);
            }

            SalesChartItems.Clear();
            var maxDailyRevenue = dashboardData.SalesByDate.Any() ? dashboardData.SalesByDate.Max(x => x.Revenue) : 1;

            foreach (var item in dashboardData.SalesByDate)
            {
                SalesChartItems.Add(new ChartItem
                {
                    Label = item.Date.ToString("dd.MM"),
                    Value = item.Revenue,
                    NormalizedHeight = (double)(item.Revenue / maxDailyRevenue) * 200.0,
                    TooltipText = $"{item.Date:dd.MM.yyyy}\nВыручка: {item.Revenue:N2} ₽\nДокументов: {item.InvoicesCount}"
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка загрузки дашборда", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GenerateReportAsync()
    {
        IsBusy = true;
        try
        {
            SalesReport = await _analyticsService.GetHierarchicalSalesReportAsync(ReportStartDate, ReportEndDate);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка формирования отчета", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportReportAsync()
    {
        if (SalesReport == null || !SalesReport.ContractorGroups.Any())
        {
            MessageBox.Show("Сначала сформируйте отчет.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new SaveFileDialog
        {
            FileName = $"Анализ_продаж_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
            DefaultExt = ".xlsx",
            Filter = "Excel Files|*.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            IsBusy = true;
            try
            {
                await _exportService.ExportSalesReportToExcelAsync(SalesReport, dialog.FileName);
                MessageBox.Show("Отчет успешно экспортирован и сгруппирован!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка экспорта", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}