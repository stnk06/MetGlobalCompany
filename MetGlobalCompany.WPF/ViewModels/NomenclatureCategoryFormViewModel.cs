using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Domain.Entities;

namespace MetGlobalCompany.WPF.ViewModels;

public partial class NomenclatureCategoryFormViewModel : BaseViewModel
{
    private readonly IRepository<NomenclatureCategory> _categoryRepository;

    [ObservableProperty]
    private NomenclatureCategory _model = new();

    public NomenclatureCategoryFormViewModel(IRepository<NomenclatureCategory> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public void Initialize(NomenclatureCategory category)
    {
        Model = category;
        Title = category.Id == 0 ? "Создание новой группы" : "Переименование группы";
    }

    [RelayCommand]
    private async Task SaveAsync(Window window)
    {
        if (string.IsNullOrWhiteSpace(Model.Name))
        {
            MessageBox.Show("Введите название группы.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            if (Model.Id == 0)
                await _categoryRepository.AddAsync(Model);
            else
                await _categoryRepository.UpdateAsync(Model);

            if (window != null) window.DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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