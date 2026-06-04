using MetGlobalCompany.Application.Interfaces;

namespace MetGlobalCompany.WPF.Services;

/// <summary>
/// Реализация сервиса для получения текущего пользователя.
/// В реальном приложении здесь будет логика извлечения ID авторизованного пользователя.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    public string UserId => "SystemAdmin"; // Временно возвращаем статический ID
}