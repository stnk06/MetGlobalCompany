using System;

namespace MetGlobalCompany.Domain.Common;

/// <summary>
/// Базовый класс для всех документов системы.
/// Содержит стандартные реквизиты шапки документа.
/// </summary>
public abstract class BaseDocument : AuditableEntity
{
    public string Number { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.Now;

    public bool IsPosted { get; set; }

    public string? Comment { get; set; }
}