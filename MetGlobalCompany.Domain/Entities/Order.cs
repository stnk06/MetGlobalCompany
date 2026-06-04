using System.Collections.Generic;
using MetGlobalCompany.Domain.Common;

namespace MetGlobalCompany.Domain.Entities;

/// <summary>
/// Документ: Заказ клиента.
/// Заказ не делает движений по регистрам, он служит основанием для Реализации.
/// </summary>
public class Order : BaseDocument
{
    public int ContractorId { get; set; }
    public virtual Contractor Contractor { get; set; } = null!;

    public int ContractId { get; set; }
    public virtual Contract Contract { get; set; } = null!;

    public string Status { get; set; } = "Новый";

    public decimal TotalAmount { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}