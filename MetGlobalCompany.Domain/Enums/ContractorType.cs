using System.ComponentModel;

namespace MetGlobalCompany.Domain.Enums;

public enum ContractorType
{
    [Description("Покупатель")]
    Buyer = 1,
    [Description("Поставщик")]
    Supplier = 2,
    [Description("Покупатель и Поставщик одновременно")]
    Both = 3
}