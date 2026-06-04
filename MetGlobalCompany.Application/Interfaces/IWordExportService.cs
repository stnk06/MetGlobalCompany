using System.Threading.Tasks;
using MetGlobalCompany.Domain.Entities;

namespace MetGlobalCompany.Application.Interfaces;

public interface IWordExportService
{
    Task ExportTorg12Async(SalesInvoice invoice, string filePath);
}