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

public partial class NomenclatureSelectViewModel : BaseViewModel
{
    private readonly IRepository<Nomenclature> _nomenclatureRepository;
    private List<Nomenclature> _allItems = new();

    [ObservableProperty]
    private ObservableCollection<Nomenclature> _items = new();

    [ObservableProperty]
    private Nomenclature? _selectedItem;

    public Nomenclature? ConfirmedSelection { get; private set; }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) ApplyFilter(); }
    }

    public NomenclatureSelectViewModel(IRepository<Nomenclature> nomenclatureRepository)
    {
        _nomenclatureRepository = nomenclatureRepository;
        Title = "Выбор номенклатуры";
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            _allItems = (await _nomenclatureRepository.GetAllWithIncludesAsync(default, n => n.Unit, n => n.Category)).OrderBy(n => n.Name).ToList();
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
            filtered = filtered.Where(n =>
                n.Name.ToLower().Contains(search) ||
                (n.Article != null && n.Article.ToLower().Contains(search)) ||
                (n.Category != null && n.Category.Name.ToLower().Contains(search))
            );
        }

        Items = new ObservableCollection<Nomenclature>(filtered);
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