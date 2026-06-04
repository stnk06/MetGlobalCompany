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

public partial class ContractsViewModel : BaseViewModel
{
    private readonly IRepository<Contract> _contractRepository;
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;

    private List<Contract> _allContracts = new();

    [ObservableProperty]
    private ObservableCollection<Contract> _contracts = new();

    [ObservableProperty]
    private Contract? _selectedContract;

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) ApplyFilter(); }
    }

    // ВЫПАДАЮЩИЙ ФИЛЬТР: Контрагенты
    public ObservableCollection<string> AvailableContractors { get; } = new();

    [ObservableProperty]
    private string? _selectedContractorFilter;
    partial void OnSelectedContractorFilterChanged(string? value) => ApplyFilter();

    public ContractsViewModel(IRepository<Contract> contractRepository, IDialogService dialogService, IServiceProvider serviceProvider)
    {
        _contractRepository = contractRepository;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;
        Title = "Справочник: Договоры контрагентов";

        _ = LoadDataAsync();
    }

    private void ApplyFilter()
    {
        var filtered = _allContracts.AsEnumerable();

        if (!string.IsNullOrEmpty(SelectedContractorFilter))
            filtered = filtered.Where(c => c.Contractor?.Name == SelectedContractorFilter);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(c =>
                (c.Name != null && c.Name.ToLower().Contains(search)) ||
                c.Number.ToLower().Contains(search) ||
                (c.Contractor != null && c.Contractor.Name.ToLower().Contains(search))
            );
        }

        Contracts = new ObservableCollection<Contract>(filtered.ToList());
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            _allContracts = (await _contractRepository.GetAllWithIncludesAsync(default, c => c.Contractor)).OrderByDescending(c => c.Date).ToList();

            // Заполнение выпадающего списка доступными контрагентами
            AvailableContractors.Clear();
            var contractors = _allContracts.Where(c => c.Contractor != null).Select(c => c.Contractor.Name).Distinct().OrderBy(n => n);
            foreach (var c in contractors) AvailableContractors.Add(c);

            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsBusy = false; }
    }

    // ИСПРАВЛЕНИЕ: Безопасный асинхронный вызов формы с try-catch
    [RelayCommand]
    private async Task AddNewAsync()
    {
        try
        {
            var formVm = _serviceProvider.GetRequiredService<ContractFormViewModel>();
            await formVm.InitializeAsync(new Contract { Date = DateTime.Now, CurrencyCode = "RUB", IsActive = true });

            if (_dialogService.ShowDialog(formVm) == true)
            {
                await LoadDataAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при открытии формы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task EditCurrentAsync()
    {
        if (SelectedContract == null) return;
        try
        {
            var formVm = _serviceProvider.GetRequiredService<ContractFormViewModel>();
            await formVm.InitializeAsync(SelectedContract);

            if (_dialogService.ShowDialog(formVm) == true)
            {
                await LoadDataAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при открытии формы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}