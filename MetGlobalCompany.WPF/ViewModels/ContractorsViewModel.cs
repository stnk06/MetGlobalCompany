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
using MetGlobalCompany.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace MetGlobalCompany.WPF.ViewModels;

public partial class ContractorsViewModel : BaseViewModel
{
    private readonly IRepository<Contractor> _contractorRepository;
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;

    private List<Contractor> _allContractors = new();

    [ObservableProperty]
    private ObservableCollection<Contractor> _contractors = new();

    [ObservableProperty]
    private Contractor? _selectedContractor;

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) ApplyFilter(); }
    }

    public ObservableCollection<ContractorType> AvailableTypes { get; } = new();
    [ObservableProperty] private ContractorType? _selectedTypeFilter;
    partial void OnSelectedTypeFilterChanged(ContractorType? value) => ApplyFilter();

    public ContractorsViewModel(IRepository<Contractor> contractorRepository, IDialogService dialogService, IServiceProvider serviceProvider)
    {
        _contractorRepository = contractorRepository;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;
        Title = "Справочник: Контрагенты";

        _ = LoadDataAsync();
    }

    private void ApplyFilter()
    {
        var filtered = _allContractors.AsEnumerable();

        if (SelectedTypeFilter.HasValue)
            filtered = filtered.Where(c => c.Type == SelectedTypeFilter.Value);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(c =>
                c.Name.ToLower().Contains(search) ||
                (c.Inn != null && c.Inn.Contains(search)));
        }

        Contractors = new ObservableCollection<Contractor>(filtered.ToList());
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            _allContractors = (await _contractorRepository.GetAllAsync()).OrderBy(c => c.Name).ToList();

            AvailableTypes.Clear();
            foreach (var type in _allContractors.Select(c => c.Type).Distinct()) AvailableTypes.Add(type);

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
            var formVm = _serviceProvider.GetRequiredService<ContractorFormViewModel>();
            formVm.Initialize(new Contractor { Type = ContractorType.Buyer });

            if (_dialogService.ShowDialog(formVm) == true) await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка инициализации формы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task EditCurrentAsync()
    {
        if (SelectedContractor == null) return;
        try
        {
            var formVm = _serviceProvider.GetRequiredService<ContractorFormViewModel>();
            formVm.Initialize(SelectedContractor);

            if (_dialogService.ShowDialog(formVm) == true) await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка инициализации формы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}