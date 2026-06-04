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

public partial class PriceSettingsViewModel : BaseViewModel
{
    private readonly IRepository<PriceSetting> _priceSettingRepository;
    private readonly IDocumentPostingService _postingService;
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;

    private List<PriceSetting> _allPriceSettings = new();

    [ObservableProperty]
    private ObservableCollection<PriceSetting> _priceSettings = new();

    [ObservableProperty]
    private PriceSetting? _selectedPriceSetting;

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ApplyFilter();
            }
        }
    }

    public PriceSettingsViewModel(
        IRepository<PriceSetting> priceSettingRepository,
        IDocumentPostingService postingService,
        IDialogService dialogService,
        IServiceProvider serviceProvider)
    {
        _priceSettingRepository = priceSettingRepository;
        _postingService = postingService;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;

        Title = "Журнал: Установка цен номенклатуры";
        _ = LoadDataAsync();
    }

    private void ApplyFilter()
    {
        var filtered = _allPriceSettings.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(p =>
                p.Number.ToLower().Contains(search) ||
                (p.Comment != null && p.Comment.ToLower().Contains(search))
            );
        }

        PriceSettings = new ObservableCollection<PriceSetting>(filtered.ToList());
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var data = await _priceSettingRepository.GetAllWithIncludesAsync(default, p => p.Details);
            _allPriceSettings = data.OrderByDescending(p => p.Date).ToList();
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
        var formVm = _serviceProvider.GetRequiredService<PriceSettingFormViewModel>();
        await formVm.InitializeAsync(new PriceSetting { Date = DateTime.Now, Number = $"УЦ-{DateTime.Now:yyyyMMdd-HHmm}" });

        if (_dialogService.ShowDialog(formVm) == true) await LoadDataAsync();
    }

    [RelayCommand]
    private async Task EditCurrentAsync()
    {
        if (SelectedPriceSetting == null) return;
        var formVm = _serviceProvider.GetRequiredService<PriceSettingFormViewModel>();
        await formVm.InitializeAsync(SelectedPriceSetting);

        if (_dialogService.ShowDialog(formVm) == true) await LoadDataAsync();
    }

    [RelayCommand]
    private async Task PostDocumentAsync()
    {
        if (SelectedPriceSetting == null || SelectedPriceSetting.Id == 0 || SelectedPriceSetting.IsPosted) return;
        IsBusy = true;
        try
        {
            if (await _postingService.PostPriceSettingAsync(SelectedPriceSetting.Id))
            {
                MessageBox.Show("Цены зафиксированы в регистре.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadDataAsync();
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task UnpostDocumentAsync()
    {
        if (SelectedPriceSetting == null || SelectedPriceSetting.Id == 0 || !SelectedPriceSetting.IsPosted) return;
        IsBusy = true;
        try
        {
            if (await _postingService.UnpostPriceSettingAsync(SelectedPriceSetting.Id))
            {
                MessageBox.Show("Проведение отменено. Цены изъяты из регистра.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadDataAsync();
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        finally { IsBusy = false; }
    }
}