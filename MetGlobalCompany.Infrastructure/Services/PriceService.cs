using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetGlobalCompany.Infrastructure.Services;

public class PriceService : IPriceService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public PriceService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<decimal> GetPriceAsync(int nomenclatureId, int priceTypeId, DateTime onDate, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var priceRecord = await context.PriceLedgers
            .AsNoTracking()
            .Where(p => p.NomenclatureId == nomenclatureId && p.PriceTypeId == priceTypeId && p.Period <= onDate)
            .OrderByDescending(p => p.Period)
            .ThenByDescending(p => p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return priceRecord?.Price ?? 0m;
    }
}