using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Domain.Entities;
using MetGlobalCompany.Domain.Enums;

namespace MetGlobalCompany.WPF.ViewModels;

public partial class ContractorFormViewModel : BaseViewModel
{
    private readonly IRepository<Contractor> _contractorRepository;
    private readonly IDadataService _dadataService;

    [ObservableProperty]
    private Contractor _model = new();

    [ObservableProperty]
    private string _searchInnText = string.Empty;

    public Array ContractorTypes => Enum.GetValues(typeof(ContractorType));

    public ContractorFormViewModel(IRepository<Contractor> contractorRepository, IDadataService dadataService)
    {
        _contractorRepository = contractorRepository;
        _dadataService = dadataService;
    }

    public void Initialize(Contractor contractor)
    {
        Model = contractor;
        Title = contractor.Id == 0 ? "Создание контрагента" : $"Редактирование: {contractor.Name}";
    }

    [RelayCommand]
    private async Task SearchByInnAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchInnText) || SearchInnText.Length < 10)
        {
            MessageBox.Show("Введите корректный ИНН (10 или 12 цифр).", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            var dto = await _dadataService.GetContractorByInnAsync(SearchInnText);
            if (dto != null)
            {
                Model.Inn = dto.Inn;
                Model.Kpp = dto.Kpp;
                Model.Ogrn = dto.Ogrn;
                Model.Name = dto.ShortName;
                Model.FullName = dto.FullName;
                Model.LegalAddress = dto.LegalAddress;
                OnPropertyChanged(nameof(Model));
            }
            else
            {
                MessageBox.Show("Контрагент по данному ИНН не найден в базе ФНС.", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка связи с сервисом Dadata: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync(Window window)
    {
        if (string.IsNullOrWhiteSpace(Model.Name))
        {
            MessageBox.Show("Наименование контрагента не может быть пустым.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!string.IsNullOrWhiteSpace(Model.Email) && !Regex.IsMatch(Model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            MessageBox.Show("Введите корректный адрес электронной почты.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            if (Model.Id == 0)
                await _contractorRepository.AddAsync(Model);
            else
                await _contractorRepository.UpdateAsync(Model);

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