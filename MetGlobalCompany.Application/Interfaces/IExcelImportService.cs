using System.Threading;
using System.Threading.Tasks;
using MetGlobalCompany.Application.DTOs;

namespace MetGlobalCompany.Application.Interfaces;

public interface IExcelImportService
{
    Task<ImportReportDto> ImportUnitsAsync(string filePath, CancellationToken cancellationToken = default);
    Task<ImportReportDto> ImportPriceTypesAsync(string filePath, CancellationToken cancellationToken = default);
    Task<ImportReportDto> ImportContractorsAsync(string filePath, CancellationToken cancellationToken = default);
    Task<ImportReportDto> ImportNomenclaturesAsync(string filePath, CancellationToken cancellationToken = default);
    Task<ImportReportDto> ImportContractsAsync(string filePath, CancellationToken cancellationToken = default);
    Task<ImportReportDto> ImportPriceSettingsAsync(string filePath, CancellationToken cancellationToken = default);
    Task<ImportReportDto> ImportPaymentsAsync(string filePath, CancellationToken cancellationToken = default);
}