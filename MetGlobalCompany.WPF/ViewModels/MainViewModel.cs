using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MetGlobalCompany.Domain.Entities;
using MetGlobalCompany.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MetGlobalCompany.WPF.ViewModels;

public class AppMenuItem
{
    public string Icon { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public Type ViewModelType { get; set; } = null!;
    public bool IsSeparator { get; set; }
}

public partial class MainViewModel : BaseViewModel
{
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private BaseViewModel? _currentViewModel;

    [ObservableProperty]
    private string _globalSearchText = string.Empty;

    public ObservableCollection<AppMenuItem> MenuItems { get; } = new();
    public ObservableCollection<AppMenuItem> OptionMenuItems { get; } = new();

    [ObservableProperty]
    private AppMenuItem? _selectedMenuItem;

    public MainViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Title = "MetGlobal ERP - Enterprise Edition";

        InitializeMenu();
        SelectedMenuItem = MenuItems.FirstOrDefault(m => !m.IsSeparator);
    }

    private void InitializeMenu()
    {
        MenuItems.Add(new AppMenuItem { Icon = "ChartLine", Label = "Анализ продаж", ViewModelType = typeof(SalesDashboardViewModel) });
        MenuItems.Add(new AppMenuItem { Icon = "ViewDashboardOutline", Label = "Единый журнал", ViewModelType = typeof(AllDocumentsViewModel) });

        MenuItems.Add(new AppMenuItem { IsSeparator = true });

        MenuItems.Add(new AppMenuItem { Icon = "CartOutline", Label = "Заказы клиентов", ViewModelType = typeof(OrdersViewModel) });
        MenuItems.Add(new AppMenuItem { Icon = "FileDocumentOutline", Label = "Реализации (УПД)", ViewModelType = typeof(SalesInvoicesViewModel) });
        MenuItems.Add(new AppMenuItem { Icon = "TruckDeliveryOutline", Label = "Поступления товаров", ViewModelType = typeof(PurchaseInvoicesViewModel) });

        MenuItems.Add(new AppMenuItem { IsSeparator = true });

        MenuItems.Add(new AppMenuItem { Icon = "BankTransfer", Label = "Банк и касса", ViewModelType = typeof(PaymentsViewModel) });

        MenuItems.Add(new AppMenuItem { IsSeparator = true });

        MenuItems.Add(new AppMenuItem { Icon = "TagOutline", Label = "Установка цен", ViewModelType = typeof(PriceSettingsViewModel) });
        MenuItems.Add(new AppMenuItem { Icon = "TagMultipleOutline", Label = "Типы цен", ViewModelType = typeof(PriceTypesViewModel) });

        MenuItems.Add(new AppMenuItem { IsSeparator = true });

        MenuItems.Add(new AppMenuItem { Icon = "AccountTieOutline", Label = "Контрагенты", ViewModelType = typeof(ContractorsViewModel) });
        MenuItems.Add(new AppMenuItem { Icon = "HandshakeOutline", Label = "Договоры", ViewModelType = typeof(ContractsViewModel) });
        MenuItems.Add(new AppMenuItem { Icon = "PackageVariantClosed", Label = "Номенклатура", ViewModelType = typeof(NomenclatureViewModel) });

        OptionMenuItems.Add(new AppMenuItem { Icon = "CogOutline", Label = "Настройки", ViewModelType = typeof(BaseViewModel) });
    }

    partial void OnSelectedMenuItemChanged(AppMenuItem? value)
    {
        if (value == null || value.ViewModelType == typeof(BaseViewModel) || value.IsSeparator) return;
        CurrentViewModel = (BaseViewModel)_serviceProvider.GetRequiredService(value.ViewModelType);
        PushSearchTextToCurrentViewModel();
    }

    public void CreateInvoiceFromOrder(Order baseOrder)
    {
        var invoiceVm = _serviceProvider.GetRequiredService<SalesInvoicesViewModel>();
        invoiceVm.InitializeFromBaseOrder(baseOrder);
        CurrentViewModel = invoiceVm;
        SelectedMenuItem = MenuItems.FirstOrDefault(m => m.ViewModelType == typeof(SalesInvoicesViewModel));
    }

    [RelayCommand]
    private void OpenImportDialog()
    {
        var dialogService = _serviceProvider.GetRequiredService<IDialogService>();
        var importVm = _serviceProvider.GetRequiredService<ImportViewModel>();
        dialogService.ShowDialog(importVm);
    }

    [RelayCommand]
    private void OpenHelpDialog()
    {
        var dialogService = _serviceProvider.GetRequiredService<IDialogService>();
        var helpVm = _serviceProvider.GetRequiredService<HelpViewModel>();
        dialogService.ShowDialog(helpVm);
    }

    partial void OnGlobalSearchTextChanged(string value)
    {
        PushSearchTextToCurrentViewModel();
    }

    private void PushSearchTextToCurrentViewModel()
    {
        if (CurrentViewModel is ContractorsViewModel cVm) cVm.SearchText = GlobalSearchText;
        if (CurrentViewModel is ContractsViewModel contVm) contVm.SearchText = GlobalSearchText;
        if (CurrentViewModel is NomenclatureViewModel nVm) nVm.SearchText = GlobalSearchText;
        if (CurrentViewModel is SalesInvoicesViewModel sVm) sVm.SearchText = GlobalSearchText;
        if (CurrentViewModel is PurchaseInvoicesViewModel pVm) pVm.SearchText = GlobalSearchText;
        if (CurrentViewModel is OrdersViewModel oVm) oVm.SearchText = GlobalSearchText;
        if (CurrentViewModel is PaymentsViewModel payVm) payVm.SearchText = GlobalSearchText;
        if (CurrentViewModel is AllDocumentsViewModel aVm) aVm.SearchText = GlobalSearchText;
        if (CurrentViewModel is PriceSettingsViewModel psetVm) psetVm.SearchText = GlobalSearchText;
    }
}