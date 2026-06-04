using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace MetGlobalCompany.WPF.ViewModels;

public partial class NomenclatureFormViewModel : BaseViewModel
{
    private readonly IRepository<Nomenclature> _nomenclatureRepository;
    private readonly IRepository<Unit> _unitRepository;

    [ObservableProperty]
    private Nomenclature _model = new();

    [ObservableProperty]
    private ObservableCollection<Unit> _units = new();

    [ObservableProperty]
    private ObservableCollection<string> _gostPrefixes = new() { "ГОСТ", "ТУ", "Нет" };

    [ObservableProperty]
    private string _selectedGostPrefix = "Нет";

    [ObservableProperty]
    private string _gostValue = string.Empty;

    public NomenclatureFormViewModel(IRepository<Nomenclature> nomenclatureRepository, IRepository<Unit> unitRepository)
    {
        _nomenclatureRepository = nomenclatureRepository;
        _unitRepository = unitRepository;
    }

    public async Task InitializeAsync(Nomenclature nomenclature)
    {
        Model = nomenclature;
        Title = nomenclature.Id == 0 ? "Создание номенклатуры" : $"Редактирование: {nomenclature.Name}";

        if (!string.IsNullOrEmpty(Model.Gost))
        {
            if (Model.Gost.StartsWith("ГОСТ "))
            {
                SelectedGostPrefix = "ГОСТ";
                GostValue = Model.Gost.Substring(5);
            }
            else if (Model.Gost.StartsWith("ТУ "))
            {
                SelectedGostPrefix = "ТУ";
                GostValue = Model.Gost.Substring(3);
            }
            else
            {
                SelectedGostPrefix = "Нет";
                GostValue = Model.Gost;
            }
        }

        var units = await _unitRepository.GetAllAsync();
        Units = new ObservableCollection<Unit>(units);
    }

    [RelayCommand]
    private async Task SaveAsync(Window window)
    {
        if (string.IsNullOrWhiteSpace(Model.Name) || Model.UnitId == 0)
        {
            MessageBox.Show("Заполните обязательные поля: Наименование и Единица измерения.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (SelectedGostPrefix != "Нет" && !string.IsNullOrWhiteSpace(GostValue))
            Model.Gost = $"{SelectedGostPrefix} {GostValue.Trim()}";
        else
            Model.Gost = GostValue?.Trim();

        IsBusy = true;
        try
        {
            if (Model.Id == 0)
                await _nomenclatureRepository.AddAsync(Model);
            else
                await _nomenclatureRepository.UpdateAsync(Model);

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