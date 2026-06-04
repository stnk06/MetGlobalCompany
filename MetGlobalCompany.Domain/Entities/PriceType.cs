using MetGlobalCompany.Domain.Common;

namespace MetGlobalCompany.Domain.Entities;

public class PriceType : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "RUB";
    public bool IsIncludesVat { get; set; }
}