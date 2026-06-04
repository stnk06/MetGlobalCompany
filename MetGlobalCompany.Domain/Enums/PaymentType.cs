using System.ComponentModel;

namespace MetGlobalCompany.Domain.Enums;

public enum PaymentType
{
    [Description("Входящий платеж")]
    Incoming = 1,

    [Description("Исходящий платеж")]
    Outgoing = 2
}