using System;
using System.Collections.Generic;
using MetGlobalCompany.Domain.Common;

namespace MetGlobalCompany.Domain.Entities;

public class PurchaseInvoice : BaseDocument
{
    public int ContractorId { get; set; }
    public virtual Contractor Contractor { get; set; } = null!;

    public int ContractId { get; set; }
    public virtual Contract Contract { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public virtual ICollection<PurchaseInvoiceDetail> Details { get; set; } = new List<PurchaseInvoiceDetail>();
}