using System;
using MetGlobalCompany.Domain.Common;

namespace MetGlobalCompany.Domain.Entities;

public class PriceLedger : BaseEntity
{
    public DateTime Period { get; set; }
    public int RegistrarId { get; set; }

    public int NomenclatureId { get; set; }
    public virtual Nomenclature Nomenclature { get; set; } = null!;

    public int PriceTypeId { get; set; }
    public virtual PriceType PriceType { get; set; } = null!;

    public decimal Price { get; set; }
}