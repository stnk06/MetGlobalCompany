using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Domain.Entities;
using Microsoft.Win32;

namespace MetGlobalCompany.WPF.ViewModels;

public partial class Torg12PreviewViewModel : BaseViewModel
{
    private readonly IWordExportService _wordExportService;

    [ObservableProperty]
    private SalesInvoice _invoice = new();

    [ObservableProperty]
    private decimal _totalQuantity;

    [ObservableProperty]
    private decimal _totalSum;

    public Torg12PreviewViewModel(IWordExportService wordExportService)
    {
        _wordExportService = wordExportService;
        Title = "Печатная форма: ТОРГ-12";
    }

    public Task InitializeAsync(SalesInvoice invoice)
    {
        Invoice = invoice;
        TotalQuantity = invoice.Details.Sum(d => d.Quantity);
        TotalSum = invoice.Details.Sum(d => d.Sum);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ExportToWordAsync()
    {
        var dialog = new SaveFileDialog
        {
            FileName = $"ТОРГ-12_{Invoice.Number.Replace("/", "_")}.docx",
            DefaultExt = ".docx",
            Filter = "Word Document|*.docx"
        };

        if (dialog.ShowDialog() == true)
        {
            IsBusy = true;
            try
            {
                await _wordExportService.ExportTorg12Async(Invoice, dialog.FileName);
                MessageBox.Show("Документ успешно экспортирован в Word!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    [RelayCommand]
    private void Print(Visual visualToPrint)
    {
        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() == true)
        {
            printDialog.PrintVisual(visualToPrint, $"ТОРГ-12 {Invoice.Number}");
        }
    }

    [RelayCommand]
    private void Close(Window window)
    {
        if (window != null)
        {
            window.DialogResult = true;
        }
    }
}