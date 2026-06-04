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

public partial class ContractorSelectViewModel : BaseViewModel
{
    private readonly IRepository<Contractor> _contractorRepository;
    private List<Contractor> _allItems = new();

    [ObservableProperty]
    private ObservableCollection<Contractor> _items = new();

    [ObservableProperty]
    private Contractor? _selectedItem;

    public Contractor? ConfirmedSelection { get; private set; }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) ApplyFilter(); }
    }

    public ContractorSelectViewModel(IRepository<Contractor> contractorRepository)
    {
        _contractorRepository = contractorRepository;
        Title = "Выбор контрагента";
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            _allItems = (await _contractorRepository.GetAllAsync()).OrderBy(c => c.Name).ToList();
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
                c.Name.ToLower().Contains(search) ||
                (c.Inn != null && c.Inn.ToLower().Contains(search))
            );
        }

        Items = new ObservableCollection<Contractor>(filtered);
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