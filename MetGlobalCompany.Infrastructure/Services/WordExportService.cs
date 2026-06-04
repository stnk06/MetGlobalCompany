using System;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Domain.Entities;

namespace MetGlobalCompany.Infrastructure.Services;

public class WordExportService : IWordExportService
{
    public async Task ExportTorg12Async(SalesInvoice invoice, string filePath)
    {
        await Task.Run(() =>
        {
            using var wordDocument = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);
            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            var sectionProps = new SectionProperties();
            var pageSize = new PageSize { Width = 16838, Height = 11906, Orient = PageOrientationValues.Landscape };
            var pageMargin = new PageMargin { Top = 720, Bottom = 720, Left = 720, Right = 720, Header = 0, Footer = 0, Gutter = 0 };
            sectionProps.Append(pageSize, pageMargin);
            body.Append(sectionProps);

            var titleProps = new ParagraphProperties(new Justification { Val = JustificationValues.Right });
            var titlePara = new Paragraph(titleProps);
            var titleRun = new Run(new Text("Унифицированная форма № ТОРГ-12\nУтверждена постановлением Госкомстата России от 25.12.98 № 132"));
            titleRun.RunProperties = new RunProperties(new FontSize { Val = "16" });
            titlePara.Append(titleRun);
            body.Append(titlePara);

            var headerTable = new Table();
            var headerTableProps = new TableProperties(new TableWidth { Type = TableWidthUnitValues.Pct, Width = "5000" });
            headerTable.AppendChild(headerTableProps);

            var tr1 = new TableRow();
            tr1.Append(CreateCell("Грузоотправитель:", true));
            tr1.Append(CreateCell(invoice.Contractor?.FullName ?? invoice.Contractor?.Name ?? string.Empty, false));
            headerTable.Append(tr1);

            var tr2 = new TableRow();
            tr2.Append(CreateCell("Грузополучатель:", true));
            tr2.Append(CreateCell(invoice.Contractor?.FullName ?? invoice.Contractor?.Name ?? string.Empty, false));
            headerTable.Append(tr2);

            var tr3 = new TableRow();
            tr3.Append(CreateCell("Поставщик:", true));
            tr3.Append(CreateCell("ООО 'МЕТГЛОБАЛ'", false));
            headerTable.Append(tr3);

            var tr4 = new TableRow();
            tr4.Append(CreateCell("Плательщик:", true));
            tr4.Append(CreateCell(invoice.Contractor?.FullName ?? invoice.Contractor?.Name ?? string.Empty, false));
            headerTable.Append(tr4);

            var tr5 = new TableRow();
            tr5.Append(CreateCell("Основание:", true));
            tr5.Append(CreateCell($"Договор {invoice.Contract?.Number} от {invoice.Contract?.Date:dd.MM.yyyy}", false));
            headerTable.Append(tr5);

            body.Append(headerTable);
            body.Append(new Paragraph(new Run(new Text(""))));

            var docTitleProps = new ParagraphProperties(new Justification { Val = JustificationValues.Center });
            var docTitlePara = new Paragraph(docTitleProps);
            var docTitleRun = new Run(new Text($"ТОВАРНАЯ НАКЛАДНАЯ № {invoice.Number} от {invoice.Date:dd.MM.yyyy}"));
            docTitleRun.RunProperties = new RunProperties(new Bold(), new FontSize { Val = "28" });
            docTitlePara.Append(docTitleRun);
            body.Append(docTitlePara);
            body.Append(new Paragraph(new Run(new Text(""))));

            var dataTable = new Table();
            var dataTableProps = new TableProperties(
                new TableBorders(
                    new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                    new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                    new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                    new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                    new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                    new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
                ),
                new TableWidth { Type = TableWidthUnitValues.Pct, Width = "5000" }
            );
            dataTable.AppendChild(dataTableProps);

            var headerRow1 = new TableRow();
            string[] headers = { "Номер\nпо\nпорядку", "Товар\n(наименование, характеристика,\nсорт, артикул)", "Код", "Ед. изм.", "Количество\n(масса\nнетто)", "Цена,\nруб. коп.", "Сумма без\nучета НДС,\nруб. коп.", "НДС\nставка %", "Сумма с\nучетом НДС,\nруб. коп." };
            foreach (var header in headers)
            {
                var cell = new TableCell();
                var para = new Paragraph(new ParagraphProperties(new Justification { Val = JustificationValues.Center }));
                var run = new Run(new Text(header));
                run.RunProperties = new RunProperties(new Bold(), new FontSize { Val = "16" });
                para.Append(run);
                cell.Append(para);
                headerRow1.Append(cell);
            }
            dataTable.Append(headerRow1);

            int index = 1;
            decimal totalQuantity = 0;
            decimal totalSum = 0;

            foreach (var detail in invoice.Details)
            {
                var row = new TableRow();
                row.Append(CreateDataCell(index.ToString(), JustificationValues.Center));
                row.Append(CreateDataCell(detail.Nomenclature?.Name ?? string.Empty, JustificationValues.Left));
                row.Append(CreateDataCell(detail.Nomenclature?.Article ?? string.Empty, JustificationValues.Center));
                row.Append(CreateDataCell(detail.Nomenclature?.Unit?.Name ?? "шт", JustificationValues.Center));
                row.Append(CreateDataCell(detail.Quantity.ToString("F3"), JustificationValues.Right));
                row.Append(CreateDataCell(detail.Price.ToString("F2"), JustificationValues.Right));
                row.Append(CreateDataCell(detail.Sum.ToString("F2"), JustificationValues.Right));
                row.Append(CreateDataCell("Без НДС", JustificationValues.Center));
                row.Append(CreateDataCell(detail.Sum.ToString("F2"), JustificationValues.Right));

                dataTable.Append(row);

                totalQuantity += detail.Quantity;
                totalSum += detail.Sum;
                index++;
            }

            var totalRow = new TableRow();
            totalRow.Append(CreateDataCell("Итого", JustificationValues.Right, 4, true));
            totalRow.Append(CreateDataCell(totalQuantity.ToString("F3"), JustificationValues.Right, 1, true));
            totalRow.Append(CreateDataCell("X", JustificationValues.Center, 1, true));
            totalRow.Append(CreateDataCell(totalSum.ToString("F2"), JustificationValues.Right, 1, true));
            totalRow.Append(CreateDataCell("X", JustificationValues.Center, 1, true));
            totalRow.Append(CreateDataCell(totalSum.ToString("F2"), JustificationValues.Right, 1, true));
            dataTable.Append(totalRow);

            body.Append(dataTable);
            body.Append(new Paragraph(new Run(new Text(""))));

            var footerTable = new Table();
            footerTable.AppendChild(new TableProperties(new TableWidth { Type = TableWidthUnitValues.Pct, Width = "5000" }));

            var fTr1 = new TableRow();
            fTr1.Append(CreateCell("Всего отпущено на сумму:", true));
            fTr1.Append(CreateCell(totalSum.ToString("F2") + " руб.", false));
            footerTable.Append(fTr1);

            var fTr2 = new TableRow();
            fTr2.Append(CreateCell("Отпуск груза произвел:", true));
            fTr2.Append(CreateCell("____________________ / ____________________ /", false));
            footerTable.Append(fTr2);

            var fTr3 = new TableRow();
            fTr3.Append(CreateCell("Груз принял грузополучатель:", true));
            fTr3.Append(CreateCell("____________________ / ____________________ /", false));
            footerTable.Append(fTr3);

            body.Append(footerTable);

            mainPart.Document.Save();
        });
    }

    private TableCell CreateCell(string text, bool isBold)
    {
        var cell = new TableCell();
        var para = new Paragraph();
        var run = new Run(new Text(text));
        if (isBold) run.RunProperties = new RunProperties(new Bold());
        para.Append(run);
        cell.Append(para);
        return cell;
    }

    private TableCell CreateDataCell(string text, JustificationValues align, int gridSpan = 1, bool isBold = false)
    {
        var cell = new TableCell();
        if (gridSpan > 1)
        {
            var cellProps = new TableCellProperties();
            cellProps.Append(new GridSpan { Val = gridSpan });
            cell.Append(cellProps);
        }
        var para = new Paragraph(new ParagraphProperties(new Justification { Val = align }));
        var run = new Run(new Text(text));
        run.RunProperties = new RunProperties(new FontSize { Val = "16" });
        if (isBold) run.RunProperties.Append(new Bold());
        para.Append(run);
        cell.Append(para);
        return cell;
    }
}