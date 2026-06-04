using System.Threading;
using System.Threading.Tasks;
using MetGlobalCompany.Application.DTOs;

namespace MetGlobalCompany.Application.Interfaces;

/// <summary>
/// Интерфейс интеграции с внешним сервисом Dadata для автозаполнения реквизитов.
/// </summary>
public interface IDadataService
{
    /// <summary>
    /// Ищет контрагента по ИНН и возвращает его основные реквизиты.
    /// </summary>
    Task<DadataContractorDto?> GetContractorByInnAsync(string inn, CancellationToken cancellationToken = default);
}