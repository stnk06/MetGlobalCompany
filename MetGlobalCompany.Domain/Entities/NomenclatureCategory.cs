using System.Collections.Generic;
using MetGlobalCompany.Domain.Common;

namespace MetGlobalCompany.Domain.Entities;

/// <summary>
/// Иерархический справочник групп номенклатуры (Папки).
/// </summary>
public class NomenclatureCategory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    // Ссылка на родительскую категорию для построения дерева (Иерархия)
    public int? ParentId { get; set; }
    public virtual NomenclatureCategory? Parent { get; set; }

    // Навигационные свойства
    public virtual ICollection<NomenclatureCategory> Children { get; set; } = new List<NomenclatureCategory>();
    public virtual ICollection<Nomenclature> Nomenclatures { get; set; } = new List<Nomenclature>();
}