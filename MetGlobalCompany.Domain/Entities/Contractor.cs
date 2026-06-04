using MetGlobalCompany.Domain.Common;
using MetGlobalCompany.Domain.Enums;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace MetGlobalCompany.Domain.Entities;

/// <summary>
/// Единый справочник Контрагентов (Заменяет разрозненные Customer и Supplier).
/// </summary>
public class Contractor : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public ContractorType Type { get; set; }

    // Юридические реквизиты
    public string? Inn { get; set; }

    public string? Kpp { get; set; }

    public string? Ogrn { get; set; }

    public string? LegalAddress { get; set; }

    public string? PhysicalAddress { get; set; }

    // Контактные данные
    public string? ContactPerson { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    // Навигационные свойства
    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}