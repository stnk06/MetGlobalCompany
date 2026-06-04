using System.ComponentModel;

namespace MetGlobalCompany.WPF.Enums;

public enum ImportTarget
{
    [Description("Единицы измерения")]
    Units,
    [Description("Типы цен")]
    PriceTypes,
    [Description("Контрагенты")]
    Contractors,
    [Description("Номенклатура")]
    Nomenclatures,
    [Description("Договоры")]
    Contracts,
    [Description("Установки цен")]
    PriceSettings,
    [Description("Платежи (Банк и касса)")]
    Payments
}