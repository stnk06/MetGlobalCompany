using System;
using System.Threading;
using System.Threading.Tasks;
using MetGlobalCompany.Application.DTOs.Analytics;

namespace MetGlobalCompany.Application.Interfaces;

public interface ISalesAnalyticsService
{
    Task<SalesDashboardDto> GetSalesDashboardAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    Task<SalesReportDto> GetHierarchicalSalesReportAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
}