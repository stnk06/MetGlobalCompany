using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Domain.Entities;

namespace MetGlobalCompany.WPF.ViewModels;

public partial class PriceTypeFormViewModel : BaseViewModel
{
    private readonly IRepository<PriceType> _priceTypeRepository;

    [ObservableProperty]
    private PriceType _model = new();

    public PriceTypeFormViewModel(IRepository<PriceType> priceTypeRepository)
    {
        _priceTypeRepository = priceTypeRepository;
    }

    public void Initialize(PriceType priceType)
    {
        Model = priceType;
        Title = priceType.Id == 0 ? "Создание типа цен" : "Редактирование типа цен";
    }

    [RelayCommand]
    private async Task SaveAsync(Window window)
    {
        if (string.IsNullOrWhiteSpace(Model.Name))
        {
            MessageBox.Show("Заполните наименование типа цен.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            if (Model.Id == 0)
                await _priceTypeRepository.AddAsync(Model);
            else
                await _priceTypeRepository.UpdateAsync(Model);

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