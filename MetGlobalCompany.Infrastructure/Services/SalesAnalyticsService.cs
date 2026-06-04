using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MetGlobalCompany.Application.DTOs.Analytics;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetGlobalCompany.Infrastructure.Services;

public class SalesAnalyticsService : ISalesAnalyticsService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public SalesAnalyticsService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<SalesDashboardDto> GetSalesDashboardAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.SalesInvoices
            .AsNoTracking()
            .Where(x => x.IsPosted);

        if (startDate.HasValue) query = query.Where(x => x.Date >= startDate.Value.Date);
        if (endDate.HasValue)
        {
            var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.Date <= endOfDay);
        }

        var dashboard = new SalesDashboardDto();

        var baseMetrics = await query
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalRevenue = g.Sum(x => x.TotalAmount),
                TotalInvoices = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (baseMetrics != null)
        {
            dashboard.TotalRevenue = baseMetrics.TotalRevenue;
            dashboard.TotalInvoices = baseMetrics.TotalInvoices;
            dashboard.AverageReceipt = baseMetrics.TotalInvoices > 0 ? baseMetrics.TotalRevenue / baseMetrics.TotalInvoices : 0;
        }

        var detailQuery = context.SalesInvoiceDetails
            .AsNoTracking()
            .Where(x => x.SalesInvoice.IsPosted);

        if (startDate.HasValue) detailQuery = detailQuery.Where(x => x.SalesInvoice.Date >= startDate.Value.Date);
        if (endDate.HasValue)
        {
            var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
            detailQuery = detailQuery.Where(x => x.SalesInvoice.Date <= endOfDay);
        }

        dashboard.TotalItemsSold = await detailQuery.SumAsync(x => x.Quantity, cancellationToken);

        var topNomenclatures = await detailQuery
            .GroupBy(x => new { x.NomenclatureId, x.Nomenclature.Name, x.Nomenclature.Article })
            .Select(g => new TopNomenclatureDto
            {
                NomenclatureName = g.Key.Name,
                Article = g.Key.Article ?? string.Empty,
                QuantitySold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Sum)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(15)
            .ToListAsync(cancellationToken);

        if (dashboard.TotalRevenue > 0)
        {
            foreach (var item in topNomenclatures) item.PercentageOfTotal = (item.Revenue / dashboard.TotalRevenue) * 100;
        }
        dashboard.TopNomenclatures = topNomenclatures;

        var topContractors = await query
            .GroupBy(x => new { x.ContractorId, x.Contractor.Name, x.Contractor.Inn })
            .Select(g => new TopContractorDto
            {
                ContractorName = g.Key.Name,
                Inn = g.Key.Inn ?? string.Empty,
                Revenue = g.Sum(x => x.TotalAmount),
                InvoicesCount = g.Count()
            })
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToListAsync(cancellationToken);

        if (dashboard.TotalRevenue > 0)
        {
            foreach (var contractor in topContractors) contractor.PercentageOfTotal = (contractor.Revenue / dashboard.TotalRevenue) * 100;
        }
        dashboard.TopContractors = topContractors;

        var salesByDate = await query
            .GroupBy(x => x.Date.Date)
            .Select(g => new DailySalesDto
            {
                Date = g.Key,
                Revenue = g.Sum(x => x.TotalAmount),
                InvoicesCount = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        dashboard.SalesByDate = salesByDate;

        return dashboard;
    }

    public async Task<SalesReportDto> GetHierarchicalSalesReportAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.SalesInvoiceDetails
            .Include(d => d.SalesInvoice)
            .ThenInclude(i => i.Contractor)
            .Include(d => d.Nomenclature)
            .AsNoTracking()
            .Where(d => d.SalesInvoice.IsPosted);

        if (startDate.HasValue) query = query.Where(d => d.SalesInvoice.Date >= startDate.Value.Date);
        if (endDate.HasValue)
        {
            var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(d => d.SalesInvoice.Date <= endOfDay);
        }

        var rawData = await query.ToListAsync(cancellationToken);

        var report = new SalesReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            GrandTotalQuantity = rawData.Sum(d => d.Quantity),
            GrandTotalSum = rawData.Sum(d => d.Sum)
        };

        var groupedByContractor = rawData
            .GroupBy(d => d.SalesInvoice.Contractor.Name)
            .OrderBy(g => g.Key);

        foreach (var contractorGroup in groupedByContractor)
        {
            var cGroupDto = new SalesReportContractorGroupDto
            {
                ContractorName = contractorGroup.Key,
                TotalQuantity = contractorGroup.Sum(d => d.Quantity),
                TotalSum = contractorGroup.Sum(d => d.Sum)
            };

            var groupedByNomenclature = contractorGroup
                .GroupBy(d => d.Nomenclature.Name)
                .OrderBy(g => g.Key);

            foreach (var nomGroup in groupedByNomenclature)
            {
                var nGroupDto = new SalesReportNomenclatureGroupDto
                {
                    NomenclatureName = nomGroup.Key,
                    TotalQuantity = nomGroup.Sum(d => d.Quantity),
                    TotalSum = nomGroup.Sum(d => d.Sum)
                };

                var items = nomGroup
                    .OrderBy(d => d.SalesInvoice.Date)
                    .Select(d => new SalesReportItemDto
                    {
                        DocumentDate = d.SalesInvoice.Date,
                        DocumentNumber = d.SalesInvoice.Number,
                        Quantity = d.Quantity,
                        Price = d.Price,
                        Sum = d.Sum
                    });

                nGroupDto.Items.AddRange(items);
                cGroupDto.NomenclatureGroups.Add(nGroupDto);
            }

            report.ContractorGroups.Add(cGroupDto);
        }

        return report;
    }
}