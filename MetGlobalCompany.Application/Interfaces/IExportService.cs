using System.Threading.Tasks;
using MetGlobalCompany.Application.DTOs.Analytics;
using MetGlobalCompany.Domain.Entities;

namespace MetGlobalCompany.Application.Interfaces;

public interface IExportService
{
    Task ExportSalesInvoiceToExcelAsync(SalesInvoice invoice, string filePath);
    Task ExportSalesReportToExcelAsync(SalesReportDto report, string filePath);
}