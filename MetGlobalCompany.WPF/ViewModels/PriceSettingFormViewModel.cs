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

public partial class PriceSettingFormViewModel : BaseViewModel
{
    private readonly IRepository<PriceSetting> _priceSettingRepository;
    private readonly IRepository<PriceSettingDetail> _detailRepository;
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;

    private readonly List<PriceSettingDetail> _detailsToDelete = new();

    [ObservableProperty]
    private PriceSetting _model = new();

    [ObservableProperty]
    private ObservableCollection<PriceSettingDetail> _currentDetails = new();

    [ObservableProperty]
    private PriceSettingDetail? _selectedDetail;

    public bool CanEdit => !Model.IsPosted;

    public PriceSettingFormViewModel(
        IRepository<PriceSetting> priceSettingRepository,
        IRepository<PriceSettingDetail> detailRepository,
        IDialogService dialogService,
        IServiceProvider serviceProvider)
    {
        _priceSettingRepository = priceSettingRepository;
        _detailRepository = detailRepository;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;
    }

    public Task InitializeAsync(PriceSetting priceSetting)
    {
        Model = priceSetting;
        Title = priceSetting.Id == 0 ? "Установка цен номенклатуры" : $"Установка цен № {priceSetting.Number} от {priceSetting.Date:dd.MM.yyyy}";

        CurrentDetails = new ObservableCollection<PriceSettingDetail>(priceSetting.Details);
        _detailsToDelete.Clear();

        OnPropertyChanged(nameof(CanEdit));
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task AddDetailAsync()
    {
        if (!CanEdit) return;
        var vm = _serviceProvider.GetRequiredService<PriceSettingDetailFormViewModel>();
        await vm.InitializeAsync(new PriceSettingDetail { Price = 0 });

        if (_dialogService.ShowDialog(vm) == true)
        {
            CurrentDetails.Add(vm.Model);
        }
    }

    [RelayCommand]
    private async Task EditDetailAsync(PriceSettingDetail detail)
    {
        if (!CanEdit || detail == null) return;

        var clone = new PriceSettingDetail
        {
            Id = detail.Id,
            NomenclatureId = detail.NomenclatureId,
            Nomenclature = detail.Nomenclature,
            PriceTypeId = detail.PriceTypeId,
            PriceType = detail.PriceType,
            Price = detail.Price,
            PriceSettingId = detail.PriceSettingId
        };

        var vm = _serviceProvider.GetRequiredService<PriceSettingDetailFormViewModel>();
        await vm.InitializeAsync(clone);

        if (_dialogService.ShowDialog(vm) == true)
        {
            var index = CurrentDetails.IndexOf(detail);
            CurrentDetails.RemoveAt(index);
            CurrentDetails.Insert(index, vm.Model);
            SelectedDetail = vm.Model;
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
        }
    }

    [RelayCommand]
    private async Task SaveAsync(Window window)
    {
        if (!CanEdit) return;

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
            foreach (var detail in Model.Details)
            {
                detail.Nomenclature = null!;
                detail.PriceType = null!;
                detail.PriceSetting = null!;
            }

            if (Model.Id == 0) await _priceSettingRepository.AddAsync(Model);
            else await _priceSettingRepository.UpdateAsync(Model);

            if (window != null) window.DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при сохранении: {ex.InnerException?.Message ?? ex.Message}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel(Window window)
    {
        if (window != null) window.DialogResult = false;
    }
}