using System;
using MetGlobalCompany.Domain.Common;
using MetGlobalCompany.Domain.Enums;

namespace MetGlobalCompany.Domain.Entities;

/// <summary>
/// Регистр накопления: Товары на складах.
/// Хранит все движения номенклатуры.
/// </summary>
public class InventoryLedger : BaseEntity
{
    public DateTime Period { get; set; } // Дата движения

    public string RegistrarName { get; set; } = string.Empty; // Имя документа (напр. "SalesInvoice")

    public int RegistrarId { get; set; } // ID документа

    public int NomenclatureId { get; set; }
    public virtual Nomenclature Nomenclature { get; set; } = null!;

    public MovementType MovementType { get; set; } // Приход/Расход

    public decimal Quantity { get; set; }
}