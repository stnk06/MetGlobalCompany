namespace MetGlobalCompany.Application.Interfaces;

/// <summary>
/// Интерфейс для получения информации о текущем авторизованном пользователе.
/// Необходим для корректной работы паттерна AuditableEntity на уровне DbContext.
/// </summary>
public interface ICurrentUserService
{
    string UserId { get; }
}