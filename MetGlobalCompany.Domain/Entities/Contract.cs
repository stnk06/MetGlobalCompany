using System;
using MetGlobalCompany.Domain.Common;

namespace MetGlobalCompany.Domain.Entities;

public class Contract : AuditableEntity
{
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "RUB";
    public bool IsActive { get; set; }

    public int ContractorId { get; set; }
    public virtual Contractor Contractor { get; set; } = null!;

    public int? PriceTypeId { get; set; }
    public virtual PriceType? PriceType { get; set; }
}