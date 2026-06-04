using System;
using System.Threading;
using System.Threading.Tasks;

namespace MetGlobalCompany.Application.Interfaces;

public interface IPriceService
{
    Task<decimal> GetPriceAsync(int nomenclatureId, int priceTypeId, DateTime onDate, CancellationToken cancellationToken = default);
}