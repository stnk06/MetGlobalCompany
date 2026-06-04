using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace MetGlobalCompany.WPF.ViewModels;

public partial class OrderDetailFormViewModel : BaseViewModel
{
    private readonly IPriceService _priceService;
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;

    private int? _contractId;
    private DateTime _documentDate;
    private int _detailId;

    [ObservableProperty] private Nomenclature? _nomenclature;
    [ObservableProperty] private decimal _quantity;
    [ObservableProperty] private decimal _price;

    public decimal Sum => Quantity * Price;

    partial void OnQuantityChanged(decimal value) => OnPropertyChanged(nameof(Sum));
    partial void OnPriceChanged(decimal value) => OnPropertyChanged(nameof(Sum));

    public OrderDetail Model { get; private set; } = new();

    public OrderDetailFormViewModel(IPriceService priceService, IDialogService dialogService, IServiceProvider serviceProvider)
    {
        _priceService = priceService;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;
    }

    public void Initialize(OrderDetail detail, int? contractId, DateTime documentDate)
    {
        Title = detail.Id == 0 ? "Добавление товара" : "Редактирование товара";
        _contractId = contractId;
        _documentDate = documentDate;
        _detailId = detail.Id;

        Nomenclature = detail.Nomenclature;
        Quantity = detail.Quantity == 0 ? 1 : detail.Quantity;
        Price = detail.Price;
    }

    [RelayCommand]
    private async Task SelectNomenclatureAsync()
    {
        var vm = _serviceProvider.GetRequiredService<NomenclatureSelectViewModel>();
        await vm.InitializeAsync();
        if (_dialogService.ShowDialog(vm) == true && vm.ConfirmedSelection != null)
        {
            Nomenclature = vm.ConfirmedSelection;

            if (_contractId > 0)
            {
                try
                {
                    var contractRepo = _serviceProvider.GetRequiredService<IRepository<Contract>>();
                    var contract = (await contractRepo.GetAllWithIncludesAsync(default, c => c.PriceType)).FirstOrDefault(c => c.Id == _contractId.Value);
                    if (contract != null && contract.PriceTypeId.HasValue)
                    {
                        var price = await _priceService.GetPriceAsync(Nomenclature.Id, contract.PriceTypeId.Value, _documentDate);
                        if (price > 0) Price = price;
                    }
                }
                catch { }
            }
        }
    }

    [RelayCommand]
    private void ClearNomenclature()
    {
        Nomenclature = null;
        Price = 0;
    }

    [RelayCommand]
    private void Save(Window window)
    {
        if (Nomenclature == null || Quantity <= 0 || Price <= 0)
        {
            MessageBox.Show("Укажите номенклатуру, корректное количество и цену.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Model = new OrderDetail
        {
            Id = _detailId,
            NomenclatureId = Nomenclature.Id,
            Nomenclature = Nomenclature,
            Quantity = Quantity,
            Price = Price,
            Sum = Sum
        };

        if (window != null) window.DialogResult = true;
    }

    [RelayCommand]
    private void Cancel(Window window)
    {
        if (window != null) window.DialogResult = false;
    }
}