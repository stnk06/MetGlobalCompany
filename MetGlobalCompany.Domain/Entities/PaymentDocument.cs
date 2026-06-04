using MetGlobalCompany.Domain.Common;
using MetGlobalCompany.Domain.Enums;

namespace MetGlobalCompany.Domain.Entities;

public class PaymentDocument : BaseDocument
{
    public int ContractorId { get; set; }
    public virtual Contractor Contractor { get; set; } = null!;

    public int? ContractId { get; set; }
    public virtual Contract? Contract { get; set; }

    public decimal Amount { get; set; }
    public PaymentType Type { get; set; }
    public string Purpose { get; set; } = string.Empty;
}