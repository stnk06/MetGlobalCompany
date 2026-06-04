using System;
using MetGlobalCompany.Domain.Common;
using MetGlobalCompany.Domain.Enums;

namespace MetGlobalCompany.Domain.Entities;

/// <summary>
/// Регистр накопления: Взаиморасчеты с контрагентами.
/// Приход - увеличение нашего долга перед ними (или оплата от нас).
/// Расход - увеличение их долга перед нами (отгрузка).
/// В 1С это называется активный/пассивный регистр, здесь реализуем базовую логику.
/// </summary>
public class SettlementLedger : BaseEntity
{
    public DateTime Period { get; set; }

    public string RegistrarName { get; set; } = string.Empty;

    public int RegistrarId { get; set; }

    public int ContractorId { get; set; }
    public virtual Contractor Contractor { get; set; } = null!;

    public int ContractId { get; set; }
    public virtual Contract Contract { get; set; } = null!;

    public MovementType MovementType { get; set; }

    public decimal Amount { get; set; }
}