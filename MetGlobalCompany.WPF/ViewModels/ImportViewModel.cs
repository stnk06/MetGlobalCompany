using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MetGlobalCompany.Application.DTOs;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.WPF.Enums;
using Microsoft.Win32;

namespace MetGlobalCompany.WPF.ViewModels;

public partial class ImportViewModel : BaseViewModel
{
    private readonly IExcelImportService _importService;

    [ObservableProperty]
    private Array _importTargets = Enum.GetValues(typeof(ImportTarget));

    [ObservableProperty]
    private ImportTarget _selectedTarget = ImportTarget.Units;

    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private string _logText = string.Empty;

    public ImportViewModel(IExcelImportService importService)
    {
        _importService = importService;
        Title = "Импорт данных из Excel";
    }

    [RelayCommand]
    private void SelectFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Excel Files|*.xlsx;*.xls",
            Title = "Выберите файл для импорта"
        };

        if (dialog.ShowDialog() == true)
        {
            FilePath = dialog.FileName;
        }
    }

    [RelayCommand]
    private async Task ImportAsync()
    {
        if (string.IsNullOrWhiteSpace(FilePath))
        {
            MessageBox.Show("Выберите файл для загрузки.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsBusy = true;
        LogText = "Выполняется загрузка данных. Пожалуйста, подождите...\n";

        try
        {
            ImportReportDto report = null!;

            switch (SelectedTarget)
            {
                case ImportTarget.Units:
                    report = await _importService.ImportUnitsAsync(FilePath);
                    break;
                case ImportTarget.PriceTypes:
                    report = await _importService.ImportPriceTypesAsync(FilePath);
                    break;
                case ImportTarget.Contractors:
                    report = await _importService.ImportContractorsAsync(FilePath);
                    break;
                case ImportTarget.Nomenclatures:
                    report = await _importService.ImportNomenclaturesAsync(FilePath);
                    break;
                case ImportTarget.Contracts:
                    report = await _importService.ImportContractsAsync(FilePath);
                    break;
                case ImportTarget.PriceSettings:
                    report = await _importService.ImportPriceSettingsAsync(FilePath);
                    break;
                case ImportTarget.Payments:
                    report = await _importService.ImportPaymentsAsync(FilePath);
                    break;
            }

            if (report != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Импорт успешно завершен.");
                sb.AppendLine(new string('-', 30));
                sb.AppendLine($"Добавлено новых записей: {report.CreatedCount}");
                sb.AppendLine($"Обновлено существующих: {report.UpdatedCount}");
                sb.AppendLine($"Количество ошибок: {report.ErrorsCount}");

                if (report.ErrorsCount > 0)
                {
                    sb.AppendLine(new string('-', 30));
                    sb.AppendLine("Лог ошибок:");
                    foreach (var err in report.Errors)
                    {
                        sb.AppendLine(err);
                    }
                }

                LogText = sb.ToString();
            }
        }
        catch (Exception ex)
        {
            LogText = $"Критическая ошибка при импорте:\n{ex.Message}\n\nТрассировка:\n{ex.StackTrace}";
            MessageBox.Show($"Произошла ошибка при обработке файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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