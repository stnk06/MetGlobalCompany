namespace MetGlobalCompany.Domain.Common;

/// <summary>
/// Базовый класс для всех сущностей в системе, содержащий первичный ключ.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
}