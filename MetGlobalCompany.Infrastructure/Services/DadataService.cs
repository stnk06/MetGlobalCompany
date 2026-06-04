using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MetGlobalCompany.Application.DTOs;
using MetGlobalCompany.Application.Interfaces;

namespace MetGlobalCompany.Infrastructure.Services;

/// <summary>
/// Полная рабочая реализация клиента Dadata API.
/// </summary>
public class DadataService : IDadataService
{
    private readonly HttpClient _httpClient;

    private const string ApiKey = "7d1281dadb84382b0ad021d28f2dfcf2539b8f66";

    public DadataService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://suggestions.dadata.ru/suggestions/api/4_1/rs/findById/party");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", ApiKey);
    }

    public async Task<DadataContractorDto?> GetContractorByInnAsync(string inn, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(inn)) return null;

        var requestBody = new { query = inn };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("", requestBody, cancellationToken);

            if (!response.IsSuccessStatusCode) return null;

            var jsonResult = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(jsonResult);

            var suggestions = document.RootElement.GetProperty("suggestions");

            if (suggestions.GetArrayLength() == 0) return null;

            var firstResult = suggestions[0];
            var data = firstResult.GetProperty("data");
            var name = data.GetProperty("name");
            var address = data.GetProperty("address");

            return new DadataContractorDto
            {
                Inn = data.GetProperty("inn").GetString() ?? string.Empty,
                Kpp = data.TryGetProperty("kpp", out var kppProp) ? kppProp.GetString() ?? string.Empty : string.Empty,
                Ogrn = data.GetProperty("ogrn").GetString() ?? string.Empty,
                ShortName = name.GetProperty("short_with_opf").GetString() ?? string.Empty,
                FullName = name.GetProperty("full_with_opf").GetString() ?? string.Empty,
                LegalAddress = address.GetProperty("unrestricted_value").GetString() ?? string.Empty
            };
        }
        catch
        {

            return null;
        }
    }
}