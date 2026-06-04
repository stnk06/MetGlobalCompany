using System;

namespace MetGlobalCompany.Domain.Common;

/// <summary>
/// Базовый класс для сущностей, требующих аудита (кто и когда создал/изменил запись).
/// Обязательный паттерн для Enterprise ERP-систем.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }
}