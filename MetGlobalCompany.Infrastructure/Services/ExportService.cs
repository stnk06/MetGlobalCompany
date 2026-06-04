using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using MetGlobalCompany.Application.DTOs.Analytics;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Domain.Entities;

namespace MetGlobalCompany.Infrastructure.Services;

public class ExportService : IExportService
{
    public async Task ExportSalesInvoiceToExcelAsync(SalesInvoice invoice, string filePath)
    {
        await Task.Run(() =>
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("УПД");

            worksheet.Cell(1, 1).Value = $"Универсальный передаточный документ № {invoice.Number} от {invoice.Date:dd.MM.yyyy}";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;
            worksheet.Range(1, 1, 1, 6).Merge();

            worksheet.Cell(3, 1).Value = "Покупатель:";
            worksheet.Cell(3, 2).Value = invoice.Contractor?.Name ?? "Не указан";
            worksheet.Cell(4, 1).Value = "Договор:";
            worksheet.Cell(4, 2).Value = invoice.Contract?.Number ?? "Не указан";

            var headerRow = 6;
            worksheet.Cell(headerRow, 1).Value = "№";
            worksheet.Cell(headerRow, 2).Value = "Номенклатура";
            worksheet.Cell(headerRow, 3).Value = "Артикул";
            worksheet.Cell(headerRow, 4).Value = "Кол-во";
            worksheet.Cell(headerRow, 5).Value = "Цена";
            worksheet.Cell(headerRow, 6).Value = "Сумма";

            var rngHeaders = worksheet.Range(headerRow, 1, headerRow, 6);
            rngHeaders.Style.Font.Bold = true;
            rngHeaders.Style.Fill.BackgroundColor = XLColor.LightGray;
            rngHeaders.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rngHeaders.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            var row = headerRow + 1;
            var index = 1;

            foreach (var detail in invoice.Details)
            {
                worksheet.Cell(row, 1).Value = index++;
                worksheet.Cell(row, 2).Value = detail.Nomenclature?.Name;
                worksheet.Cell(row, 3).Value = detail.Nomenclature?.Article;
                worksheet.Cell(row, 4).Value = detail.Quantity;
                worksheet.Cell(row, 5).Value = detail.Price;
                worksheet.Cell(row, 6).Value = detail.Sum;

                var rngData = worksheet.Range(row, 1, row, 6);
                rngData.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                rngData.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                row++;
            }

            worksheet.Cell(row + 1, 5).Value = "ИТОГО:";
            worksheet.Cell(row + 1, 5).Style.Font.Bold = true;
            worksheet.Cell(row + 1, 6).Value = invoice.TotalAmount;
            worksheet.Cell(row + 1, 6).Style.Font.Bold = true;

            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(filePath);
        });
    }

    public async Task ExportSalesReportToExcelAsync(SalesReportDto report, string filePath)
    {
        await Task.Run(() =>
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Анализ продаж");

            // Заголовок отчета
            ws.Cell(1, 1).Value = "Отчет: Анализ продаж (Выручка)";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 16;
            ws.Range(1, 1, 1, 5).Merge();

            var periodStr = "За все время";
            if (report.StartDate.HasValue && report.EndDate.HasValue)
                periodStr = $"За период с {report.StartDate.Value:dd.MM.yyyy} по {report.EndDate.Value:dd.MM.yyyy}";
            else if (report.StartDate.HasValue)
                periodStr = $"С {report.StartDate.Value:dd.MM.yyyy}";
            else if (report.EndDate.HasValue)
                periodStr = $"По {report.EndDate.Value:dd.MM.yyyy}";

            ws.Cell(2, 1).Value = periodStr;
            ws.Cell(2, 1).Style.Font.Italic = true;
            ws.Range(2, 1, 2, 5).Merge();

            // Шапка таблицы
            var hr = 4;
            ws.Cell(hr, 1).Value = "Контрагент / Номенклатура / Документ";
            ws.Cell(hr, 2).Value = "Дата";
            ws.Cell(hr, 3).Value = "Количество";
            ws.Cell(hr, 4).Value = "Цена";
            ws.Cell(hr, 5).Value = "Сумма";

            var rngHeaders = ws.Range(hr, 1, hr, 5);
            rngHeaders.Style.Font.Bold = true;
            rngHeaders.Style.Fill.BackgroundColor = XLColor.FromArgb(230, 230, 230);
            rngHeaders.Style.Border.BottomBorder = XLBorderStyleValues.Medium;

            var r = hr + 1;

            foreach (var cGroup in report.ContractorGroups)
            {
                // Уровень 1: Контрагент (Итоги)
                ws.Cell(r, 1).Value = cGroup.ContractorName;
                ws.Cell(r, 3).Value = cGroup.TotalQuantity;
                ws.Cell(r, 5).Value = cGroup.TotalSum;

                var cRowRng = ws.Range(r, 1, r, 5);
                cRowRng.Style.Font.Bold = true;
                cRowRng.Style.Fill.BackgroundColor = XLColor.FromArgb(245, 245, 245);

                var startCRow = r + 1;
                r++;

                foreach (var nGroup in cGroup.NomenclatureGroups)
                {
                    // Уровень 2: Номенклатура (Итоги)
                    ws.Cell(r, 1).Value = "    " + nGroup.NomenclatureName;
                    ws.Cell(r, 3).Value = nGroup.TotalQuantity;
                    ws.Cell(r, 5).Value = nGroup.TotalSum;

                    var nRowRng = ws.Range(r, 1, r, 5);
                    nRowRng.Style.Font.Italic = true;
                    nRowRng.Style.Font.Bold = true;
                    ws.Row(r).OutlineLevel = 1; // Уровень группировки 1С

                    var startNRow = r + 1;
                    r++;

                    foreach (var item in nGroup.Items)
                    {
                        // Уровень 3: Детальные записи
                        ws.Cell(r, 1).Value = "        УПД № " + item.DocumentNumber;
                        ws.Cell(r, 2).Value = item.DocumentDate.ToString("dd.MM.yyyy HH:mm");
                        ws.Cell(r, 3).Value = item.Quantity;
                        ws.Cell(r, 4).Value = item.Price;
                        ws.Cell(r, 5).Value = item.Sum;

                        ws.Row(r).OutlineLevel = 2; // Уровень группировки 1С
                        r++;
                    }
                }
            }

            // Общие итоги
            ws.Cell(r, 1).Value = "ОБЩИЙ ИТОГ:";
            ws.Cell(r, 3).Value = report.GrandTotalQuantity;
            ws.Cell(r, 5).Value = report.GrandTotalSum;

            var totalRng = ws.Range(r, 1, r, 5);
            totalRng.Style.Font.Bold = true;
            totalRng.Style.Fill.BackgroundColor = XLColor.FromArgb(230, 230, 230);
            totalRng.Style.Border.TopBorder = XLBorderStyleValues.Medium;
            totalRng.Style.Border.BottomBorder = XLBorderStyleValues.Medium;

            ws.Column(1).Width = 50;
            ws.Column(2).Width = 15;
            ws.Column(3).Width = 15;
            ws.Column(4).Width = 15;
            ws.Column(5).Width = 20;

            ws.Columns(3, 5).Style.NumberFormat.Format = "#,##0.00";

            workbook.SaveAs(filePath);
        });
    }
}