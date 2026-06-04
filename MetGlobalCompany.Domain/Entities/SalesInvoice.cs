using System.Collections.Generic;
using MetGlobalCompany.Domain.Common;

namespace MetGlobalCompany.Domain.Entities;

/// <summary>
/// Документ: Реализация товаров и услуг (УПД, Накладная).
/// При проведении списывает товары со склада и начисляет долг покупателю.
/// </summary>
public class SalesInvoice : BaseDocument
{
    public int ContractorId { get; set; }
    public virtual Contractor Contractor { get; set; } = null!;

    public int ContractId { get; set; }
    public virtual Contract Contract { get; set; } = null!;

    // Документ-основание (Опционально)
    public int? BaseOrderId { get; set; }
    public virtual Order? BaseOrder { get; set; }

    public decimal TotalAmount { get; set; }

    public virtual ICollection<SalesInvoiceDetail> Details { get; set; } = new List<SalesInvoiceDetail>();
}