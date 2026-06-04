using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Spreadsheet;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MetGlobalCompany.WPF.ViewModels;

public partial class ContractSelectViewModel : BaseViewModel
{
    private readonly IRepository<Contract> _contractRepository;
    private List<Contract> _allItems = new();

    [ObservableProperty]
    private ObservableCollection<Contract> _items = new();

    [ObservableProperty]
    private Contract? _selectedItem;

    public Contract? ConfirmedSelection { get; private set; }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) ApplyFilter(); }
    }

    public ContractSelectViewModel(IRepository<Contract> contractRepository)
    {
        _contractRepository = contractRepository;
        Title = "Выбор договора";
    }

    public async Task InitializeAsync(int? contractorId = null)
    {
        IsBusy = true;
        try
        {
            var data = await _contractRepository.GetAllWithIncludesAsync(default, c => c.Contractor, c => c.PriceType);
            if (contractorId.HasValue)
            {
                data = data.Where(c => c.ContractorId == contractorId.Value).ToList();
            }

            _allItems = data.OrderByDescending(c => c.Date).ToList();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка загрузки", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        var filtered = _allItems.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(c =>
                c.Number.ToLower().Contains(search) ||
                (c.Name != null && c.Name.ToLower().Contains(search))
            );
        }

        Items = new ObservableCollection<Contract>(filtered);
    }

    [RelayCommand]
    private void ConfirmSelection(Window window)
    {
        if (SelectedItem == null)
        {
            MessageBox.Show("Выберите позицию из списка.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ConfirmedSelection = SelectedItem;
        if (window != null) window.DialogResult = true;
    }

    [RelayCommand]
    private void Cancel(Window window)
    {
        ConfirmedSelection = null;
        if (window != null) window.DialogResult = false;
    }
}