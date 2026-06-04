using MetGlobalCompany.Domain.Common;

namespace MetGlobalCompany.Domain.Entities;

/// <summary>
/// Справочник "Номенклатура" (Товары и Услуги). Заменяет старый Product.
/// </summary>
public class Nomenclature : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Article { get; set; } = string.Empty;

    // Связь с группой (Иерархия)
    public int CategoryId { get; set; }
    public virtual NomenclatureCategory Category { get; set; } = null!;

    // Связь с единицей измерения (ОКЕИ)
    public int UnitId { get; set; }
    public virtual Unit Unit { get; set; } = null!;

    // Специфичные реквизиты металлоторговли
    public string? Gost { get; set; }

    public string? Density { get; set; }

    public bool IsService { get; set; } // Флаг: Услуга (не хранится на складе) или Товар
}