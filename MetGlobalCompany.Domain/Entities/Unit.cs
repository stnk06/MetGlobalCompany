using MetGlobalCompany.Domain.Common;

namespace MetGlobalCompany.Domain.Entities;

/// <summary>
/// Справочник Единиц Измерения (ОКЕИ).
/// </summary>
public class Unit : AuditableEntity
{
    public string Code { get; set; } = string.Empty; // Код по ОКЕИ (например, 796)

    public string Name { get; set; } = string.Empty; // Штука, Тонна, Метр

    public string ShortName { get; set; } = string.Empty; // шт, т, м
}