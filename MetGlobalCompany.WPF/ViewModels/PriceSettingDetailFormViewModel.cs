using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace MetGlobalCompany.WPF.ViewModels;

public partial class PriceSettingDetailFormViewModel : BaseViewModel
{
    private readonly IRepository<PriceType> _priceTypeRepository;
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;

    private int _detailId;

    [ObservableProperty] private Nomenclature? _nomenclature;
    [ObservableProperty] private PriceType? _priceType;
    [ObservableProperty] private decimal _price;

    [ObservableProperty] private ObservableCollection<PriceType> _priceTypes = new();

    public PriceSettingDetail Model { get; private set; } = new();

    public PriceSettingDetailFormViewModel(
        IRepository<PriceType> priceTypeRepository,
        IDialogService dialogService,
        IServiceProvider serviceProvider)
    {
        _priceTypeRepository = priceTypeRepository;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;
    }

    public async Task InitializeAsync(PriceSettingDetail detail)
    {
        Title = detail.Id == 0 ? "Добавление цены" : "Редактирование цены";
        _detailId = detail.Id;

        PriceTypes = new ObservableCollection<PriceType>(await _priceTypeRepository.GetAllAsync());

        Nomenclature = detail.Nomenclature;
        Price = detail.Price;

        if (detail.PriceTypeId > 0)
        {
            foreach (var pt in PriceTypes)
            {
                if (pt.Id == detail.PriceTypeId)
                {
                    PriceType = pt;
                    break;
                }
            }
        }
    }

    [RelayCommand]
    private async Task SelectNomenclatureAsync()
    {
        var vm = _serviceProvider.GetRequiredService<NomenclatureSelectViewModel>();
        await vm.InitializeAsync();
        if (_dialogService.ShowDialog(vm) == true && vm.ConfirmedSelection != null)
        {
            Nomenclature = vm.ConfirmedSelection;
        }
    }

    [RelayCommand]
    private void ClearNomenclature()
    {
        Nomenclature = null;
    }

    [RelayCommand]
    private void Save(Window window)
    {
        if (Nomenclature == null || PriceType == null || Price < 0)
        {
            MessageBox.Show("Укажите номенклатуру, тип цены и корректное значение цены.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Model = new PriceSettingDetail
        {
            Id = _detailId,
            NomenclatureId = Nomenclature.Id,
            Nomenclature = Nomenclature,
            PriceTypeId = PriceType.Id,
            PriceType = PriceType,
            Price = Price
        };

        if (window != null) window.DialogResult = true;
    }

    [RelayCommand]
    private void Cancel(Window window)
    {
        if (window != null) window.DialogResult = false;
    }
}