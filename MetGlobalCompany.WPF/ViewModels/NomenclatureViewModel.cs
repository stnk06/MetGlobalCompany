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

public partial class NomenclatureViewModel : BaseViewModel
{
    private readonly IRepository<NomenclatureCategory> _categoryRepository;
    private readonly IRepository<Nomenclature> _nomenclatureRepository;
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;

    private List<Nomenclature> _allNomenclatures = new();

    [ObservableProperty]
    private ObservableCollection<NomenclatureCategory> _rootCategories = new();

    [ObservableProperty]
    private ObservableCollection<Nomenclature> _nomenclatures = new();

    [ObservableProperty]
    private NomenclatureCategory? _selectedCategory;

    [ObservableProperty]
    private Nomenclature? _selectedNomenclature;

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) ApplyFilter(); }
    }

    public NomenclatureViewModel(
        IRepository<NomenclatureCategory> categoryRepository,
        IRepository<Nomenclature> nomenclatureRepository,
        IDialogService dialogService,
        IServiceProvider serviceProvider)
    {
        _categoryRepository = categoryRepository;
        _nomenclatureRepository = nomenclatureRepository;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;
        Title = "Справочник: Номенклатура";

        _ = LoadDataAsync();
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Nomenclatures = new ObservableCollection<Nomenclature>(_allNomenclatures);
            return;
        }

        var search = SearchText.ToLower();
        var filtered = _allNomenclatures.Where(n =>
            n.Name.ToLower().Contains(search) ||
            (n.Article != null && n.Article.ToLower().Contains(search))
        ).ToList();

        Nomenclatures = new ObservableCollection<Nomenclature>(filtered);
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var allCategories = await _categoryRepository.GetAllAsync();
            var rootNodes = allCategories.Where(c => c.ParentId == null).ToList();

            // Создаем виртуальную глобальную группу
            var globalRoot = new NomenclatureCategory { Id = -1, Name = "Вся номенклатура" };
            foreach (var node in rootNodes) globalRoot.Children.Add(node);

            RootCategories = new ObservableCollection<NomenclatureCategory> { globalRoot };

            // Если ничего не выбрано, выбираем глобальную группу
            if (SelectedCategory == null)
                SelectedCategory = globalRoot;

            await LoadNomenclaturesByCategoryAsync(SelectedCategory.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsBusy = false; }
    }

    public void SetSelectedCategory(NomenclatureCategory? category)
    {
        SelectedCategory = category;
        if (category != null)
        {
            _ = LoadNomenclaturesByCategoryAsync(category.Id);
        }
    }

    private async Task LoadNomenclaturesByCategoryAsync(int categoryId)
    {
        IsBusy = true;
        try
        {
            if (categoryId == -1)
                _allNomenclatures = (await _nomenclatureRepository.GetAllAsync()).OrderBy(n => n.Name).ToList();
            else
                _allNomenclatures = (await _nomenclatureRepository.GetAsync(n => n.CategoryId == categoryId)).OrderBy(n => n.Name).ToList();

            ApplyFilter();
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task AddRootCategoryAsync()
    {
        var formVm = _serviceProvider.GetRequiredService<NomenclatureCategoryFormViewModel>();
        formVm.Initialize(new NomenclatureCategory());

        if (_dialogService.ShowDialog(formVm) == true)
        {
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task EditCategoryAsync()
    {
        if (SelectedCategory == null || SelectedCategory.Id == -1)
        {
            MessageBox.Show("Пожалуйста, выберите конкретную папку для редактирования (не 'Вся номенклатура').", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var formVm = _serviceProvider.GetRequiredService<NomenclatureCategoryFormViewModel>();
        formVm.Initialize(SelectedCategory);

        if (_dialogService.ShowDialog(formVm) == true)
        {
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task AddNomenclatureAsync()
    {
        if (SelectedCategory == null || SelectedCategory.Id == -1)
        {
            MessageBox.Show("Сначала выберите конкретную группу (папку) слева, в которую хотите добавить позицию.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var formVm = _serviceProvider.GetRequiredService<NomenclatureFormViewModel>();
        await formVm.InitializeAsync(new Nomenclature { CategoryId = SelectedCategory.Id, IsService = false });

        if (_dialogService.ShowDialog(formVm) == true)
        {
            await LoadNomenclaturesByCategoryAsync(SelectedCategory.Id);
        }
    }

    [RelayCommand]
    private async Task EditNomenclatureAsync()
    {
        if (SelectedNomenclature == null) return;

        var formVm = _serviceProvider.GetRequiredService<NomenclatureFormViewModel>();
        await formVm.InitializeAsync(SelectedNomenclature);

        if (_dialogService.ShowDialog(formVm) == true && SelectedCategory != null)
        {
            await LoadNomenclaturesByCategoryAsync(SelectedCategory.Id);
        }
    }
}